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
                        await Task.Delay(100);

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
            if (triggerDevInfos.Any())
            {
                _ = Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(100);

                        foreach (var intervalDevInfo in triggerDevInfos)
                        {
                            var eventData = new ReplyEventData(deviceInfo.Schema, intervalDevInfo.Tag, 1);
                            await _eventBus.Trigger(eventData, _cts.Token);
                        }
                    }
                });
            }

            var intervalDevInfos = deviceInfo.Variables.Where(s => s.Flag == VariableFlag.Interval).ToList();
            if (intervalDevInfos.Any())
            {
                _ = Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(100);

                        foreach (var intervalDevInfo in intervalDevInfos)
                        {
                            var eventData = new NoticeEventData(deviceInfo.Schema, intervalDevInfo.Tag);
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
    }

    public void Dispose()
    {
        foreach (var driver in _drivers)
        {
            (driver.Value as NetworkDeviceBase)!.Dispose();
        }
    }
}
