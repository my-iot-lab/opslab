using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ops.Communication.Core;
using Ops.Communication.Profinet.Siemens;
using Ops.Exchange.Bus;
using Ops.Exchange.Configuration;
using Ops.Exchange.Handlers.Heartbeat;
using Ops.Exchange.Handlers.Notice;
using Ops.Exchange.Handlers.Reply;
using Ops.Exchange.Management;
using Ops.Exchange.Model;
using Ops.Exchange.Utils;

namespace Ops.Exchange.Monitors;

/// <summary>
/// 监控器管理对象。
/// </summary>
public sealed class MonitorManager : IDisposable
{
    private CancellationTokenSource? _cts = new();
    private bool _isRunning;

    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly DriverConnectorManager _driverConnectorManager;
    private readonly EventBus _eventBus;
    private readonly CallbackTaskQueueManager _callbackTaskQueue;
    private readonly OpsConfig _opsCofig;
    private readonly ILogger _logger;

    public MonitorManager(DeviceInfoManager deviceInfoManager,
        DriverConnectorManager driverConnectorManager,
        EventBus eventBus,
        CallbackTaskQueueManager callbackTaskQueue,
        IOptions<OpsConfig> opsConfig,
        ILogger<MonitorManager> logger)
    {
        _deviceInfoManager = deviceInfoManager;
        _driverConnectorManager = driverConnectorManager;
        _eventBus = eventBus;
        _callbackTaskQueue = callbackTaskQueue;
        _opsCofig = opsConfig.Value;
        _logger = logger;

        // TODO: 在其他地方注册
        _eventBus.Register<HeartbeatEventData, HeartbeatEventHandler>();
        _eventBus.Register<NoticeEventData, NoticeEventHandler>();
        _eventBus.Register<ReplyEventData, ReplyEventHandler>();
    }

    /// <summary>
    /// 启动监听
    /// </summary>
    public async Task StartAsync(MonitorStartOptions? startOptions = null)
    {
        if (_isRunning)
        {
            return;
        }
        _isRunning = true;

        _logger.LogInformation("[Monitor] 监控启动");

        if (_cts == null)
        {
            _cts = new();
        }

        await _driverConnectorManager.LoadAsync();
        await _driverConnectorManager.ConnectServerAsync();

        // 每个工站启用
        foreach (var connector in _driverConnectorManager.GetAllDriver())
        {
            // 心跳数据监控器
            _ = HeartbeatMonitorAsync(connector, startOptions?.HeartbeatDelegate);

            // 触发数据监控器
            _ = TriggerMonitorAsync(connector, startOptions?.TriggerDelegate);

            // 通知数据监控器
            _ = NoticeMonitorAsync(connector, startOptions?.NoticeDelegate);
        }

        // 数据回写，如心跳和触发数据（触发点和对应数据）
        _ = CallbackMonitorAsync();
    }

