using Microsoft.Extensions.Logging;
using Ops.Communication.Core;
using Ops.Communication.Core.Net;
using Ops.Communication.Modbus;
using Ops.Communication.Profinet.AllenBradley;
using Ops.Communication.Profinet.Omron;
using Ops.Communication.Profinet.Siemens;
using Ops.Exchange.Model;

namespace Ops.Exchange.Management;

/// <summary>
/// 连接状态
/// </summary>
public enum ConnectingStatus
{
    /// <summary>
    /// 等待连接
    /// </summary>
    Wait = 1,

    /// <summary>
    /// 已连接
    /// </summary>
    Connected,

    /// <summary>
    /// 已断开
    /// </summary>
    Disconnected,
}

public sealed class DriverConnector
{
    /// <summary>
    /// 连接ID, 与设备 Id 一致
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// 连接驱动
    /// </summary>
    public IReadWriteNet Driver { get; }

    /// <summary>
    /// 连接状态
    /// </summary>
    public ConnectingStatus Status { get; set; } = ConnectingStatus.Wait;

    public DriverConnector(long id, IReadWriteNet driver)
    {
        Id = id;
        Driver = driver;
    }
}

/// <summary>
/// 设备连接器管理对象
/// </summary>
public sealed class DriverConnectorManager : IDisposable
{
    private readonly Dictionary<long, DriverConnector> _drivers = new(); // Key 为 device Id
    private bool _isConnectServer;

    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly ILogger _logger;

    public DriverConnectorManager(DeviceInfoManager deviceInfoManager, ILogger<DriverConnectorManager> logger)
    {
        _deviceInfoManager = deviceInfoManager;
        _logger = logger;
    }

    /// <summary>
    /// 获取指定的连接驱动
    /// </summary>
    /// <param name="id">设备Id</param>
    /// <returns></returns>
    public DriverConnector this[long id] => _drivers[id];

    /// <summary>
    /// 获取指定的连接驱动
    /// </summary>
    /// <param name="id">设备Id</param>
    /// <returns></returns>
    public IReadWriteNet GetDriver(long id)
    {
        var connector = _drivers[id];
        return connector.Driver;
    }

    /// <summary>
    /// 获取所有的连接驱动
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<DriverConnector> GetAllDriver()
    {
        return _drivers.Values;
    }

    /// <summary>
    /// 加载所有的驱动
    /// </summary>
    public async Task LoadAsync()
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
            _drivers.Add(deviceInfo.Id, new DriverConnector(deviceInfo.Id, driverNet));
        }
    }

    /// <summary>
    /// 驱动连接到服务
    /// </summary>
    /// <returns></returns>
    public async Task ConnectServerAsync()
    {
        if (!_isConnectServer)
        {
            foreach (var connector in _drivers.Values)
            {
                await (connector.Driver as NetworkDeviceBase)!.ConnectServerAsync();
                connector.Status = ConnectingStatus.Connected;
            }
        }
        _isConnectServer = true;
    }

    public void Dispose()
    {
        foreach (var connector in _drivers.Values)
        {
            (connector.Driver as NetworkDeviceBase)!.Dispose();
            connector.Status = ConnectingStatus.Disconnected;
        }
    }
}
