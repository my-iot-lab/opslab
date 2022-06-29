using Microsoft.Extensions.Logging;
using Ops.Communication.Core;
using Ops.Communication.Core.Net;
using Ops.Communication.Modbus;
using Ops.Communication.Profinet.AllenBradley;
using Ops.Communication.Profinet.Omron;
using Ops.Communication.Profinet.Siemens;
using Ops.Exchange.Bus;
using Ops.Exchange.Handlers.Heartbeat;
using Ops.Exchange.Handlers.Notice;
using Ops.Exchange.Handlers.Reply;
using Ops.Exchange.Management;
using Ops.Exchange.Model;
using Ops.Exchange.Stateless;

namespace Ops.Exchange.Monitors;

/// <summary>
/// 监控器管理对象。
/// </summary>
public sealed class MonitorManager : IDisposable
{
    private readonly Dictionary<long, IReadWriteNet> _drivers = new();
    private readonly CancellationTokenSource _cts = new();

    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly EventBus _eventBus;
    private readonly ILogger _logger;

    public MonitorManager(DeviceInfoManager deviceInfoManager, EventBus eventBus, ILogger<MonitorManager> logger)
    {
        _deviceInfoManager = deviceInfoManager;
        _eventBus = eventBus;
        _logger = logger;
    }

    /// <summary>
    /// 启动监听
    /// </summary>
    public async Task Start()
    {
        var deviceInfos = await _deviceInfoManager.GetAllAsync();
        foreach (var deviceInfo in deviceInfos)
        {
            var schema = deviceInfo.Schema;
            NetworkDeviceBase driverNet = schema.DriverModel switch
            {
                DriverModel.ModbusTcp => new ModbusTcpNet(schema.Host),
                DriverModel.S7_1500 => new SiemensS7Net(SiemensPLCS.S1500, schema.Host),
                DriverModel.S7_1200 => new SiemensS7Net(SiemensPLCS.S1200, schema.Host),
                DriverModel.S7_400 => new SiemensS7Net(SiemensPLCS.S400, schema.Host),
                DriverModel.S7_300 => new SiemensS7Net(SiemensPLCS.S300, schema.Host),
                DriverModel.S7_S200 => new SiemensS7Net(SiemensPLCS.S200, schema.Host),
                DriverModel.S7_S200Smart => new SiemensS7Net(SiemensPLCS.S200Smart, schema.Host),
                DriverModel.Omron_FinsTcp => new OmronFinsNet(schema.Host, schema.Port),
                DriverModel.AllenBradley_CIP => new AllenBradleyNet(schema.Host),
                _ => throw new NotImplementedException(),
            };

            // 设置 SocketKeepAliveTime 心跳时间
            driverNet.SocketKeepAliveTime = 60_000;
            _drivers.Add(deviceInfo.Id, driverNet);
        }

        foreach (var driver in _drivers)
        {
            await (driver.Value as NetworkDeviceBase)!.ConnectServerAsync();
        }

        // 每个工站启用
        foreach (var driver in _drivers)
        {
            var deviceInfo = deviceInfos.Single(s => s.Id == driver.Key);

            // 心跳只有一个标志位
            var heartbeatDevInfo = deviceInfo.Variables.FirstOrDefault(s => s.Flag == VariableFlag.Heartbeat);
            if (heartbeatDevInfo != null)
            {
                _ = Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(heartbeatDevInfo.PollingInterval, _cts.Token);

                        var result = await driver.Value.ReadInt32Async(heartbeatDevInfo.Address);
                        if (result.IsSuccess && result.Content == 1)
                        {
                            var eventData = new HeartbeatEventData(deviceInfo.Schema, heartbeatDevInfo.Tag, result.Content);
                            await _eventBus.Trigger(eventData, _cts.Token);
                        }
                    }
                });
            }

            var triggerDevInfos = deviceInfo.Variables.Where(s => s.Flag == VariableFlag.Trigger).ToList();
            foreach (var triggerDevInfo in triggerDevInfos)
            {
                _ = Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(triggerDevInfo.PollingInterval, _cts.Token);

                        var result = await driver.Value.ReadInt32Async(triggerDevInfo.Address);
                        if (result.IsSuccess && result.Content == StateConstant.CanTransfer)
                        {
                            var normalVariables = triggerDevInfo.NormalVariables;
                            if (normalVariables.Any())
                            {
                                foreach (var normalVariable in normalVariables)
                                {
                                    switch (normalVariable.VarType)
                                    {
                                        case VariableType.Bit:
                                            var resultBit21 = await driver.Value.ReadBoolAsync(normalVariable.Address);
                                            if (normalVariable.Length > 0)
                                            {
                                                var resultBit22 = await driver.Value.ReadBoolAsync(normalVariable.Address, (ushort)normalVariable.Length);
                                            }
                                            break;
                                        case VariableType.Byte:
                                            break;
                                        case VariableType.Word:
                                            var resultWord21 = await driver.Value.ReadUInt16Async(normalVariable.Address);
                                            if (normalVariable.Length > 0)
                                            {
                                                var resultWord22 = await driver.Value.ReadUInt16Async(normalVariable.Address, (ushort)normalVariable.Length);
                                            }
                                            break;
                                        case VariableType.DWord:
                                            break;
                                        case VariableType.Int:
                                            var resultInt21 = await driver.Value.ReadInt16Async(normalVariable.Address);
                                            if (normalVariable.Length > 0)
                                            {
                                                var resultInt22 = await driver.Value.ReadInt16Async(normalVariable.Address, (ushort)normalVariable.Length);
                                            }
                                            break;
                                        case VariableType.DInt:
                                            var resultDInt21 = await driver.Value.ReadInt32Async(normalVariable.Address);
                                            if (normalVariable.Length > 0)
                                            {
                                                var resultDInt22 = await driver.Value.ReadInt32Async(normalVariable.Address, (ushort)normalVariable.Length);
                                            }
                                            break;
                                        case VariableType.Real:
                                            break;
                                        case VariableType.LReal:
                                            break;
                                        case VariableType.String:
                                            break;
                                        case VariableType.S7String:
                                            break;
                                        case VariableType.S7WString:
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }

                            var eventData = new ReplyEventData(deviceInfo.Schema, triggerDevInfo.Tag, result.Content, Array.Empty<object>());
                            await _eventBus.Trigger(eventData, _cts.Token);
                        }
                    }
                });
            }

            var noticeDevInfos = deviceInfo.Variables.Where(s => s.Flag == VariableFlag.Notice).ToList();
            foreach (var noticeDevInfo in noticeDevInfos)
            {
                _ = Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(noticeDevInfo.PollingInterval, _cts.Token);

                        var result = await driver.Value.ReadInt32Async(noticeDevInfo.Address);
                        if (result.IsSuccess)
                        {
                            var eventData = new NoticeEventData(deviceInfo.Schema, noticeDevInfo.Tag, result.Content);
                            await _eventBus.Trigger(eventData, _cts.Token);
                        }
                    }
                });
            }
        }
    }

    public void Stop()
    {
        _cts.Cancel();
        _logger.LogInformation("监控停止");
    }

    public void Dispose()
    {
        foreach (var driver in _drivers)
        {
            (driver.Value as NetworkDeviceBase)!.Dispose();
        }
    }
}