    private async Task HeartbeatMonitorAsync(DriverConnector connector, Action<HeartbeatEventData>? heartbeatDelegate = null)
    {
        var deviceInfo = await _deviceInfoManager.GetAsync(connector.Id);
        var variable = deviceInfo!.Variables.FirstOrDefault(s => s.Flag == VariableFlag.Heartbeat);
        if (variable == null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            int pollingInterval = variable.PollingInterval > 0 ? variable.PollingInterval : _opsCofig.Monitor.DefaultPollingInterval;
            while (!_cts!.Token.IsCancellationRequested)
            {
                await Task.Delay(pollingInterval, _cts.Token);

                var result = await connector.Driver.ReadInt16Async(variable.Address);
                if (result.IsSuccess && result.Content == 1)
                {
                    var context = new PayloadContext(new PayloadRequest(deviceInfo));
                    var eventData = new HeartbeatEventData(context, variable.Tag, result.Content);
                    heartbeatDelegate?.Invoke(eventData);
                    await _eventBus.Trigger(eventData, _cts.Token);
                }
            }
        });
    }

    private async Task TriggerMonitorAsync(DriverConnector connector, Action<ReplyEventData>? triggerDelegate = null)
    {
        var deviceInfo = await _deviceInfoManager.GetAsync(connector.Id);
        var variables = deviceInfo!.Variables.Where(s => s.Flag == VariableFlag.Trigger).ToList();
        foreach (var variable in variables)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = variable.PollingInterval > 0 ? variable.PollingInterval : _opsCofig.Monitor.DefaultPollingInterval;
                while (!_cts!.Token.IsCancellationRequested)
                {
                    await Task.Delay(pollingInterval, _cts.Token);

                    var result = await connector.Driver.ReadInt16Async(variable.Address);
                    if (result.IsSuccess && result.Content == ExStatusCode.Trigger)
                    {
                        var normalVariables = variable.NormalVariables;
                        List<PayloadData> datas = new(normalVariables.Count);

                        foreach (var normalVariable in normalVariables)
                        {
                            var (ok, data, _) = await ReadDataAsync(connector.Driver, normalVariable);
                            if (ok)
                            {
                                datas.Add(data);
                            }
                        }

                        var context = new PayloadContext(new PayloadRequest(deviceInfo));
                        var eventData = new ReplyEventData(context, variable.Tag, result.Content, datas.ToArray())
                        {
                            HandleTimeout = _opsCofig.Monitor.EventHandlerTimeout,
                        };
                        triggerDelegate?.Invoke(eventData);
                        await _eventBus.Trigger(eventData, _cts.Token);
                    }
                }
            });
        }
    }

    private async Task NoticeMonitorAsync(DriverConnector connector, Action<NoticeEventData>? noticeDelegate = null)
    {
        var deviceInfo = await _deviceInfoManager.GetAsync(connector.Id);
        var variables = deviceInfo!.Variables.Where(s => s.Flag == VariableFlag.Notice).ToList();
        foreach (var variable in variables)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = variable.PollingInterval > 0 ? variable.PollingInterval : _opsCofig.Monitor.DefaultPollingInterval;
                while (!_cts!.Token.IsCancellationRequested)
                {
                    await Task.Delay(pollingInterval, _cts.Token);

                    var (ok, data, _) = await ReadDataAsync(connector.Driver, variable);
                    if (ok)
                    {
                        var eventData = new NoticeEventData(GuidIdGenerator.NextId(), deviceInfo.Schema, variable.Tag, data)
                        {
                            HandleTimeout = _opsCofig.Monitor.EventHandlerTimeout,
                        };
                        noticeDelegate?.Invoke(eventData);
                        await _eventBus.Trigger(eventData, _cts.Token);
                    }
                }
            });
        }
    }

    private Task CallbackMonitorAsync()
    {
        return Task.Run(async () =>
        {
            while (!_cts!.Token.IsCancellationRequested)
            {
                var (ok, ctx) = await _callbackTaskQueue.WaitDequeueAsync(_cts.Token);
                if (!ok)
                {
                    break;
                }

                // 若连接驱动不处于连接状态，会循环等待。
                var connector = _driverConnectorManager[ctx!.Request.DeviceInfo.Name];
                while (!_cts.Token.IsCancellationRequested)
                {
                    if (connector.Status == ConnectingStatus.Connected)
                    {
                        break;
                    }

                    await Task.Delay(1000, _cts.Token); // 延迟1s
                    connector = _driverConnectorManager[ctx!.Request.DeviceInfo.Name];
                }

                PayloadData? value = null;
                try
                {
                    for (int i = 0; i < ctx.Response.Values.Count; i++)
                    {
                        value = ctx.Response.Values[i];
                        await WriteDataAsync(connector.Driver, value);
                    }

                    ctx.Response.LastAction?.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Monitor] 数据写入 PLC 出错，RequestId：{0}，工站：{1}, 触发点：{2}，数据：{3}",
                            ctx.Request.RequestId,
                            ctx.Request.DeviceInfo.Schema.Station,
                            value?.Tag ?? "",
                            value?.Value ?? "");
                }
            }
        });
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }
        _isRunning = false;

        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        _cts = null;

        // 阻塞 500ms
        Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult();
        _driverConnectorManager.Reset();

        _logger.LogInformation("[Monitor] 监控停止");
    }

    public void Dispose()
    {
        Stop();
    }

    private static async Task<(bool ok, PayloadData data, string err)> ReadDataAsync(IReadWriteNet driver, DeviceVariable deviceVariable)
    {
        var data = PayloadData.From(deviceVariable);

        switch (deviceVariable.VarType)
        {
            case VariableType.Bit:
                if (deviceVariable.Length > 0)
                {
                    var resultBit2 = await driver.ReadBoolAsync(deviceVariable.Address, (ushort)deviceVariable.Length);
                    SetValue(resultBit2, data);
                }
                else
                {
                    var resultBit1 = await driver.ReadBoolAsync(deviceVariable.Address);
                    SetValue(resultBit1, data);
                }
                break;
            case VariableType.Byte:
                break;
            case VariableType.Word:
                if (deviceVariable.Length > 0)
                {
                    var resultWord2 = await driver.ReadUInt16Async(deviceVariable.Address, (ushort)deviceVariable.Length);
                    SetValue(resultWord2, data);
                }
                else
                {
                    var resultWord1 = await driver.ReadUInt16Async(deviceVariable.Address);
                    SetValue(resultWord1, data);
                }
                break;
            case VariableType.DWord:
                if (deviceVariable.Length > 0)
                {
                    var resultUInt2 = await driver.ReadUInt32Async(deviceVariable.Address, (ushort)deviceVariable.Length);
                    SetValue(resultUInt2, data);
                }
                else
                {
                    var resultUInt1 = await driver.ReadUInt32Async(deviceVariable.Address);
                    SetValue(resultUInt1, data);
                }
                break;
            case VariableType.Int:
                if (deviceVariable.Length > 0)
                {
                    var resultInt2 = await driver.ReadInt16Async(deviceVariable.Address, (ushort)deviceVariable.Length);
                    SetValue(resultInt2, data);
                }
                else
                {
                    var resultInt1 = await driver.ReadInt16Async(deviceVariable.Address);
                    SetValue(resultInt1, data);
                }
                break;
            case VariableType.DInt:
                if (deviceVariable.Length > 0)
                {
                    var resultDInt2 = await driver.ReadInt32Async(deviceVariable.Address, (ushort)deviceVariable.Length);
                    SetValue(resultDInt2, data);
                }
                else
                {
                    var resultDInt1 = await driver.ReadInt32Async(deviceVariable.Address);
                    SetValue(resultDInt1, data);
                }
                break;
            case VariableType.Real:
                if (deviceVariable.Length > 0)
                {
                    var resultReal2 = await driver.ReadFloatAsync(deviceVariable.Address, (ushort)deviceVariable.Length);
                    SetValue(resultReal2, data);
                }
                else
                {
                    var resultReal1 = await driver.ReadFloatAsync(deviceVariable.Address);
                    SetValue(resultReal1, data);
                }
                break;
            case VariableType.LReal:
                if (deviceVariable.Length > 0)
                {
                    var resultLReal2 = await driver.ReadDoubleAsync(deviceVariable.Address, (ushort)deviceVariable.Length);
                    SetValue(resultLReal2, data);
                }
                else
                {
                    var resultLReal1 = await driver.ReadDoubleAsync(deviceVariable.Address);
                    SetValue(resultLReal1, data);
                }
                break;
            case VariableType.String or VariableType.S7String:
                var resultString2 = await driver.ReadStringAsync(deviceVariable.Address, (ushort)deviceVariable.Length);
                SetValue(resultString2, data);
                break;
            case VariableType.S7WString:
                if (driver is SiemensS7Net driver2)
                {
                    var resultWString2 = await driver2.ReadWStringAsync(deviceVariable.Address);
                    SetValue(resultWString2, data);
                }
                break;
            default:
                break;
        }

        return (true, data, "");

        static void SetValue<T>(Communication.OperateResult<T> result, PayloadData data)
        {
            if (result.IsSuccess)
            {
                data.Value = result.Content!;
            }
        }
    }

    private static async Task WriteDataAsync(IReadWriteNet driver, PayloadData data)
    {
        switch (data.VarType)
        {
            case VariableType.Bit:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2Array<bool>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Convert.ToBoolean(data.Value));
                }
                break;
            case VariableType.Byte:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2Array<byte>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Convert.ToByte(data.Value));
                }
                break;
            case VariableType.Word:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2Array<ushort>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Convert.ToUInt16(data.Value));
                }
                break;
            case VariableType.DWord:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2Array<uint>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Convert.ToUInt32(data.Value));
                }
                break;
            case VariableType.Int:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2Array<short>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Convert.ToInt16(data.Value));
                }
                break;
            case VariableType.DInt:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2Array<int>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Convert.ToInt32(data.Value));
                }
                break;
            case VariableType.Real:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2Array<float>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Convert.ToSingle(data.Value));
                }
                break;
            case VariableType.LReal:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2Array<double>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Convert.ToDouble(data.Value));
                }
                break;
            case VariableType.String or VariableType.S7String:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Convert.ToString(data.Value), data.Length);
                }
                else
                {
                    await driver.WriteAsync(data.Address, Convert.ToString(data.Value));
                }
                break;
            case VariableType.S7WString:
                if (driver is SiemensS7Net driver2)
                {
                    await driver2.WriteWStringAsync(data.Address, Convert.ToString(data.Value));
                }
                break;
            default:
                break;
        }
    }

    private static T[] Object2Array<T>(object obj)
    {
        if (obj is T[] obj2)
        {
            return obj2;
        }

        if (!obj.GetType().IsArray)
        {
            return Array.Empty<T>();
        }

        if (typeof(T) == typeof(bool))
        {
            var arr = (bool[])obj;
            return arr.Cast<T>().ToArray();
        }

        // 从宽类型转换为窄类型
        if (typeof(T) == typeof(byte) ||
            typeof(T) == typeof(ushort) ||
            typeof(T) == typeof(short) ||
            typeof(T) == typeof(uint) ||
            typeof(T) == typeof(int))
        {
            var arr = (int[])obj;
            return arr.Cast<T>().ToArray();
        }

        if (typeof(T) == typeof(float))
        {
            var arr = (float[])obj;
            return arr.Cast<T>().ToArray();
        }

        if (typeof(T) == typeof(double))
        {
            var arr = (double[])obj;
            return arr.Cast<T>().ToArray();
        }

        return Array.Empty<T>();
    }
}
