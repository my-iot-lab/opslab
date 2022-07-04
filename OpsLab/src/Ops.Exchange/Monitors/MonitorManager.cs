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
    private readonly CancellationTokenSource _cts = new();
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

        _eventBus.Register<HeartbeatEventData, HeartbeatEventHandler>();
        _eventBus.Register<NoticeEventData, NoticeEventHandler>();
        _eventBus.Register<ReplyEventData, ReplyEventHandler>();
    }

    /// <summary>
    /// 启动监听
    /// </summary>
    public async Task StartAsync()
    {
        if (_isRunning)
        {
            return;
        }
        _isRunning = true;

        await _driverConnectorManager.LoadAsync();
        await _driverConnectorManager.ConnectServerAsync();

        // 每个工站启用
        foreach (var connector in _driverConnectorManager.GetAllDriver())
        {
            // 心跳数据监控器
            _ = HeartbeatMonitorAsync(connector);

            // 触发数据监控器
            _ = TriggerMonitorAsync(connector);

            // 通知数据监控器
            _ = NoticeMonitorAsync(connector);
        }

        // 数据回写，如心跳和触发数据（触发点和对应数据）
        _ = CallbackMonitorAsync();
    }

    private async Task HeartbeatMonitorAsync(DriverConnector connector)
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
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(pollingInterval, _cts.Token);

                var result = await connector.Driver.ReadInt16Async(variable.Address);
                if (result.IsSuccess && result.Content == 1)
                {
                    var context = new PayloadContext(new PayloadRequest(deviceInfo));
                    var eventData = new HeartbeatEventData(context, variable.Tag, result.Content);
                    await _eventBus.Trigger(eventData, _cts.Token);
                }
            }
        });
    }

    private async Task TriggerMonitorAsync(DriverConnector connector)
    {
        var deviceInfo = await _deviceInfoManager.GetAsync(connector.Id);
        var variables = deviceInfo!.Variables.Where(s => s.Flag == VariableFlag.Trigger).ToList();
        foreach (var variable in variables)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = variable.PollingInterval > 0 ? variable.PollingInterval : _opsCofig.Monitor.DefaultPollingInterval;
                while (!_cts.Token.IsCancellationRequested)
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
                        await _eventBus.Trigger(eventData, _cts.Token);
                    }
                }
            });
        }
    }

    private async Task NoticeMonitorAsync(DriverConnector connector)
    {
        var deviceInfo = await _deviceInfoManager.GetAsync(connector.Id);
        var variables = deviceInfo!.Variables.Where(s => s.Flag == VariableFlag.Notice).ToList();
        foreach (var variable in variables)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = variable.PollingInterval > 0 ? variable.PollingInterval : _opsCofig.Monitor.DefaultPollingInterval;
                while (!_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(pollingInterval, _cts.Token);

                    var (ok, data, _) = await ReadDataAsync(connector.Driver, variable);
                    if (ok)
                    {
                        var eventData = new NoticeEventData(GuidIdGenerator.NextId(), deviceInfo.Schema, variable.Tag, data)
                        {
                            HandleTimeout = _opsCofig.Monitor.EventHandlerTimeout,
                        };
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
            while (!_cts.Token.IsCancellationRequested)
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

                foreach (var value in ctx.Response.Values)
                {
                    await WriteDataAsync(connector.Driver, value);
                }

                ctx.Response.LastAction?.Invoke();
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

        _cts.Cancel();

        // 阻塞 500ms
        Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult();
        _driverConnectorManager.Reset();

        _logger.LogInformation("监控停止");
    }

    public void Dispose()
    {
        Stop();
    }

    private async Task<(bool ok, PayloadData data, string err)> ReadDataAsync(IReadWriteNet driver, DeviceVariable deviceVariable)
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

    private async Task WriteDataAsync(IReadWriteNet driver, PayloadData data)
    {
        switch (data.VarType)
        {
            case VariableType.Bit:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, (bool[])data.Value);
                }
                else
                {
                    await driver.WriteAsync(data.Address, (bool)data.Value);
                }
                break;
            case VariableType.Byte:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, (byte[])data.Value);
                }
                else
                {
                    await driver.WriteAsync(data.Address, (byte)data.Value);
                }
                break;
            case VariableType.Word:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, (ushort[])data.Value);
                }
                else
                {
                    await driver.WriteAsync(data.Address, (ushort)data.Value);
                }
                break;
            case VariableType.DWord:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, (uint[])data.Value);
                }
                else
                {
                    await driver.WriteAsync(data.Address, (uint)data.Value);
                }
                break;
            case VariableType.Int:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, (short[])data.Value);
                }
                else
                {
                    await driver.WriteAsync(data.Address, (short)data.Value);
                }
                break;
            case VariableType.DInt:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, (int[])data.Value);
                }
                else
                {
                    await driver.WriteAsync(data.Address, (int)data.Value);
                }
                break;
            case VariableType.Real:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, (float[])data.Value);
                }
                else
                {
                    await driver.WriteAsync(data.Address, (float)data.Value);
                }
                break;
            case VariableType.LReal:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, (double[])data.Value);
                }
                else
                {
                    await driver.WriteAsync(data.Address, (double)data.Value);
                }
                break;
            case VariableType.String or VariableType.S7String:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, (string)data.Value, data.Length);
                }
                else
                {
                    await driver.WriteAsync(data.Address, (string)data.Value);
                }
                break;
            case VariableType.S7WString:
                if (driver is SiemensS7Net driver2)
                {
                    await driver2.WriteWStringAsync(data.Address, (string)data.Value);
                }
                break;
            default:
                break;
        }
    }
}
