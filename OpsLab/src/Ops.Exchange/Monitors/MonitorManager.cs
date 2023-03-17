using Ops.Communication.Core;
using Ops.Communication.Profinet.Siemens;
using Ops.Exchange.Bus;
using Ops.Exchange.Configuration;
using Ops.Exchange.Handlers.Heartbeat;
using Ops.Exchange.Handlers.Notice;
using Ops.Exchange.Handlers.Reply;
using Ops.Exchange.Handlers.Switch;
using Ops.Exchange.Handlers.Underly;
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
    private MonitorStartOptions? _monitorStartOptions;

    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly DriverConnectorManager _driverConnectorManager;
    private readonly EventBus _eventBus;
    private readonly CallbackTaskQueueManager _callbackTaskQueue;
    private readonly OpsConfig _opsConfig;
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
        _opsConfig = opsConfig.Value;
        _logger = logger;

        // TODO: 在其他地方注册
        _eventBus.Register<HeartbeatEventData, HeartbeatEventHandler>();
        _eventBus.Register<NoticeEventData, NoticeEventHandler>();
        _eventBus.Register<ReplyEventData, ReplyEventHandler>();
        _eventBus.Register<SwitchEventData, SwitchEventHandler>();
        _eventBus.Register<UnderlyEventData, UnderlyEventHandler>();
    }

    /// <summary>
    /// 获取运行状态，是否正在运行中。
    /// </summary>
    public bool IsRuning { get; private set; }

    /// <summary>
    /// 启动监听
    /// </summary>
    /// <param name="startOptions">监听选项</param>
    public async Task StartAsync(MonitorStartOptions? startOptions = default)
    {
        if (IsRuning)
        {
            return;
        }
        IsRuning = true;

        _monitorStartOptions = startOptions ?? new MonitorStartOptions();
        _logger.LogInformation("[Monitor] 监控启动");

        _cts ??= new();

        await _driverConnectorManager.LoadAsync();
        await _driverConnectorManager.ConnectAsync();

        // 每个工站启用
        // TODO: 下面都使用线程池线程运行，若有长时间运行的任务过多，会不会导致线程中线程被消耗殆尽？
        //      或是考虑 Task.Factory.StartNew TaskCreationOptions.LongRunning，但这样每个任务都会创建一个新的独立线程（与线程池无关）。
        //      或是 可以采用 Actor 模式，可参考 https://mp.weixin.qq.com/s/hep-t5hhUngtiIHVCC9NdQ
        foreach (var connector in _driverConnectorManager.GetAllDriver())
        {
            // 心跳数据监控器
            _ = HeartbeatMonitorAsync(connector);

            // 触发数据监控器
            _ = TriggerMonitorAsync(connector);

            // 通知数据监控器
            _ = NoticeMonitorAsync(connector);

            // 开关数据监控器
            _ = SwitchMonitorAsync(connector);

            // 底层数据监控器
            _ = UnderlyMonitorAsync(connector);
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
            int pollingInterval = variable.PollingInterval > 0 ? variable.PollingInterval : _opsConfig.Monitor.DefaultPollingInterval;
            while (_cts != null && !_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(pollingInterval, _cts.Token);

                if (_cts != null)
                {
                    break;
                }

                if (!connector.CanConnect)
                {
                    continue;
                }

                try
                {
                    var result = await connector.Driver.ReadInt16Async(variable.Address);
                    if (!result.IsSuccess)
                    {
                        if (_monitorStartOptions!.IsLoggerHeartbeatError)
                        {
                            _logger.LogError($"[Monitor] Heartbeat 数据读取异常，工站：{deviceInfo.Schema.Station}，变量：{variable.Tag}, 地址：{variable.Address}，错误：{result.Message}");
                        }

                        continue;
                    }

                    if (result.Content == 1)
                    {
                        var context = new PayloadContext(new PayloadRequest(deviceInfo));
                        var eventData = new HeartbeatEventData(context, variable.Tag, result.Content);
                        _monitorStartOptions!.HeartbeatDelegate?.Invoke(eventData);
                        if (_cts != null)
                        {
                            await _eventBus.TriggerAsync(eventData, _cts.Token);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[Monitor] Heartbeat 数据处理异常，工站：{deviceInfo.Schema.Station}，变量：{variable.Tag}, 地址：{variable.Address}");
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
                int pollingInterval = variable.PollingInterval > 0 ? variable.PollingInterval : _opsConfig.Monitor.DefaultPollingInterval;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(pollingInterval, _cts.Token);

                    if (_cts == null)
                    {
                        break;
                    }

                    if (!connector.CanConnect)
                    {
                        continue;
                    }

                    try
                    {
                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var result = await connector.Driver.ReadInt16Async(variable.Address);
                        if (!result.IsSuccess)
                        {
                            if (_monitorStartOptions!.IsLoggerTriggerError)
                            {
                                _logger.LogError($"[Monitor] Trigger 数据读取异常，工站：{deviceInfo.Schema.Station}，变量：{variable.Tag}, 地址：{variable.Address}，错误：{result.Message}");
                            }

                            continue;
                        }

                        if (result.Content == ExStatusCode.Trigger)
                        {
                            // TODO: 此处校验是否能下发，若能，再去获取其明细数据。
                            // 由于数据处于触发状态的时间很短（取决于后端执行的速度），目前暂时可按获取全部数据来处理。

                            var normalVariables = variable.NormalVariables;
                            List<PayloadData> datas = new(normalVariables.Count);

                            bool ok = true;
                            foreach (var normalVariable in normalVariables)
                            {
                                // 若其中某一项异常，会终止此次后续流程，等下一次访问再执行。
                                var (ok1, data, err) = await ReadDataAsync(connector.Driver, normalVariable);
                                ok = ok1;

                                if (!ok)
                                {
                                    _logger.LogError($"[Monitor] Trigger 数据读取异常，工站：{deviceInfo.Schema.Station}，变量：{normalVariable.Tag} ，错误：{err}");
                                    break;
                                }

                                datas.Add(data);
                            }

                            if (!ok)
                            {
                                continue;
                            }

                            var context = new PayloadContext(new PayloadRequest(deviceInfo));
                            var eventData = new ReplyEventData(context, variable.Tag, variable.Name, result.Content, datas.ToArray())
                            {
                                HandleTimeout = _opsConfig.Monitor.EventHandlerTimeout,
                            };
                            _monitorStartOptions!.TriggerDelegate?.Invoke(eventData);
                            if (_cts != null)
                            {
                                await _eventBus.TriggerAsync(eventData, _cts.Token);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Monitor] Trigger 数据处理异常，工站：{deviceInfo.Schema.Station}，变量：{variable.Tag}, 地址：{variable.Address}");
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
                int pollingInterval = variable.PollingInterval > 0 ? variable.PollingInterval : _opsConfig.Monitor.DefaultPollingInterval;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(pollingInterval, _cts.Token);

                    if (_cts == null)
                    {
                        break;
                    }

                    if (!connector.CanConnect)
                    {
                        continue;
                    }

                    try
                    {
                        // 若读取异常，等到下一次再访问即可。
                        var (ok, data, err) = await ReadDataAsync(connector.Driver, variable);
                        if (!ok)
                        {
                            if (_monitorStartOptions!.IsLoggerNoticeError)
                            {
                                _logger.LogError($"[Monitor] Notice 数据读取异常，工站：{deviceInfo.Schema.Station}，变量：{variable.Tag}, 地址：{variable.Address}，错误：{err}");
                            }

                            continue;
                        }

                        var eventData = new NoticeEventData(GuidIdGenerator.NextId(), deviceInfo.Schema, variable.Tag, variable.Name, data)
                        {
                            HandleTimeout = _opsConfig.Monitor.EventHandlerTimeout,
                        };
                        _monitorStartOptions!.NoticeDelegate?.Invoke(eventData);
                        if (_cts != null)
                        {
                            await _eventBus.TriggerAsync(eventData, _cts.Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Monitor] Notice 数据处理异常，工站：{deviceInfo.Schema.Station}，变量：{variable.Tag}, 地址：{variable.Address}");
                    }
                }
            });
        }
    }

    private async Task SwitchMonitorAsync(DriverConnector connector)
    {
        var deviceInfo = await _deviceInfoManager.GetAsync(connector.Id);
        var variables = deviceInfo!.Variables.Where(s => s.Flag == VariableFlag.Switch).ToList();
        foreach (var variable in variables)
        {
            ManualResetEvent mre = new(false); // 手动事件
            _ = Task.Run(async () =>
            {
                int pollingInterval = _monitorStartOptions!.SwitchPollingInterval > 0 ? _monitorStartOptions!.SwitchPollingInterval : 10;
                while (mre.WaitOne() && _cts != null && !_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(pollingInterval, _cts.Token);

                    if (_cts == null)
                    {
                        break;
                    }

                    if (!connector.CanConnect)
                    {
                        continue;
                    }

                    try
                    {
                        var normalVariables = variable.NormalVariables;
                        List<PayloadData> datas = new(normalVariables.Count);

                        bool ok = false;
                        foreach (var normalVariable in normalVariables)
                        {
                            // 若其中某一项异常，会终止此次后续流程，等下一次访问再执行。
                            var (ok1, data, err) = await ReadDataAsync(connector.Driver, normalVariable);
                            ok = ok1;

                            if (!ok)
                            {
                                _logger.LogError($"[Monitor] Switch 数据读取异常，工站：{deviceInfo.Schema.Station}，变量：{normalVariable.Tag} ，错误：{err}");
                                break;
                            }

                            datas.Add(data);
                        }

                        if (!ok)
                        {
                            continue;
                        }

                        // 发送 On 信号
                        var eventData = new SwitchEventData(GuidIdGenerator.NextId(), deviceInfo.Schema, variable.Tag, variable.Name, SwitchEventData.StateOn, datas)
                        {
                            HandleTimeout = _opsConfig.Monitor.EventHandlerTimeout,
                        };
                        _monitorStartOptions.SwitchDelegate?.Invoke(eventData);
                        if (_cts != null)
                        {
                            await _eventBus.TriggerAsync(eventData, _cts.Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Monitor] Switch 数据处理异常，工站：{deviceInfo.Schema.Station}，变量：{variable.Tag}");
                    }
                }
            });

            _ = Task.Run(async () =>
            {
                int pollingInterval = variable.PollingInterval > 0 ? variable.PollingInterval : _opsConfig.Monitor.DefaultPollingInterval;
                bool isOn = false; // 开关开启状态
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(pollingInterval, _cts.Token);

                    if (_cts == null)
                    {
                        break;
                    }

                    if (!connector.CanConnect)
                    {
                        continue;
                    }

                    // 若读取失败，该信号点不会复位，下次会继续读取执行。
                    var result = await connector.Driver.ReadInt16Async(variable.Address);
                    if (result.IsSuccess)
                    {
                        if (result.Content == ExStatusCode.SwitchOn)
                        {
                            if (!isOn)
                            {
                                // 发送 Ready 信号
                                var eventData = new SwitchEventData(GuidIdGenerator.NextId(), deviceInfo.Schema, variable.Tag, variable.Name, SwitchEventData.StateReady)
                                {
                                    HandleTimeout = _opsConfig.Monitor.EventHandlerTimeout,
                                };

                                if (_cts != null)
                                {
                                    await _eventBus.TriggerAsync(eventData, _cts.Token);
                                }

                                // 开关开启时，发送信号，让子任务执行。
                                mre.Set(); 
                                isOn = true;
                            }
                        }
                        else
                        {
                            if (isOn)
                            {
                                // 发送 Off 信号
                                var eventData = new SwitchEventData(GuidIdGenerator.NextId(), deviceInfo.Schema, variable.Tag, variable.Name, SwitchEventData.StateOff)
                                {
                                    HandleTimeout = _opsConfig.Monitor.EventHandlerTimeout,
                                };

                                if (_cts != null)
                                {
                                    await _eventBus.TriggerAsync(eventData, _cts.Token);
                                }

                                // 读取失败或开关关闭时，重置信号，让子任务阻塞。
                                mre.Reset(); 
                                isOn = false;
                            }
                        }
                    }
                }

                // 任务取消后，发送信号，让子任务能退出任务
                if (!isOn)
                {
                    mre.Set();
                    isOn = true;
                }
            });
        }
    }

    private async Task UnderlyMonitorAsync(DriverConnector connector)
    {
        var deviceInfo = await _deviceInfoManager.GetAsync(connector.Id);
        var variables = deviceInfo!.Variables.Where(s => s.Flag == VariableFlag.Underly).ToList();
        foreach (var variable in variables)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = variable.PollingInterval > 0 ? variable.PollingInterval : _opsConfig.Monitor.DefaultPollingInterval;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(pollingInterval, _cts.Token);

                    if (_cts == null)
                    {
                        break;
                    }

                    if (!connector.CanConnect)
                    {
                        continue;
                    }

                    try
                    {
                        // 若读取异常，等到下一次再访问即可。
                        var (ok0, data0, err0) = await ReadDataAsync(connector.Driver, variable);
                        if (!ok0)
                        {
                            if (_monitorStartOptions!.IsLoggerUnderlyError)
                            {
                                _logger.LogError($"[Monitor] Underly 数据读取异常，工站：{deviceInfo.Schema.Station}，变量：{variable.Tag}, 地址：{variable.Address}，错误：{err0}");
                            }

                            continue;
                        }

                        var normalVariables = variable.NormalVariables;
                        List<PayloadData> datas = new(normalVariables.Count + 1);
                        datas.Add(data0);

                        bool ok = true;
                        foreach (var normalVariable in normalVariables)
                        {
                            // 若其中某一项异常，会终止此次后续流程，等下一次访问再执行。
                            var (ok1, data, err) = await ReadDataAsync(connector.Driver, normalVariable);
                            ok = ok1;

                            if (!ok)
                            {
                                _logger.LogError($"[Monitor] Underly 数据读取异常，工站：{deviceInfo.Schema.Station}，变量：{normalVariable.Tag} ，错误：{err}");
                                break;
                            }

                            datas.Add(data);
                        }

                        if (!ok)
                        {
                            continue;
                        }

                        var context = new PayloadContext(new PayloadRequest(deviceInfo));
                        var eventData = new UnderlyEventData(context, variable.Tag, variable.Name, datas.ToArray())
                        {
                            HandleTimeout = _opsConfig.Monitor.EventHandlerTimeout,
                        };
                        _monitorStartOptions!.UnderlyDelegate?.Invoke(eventData);
                        if (_cts != null)
                        {
                            await _eventBus.TriggerAsync(eventData, _cts.Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Monitor] Underly 数据处理异常，工站：{deviceInfo.Schema.Station}，变量：{variable.Tag}, 地址：{variable.Address}");
                    }
                }
            });
        }
    }

    private Task CallbackMonitorAsync()
    {
        return Task.Run(async () =>
        {
            while (_cts != null && !_cts.Token.IsCancellationRequested)
            {
                var (ok, ctx) = await _callbackTaskQueue.WaitDequeueAsync(_cts.Token);
                if (!ok)
                {
                    break;
                }

                try
                {
                    _ = Task.Run(async () =>
                    {
                        PayloadData? value = null;
                        try
                        {
                            // 若连接驱动不处于连接状态，会循环等待。
                            // TODO: 注：此处 Driver Connector 可能有被清空的情况出现，还需做一些其他的校验。
                            var connector = _driverConnectorManager[ctx!.Request.DeviceInfo.Name];
                            if (!connector.CanConnect)
                            {
                                // 若某个写入卡死，可能导致整个回写卡住，考虑在 Task.Run 中执行回写。
                                CancellationTokenSource cts1 = new(_opsConfig.Monitor.CallbackTimeout); // 回写超时
                                using var cts2 = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cts1.Token);
                                while (!cts2.Token.IsCancellationRequested)
                                {
                                    if (connector.CanConnect)
                                    {
                                        break;
                                    }

                                    await Task.Delay(1000); // 延迟1s
                                    connector = _driverConnectorManager[ctx.Request.DeviceInfo.Name];
                                }
                            }

                            for (int i = 0; i < ctx.Response.Values.Count; i++)
                            {
                                value = ctx.Response.Values[i];
                                var (ok, err) = await WriteDataAsync(connector.Driver, value); // 考虑某个值回写失败该如何处理
                                if (!ok)
                                {
                                    _logger.LogError("[Monitor] 数据写入 PLC 出错，RequestId：{0}，工站：{1}, 触发点：{2}，数据：{3}, 错误：{4}",
                                        ctx.Request.RequestId,
                                        ctx.Request.DeviceInfo.Schema.Station,
                                        value?.Tag ?? "",
                                        value?.Value ?? "",
                                        err);
                                }
                            }

                            ctx.Response.LastDelegate?.Invoke(); // TODO: 回写失败，状态机状态是否要继续设置？若是要重试，一定更改对应的状态。
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[Monitor] 数据写入 PLC 出错，RequestId：{0}，工站：{1}, 触发点：{2}，数据：{3}",
                                    ctx!.Request.RequestId,
                                    ctx.Request.DeviceInfo.Schema.Station,
                                    value?.Tag ?? "",
                                    value?.Value ?? "");
                        }
                    }, _cts.Token);
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

    /// <summary>
    /// 停止监控。
    /// </summary>
    public void Stop()
    {
        if (!IsRuning)
        {
            return;
        }
        IsRuning = false;

        if (_cts != null)
        {
            CancellationTokenSource cts = _cts;
            _cts = null;
            cts.Cancel();
            cts.Dispose();
        }

        Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult(); // 阻塞 500ms
        _driverConnectorManager.Close();

        _logger.LogInformation("[Monitor] 监控停止");
    }

    public void Dispose()
    {
        Stop();
    }

    private static async Task<(bool ok, PayloadData data, string err)> ReadDataAsync(IReadWriteNet driver, DeviceVariable deviceVariable)
    {
        bool ok = false;
        string errMessage = string.Empty;
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
                errMessage = "Not match VariableType";
                break;
        }

        return (ok, data, errMessage);

        void SetValue<T>(Communication.OperateResult<T> result, PayloadData data)
        {
            if (result.IsSuccess)
            {
                data.Value = result.Content!;
                ok = true;
            }
            else
            {
                ok = false;
                errMessage = result.Message;
            }
        }
    }

    private static async Task<(bool ok, string err)> WriteDataAsync(IReadWriteNet driver, PayloadData data)
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

        return (true, string.Empty);
    }
}
