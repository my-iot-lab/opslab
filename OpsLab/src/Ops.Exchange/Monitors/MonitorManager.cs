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
        await _driverConnectorManager.ConnectAsync();

        // 每个工站启用
        // TODO: 下面都使用线程池线程运行，若有长时间运行的任务过多，会不会导致线程中线程被消耗殆尽？
        //      或是考虑 Task.Factory.StartNew TaskCreationOptions.LongRunning，但这样每个任务都会创建一个新的独立线程（与线程池无关）。
        //      或是 可以采用 Actor 模式，可参考 https://mp.weixin.qq.com/s/hep-t5hhUngtiIHVCC9NdQ
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

                if (!connector.CanConnect)
                {
                    continue;
                }

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

                    if (!connector.CanConnect)
                    {
                        continue;
                    }

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

                    if (!connector.CanConnect)
                    {
                        continue;
                    }

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

                try
                {
                    // 若某个写入卡死，可能导致整个回写卡住，考虑在 Task.Run 中允许回写。
                    CancellationTokenSource cts1 = new(_opsCofig.Monitor.CallbackTimeout); // 回写超时
                    using var cts0 = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cts1.Token);
                    _ = Task.Run(async () =>
                    {
                        // 若连接驱动不处于连接状态，会循环等待。
                        // TODO: 注：此处 Driver Connector 可能有被清空的情况出现，还需做一些其他的校验。
                        var connector = _driverConnectorManager[ctx!.Request.DeviceInfo.Name];
                        while (!_cts.Token.IsCancellationRequested)
                        {
                            if (connector.CanConnect)
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

                            ctx.Response.LastDelegate?.Invoke(); // TODO: 回写失败，状态机状态是否要继续设置？若是要重试，一定更改对应的状态。
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[Monitor] 数据写入 PLC 出错，RequestId：{0}，工站：{1}, 触发点：{2}，数据：{3}",
                                    ctx.Request.RequestId,
                                    ctx.Request.DeviceInfo.Schema.Station,
                                    value?.Tag ?? "",
                                    value?.Value ?? "");
                        }
                    }, cts0.Token);
                }
                catch (Exception ex) when (ex is OperationCanceledException)
                {
                    _logger.LogError(ex, "[Monitor] 数据写入 PLC 超时取消，RequestId：{0}，工站：{1}",
                                    ctx!.Request.RequestId,
                                    ctx!.Request.DeviceInfo.Schema.Station);
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
            _cts.Cancel(); // 阻塞 500ms
            _cts.Dispose();
            _cts = null;
        }

        Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult();
        _driverConnectorManager.Close();

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
                if (deviceVariable.Length > 0)
                {
                    var resultBit2 = await driver.ReadAsync(deviceVariable.Address, (ushort)deviceVariable.Length);
                    SetValue(resultBit2, data);
                }
                else
                {
                    var resultBit1 = await driver.ReadAsync(deviceVariable.Address, 1);
                    SetValue(resultBit1, data);
                }
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
                if (driver is SiemensS7Net driver1)
                {
                    var resultString1 = await driver1.ReadStringAsync(deviceVariable.Address); // S7 自动计算长度
                    SetValue(resultString1, data);
                } 
                else
                {
                    var resultString2 = await driver.ReadStringAsync(deviceVariable.Address, (ushort)deviceVariable.Length);
                    SetValue(resultString2, data);
                }
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
                    await driver.WriteAsync(data.Address, Object2ValueHelper.ToArray<bool>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.To<bool>(data.Value));
                }
                break;
            case VariableType.Byte:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.ToArray<byte>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.To<byte>(data.Value));
                }
                break;
            case VariableType.Word:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.ToArray<ushort>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.To<ushort>(data.Value));
                }
                break;
            case VariableType.DWord:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.ToArray<uint>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.To<uint>(data.Value));
                }
                break;
            case VariableType.Int:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.ToArray<short>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.To<short>(data.Value));
                }
                break;
            case VariableType.DInt:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.ToArray<int>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.To<int>(data.Value));
                }
                break;
            case VariableType.Real:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.ToArray<float>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.To<float>(data.Value));
                }
                break;
            case VariableType.LReal:
                if (data.Length > 0)
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.ToArray<double>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.To<double>(data.Value));
                }
                break;
            case VariableType.String or VariableType.S7String:
                await driver.WriteAsync(data.Address, Object2ValueHelper.To<string>(data.Value));
                break;
            case VariableType.S7WString:
                if (driver is SiemensS7Net driver2)
                {
                    await driver2.WriteWStringAsync(data.Address, Object2ValueHelper.To<string>(data.Value));
                }
                else
                {
                    await driver.WriteAsync(data.Address, Object2ValueHelper.To<string>(data.Value));
                }
                break;
            default:
                break;
        }
    }
}
