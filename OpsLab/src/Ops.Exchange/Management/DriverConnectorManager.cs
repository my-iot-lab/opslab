using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Ops.Communication.Core;
using Ops.Communication.Core.Net;
using Ops.Communication.Modbus;
using Ops.Communication.Profinet.AllenBradley;
using Ops.Communication.Profinet.Melsec;
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
    /// 初始化，等待连接
    /// </summary>
    Wait = 1,

    /// <summary>
    /// 已连接
    /// </summary>
    Connected = 2,

    /// <summary>
    /// 已与服务断开（被动断开）
    /// </summary>
    Disconnected = 3,

    /// <summary>
    /// 连接终止（主动终止）
    /// </summary>
    Aborted = 4,
}

public sealed class DriverConnector
{
    /// <summary>
    /// 连接ID, 与设备编号 一致
    /// </summary>
    public string Id { get; }

    public string Host { get; }

    public int Port { get; }

    /// <summary>
    /// 连接驱动
    /// </summary>
    public IReadWriteNet Driver { get; }

    /// <summary>
    /// 连接状态
    /// </summary>
    public ConnectingStatus Status { get; set; } = ConnectingStatus.Wait;

    public DriverConnector(string id, string host, int port, IReadWriteNet driver)
    {
        Id = id;
        Host = host;
        Port = port;
        Driver = driver;
    }
}

/// <summary>
/// 设备连接器管理对象
/// </summary>
public sealed class DriverConnectorManager : IDisposable
{
    private readonly Dictionary<string, DriverConnector> _connectors = new(); // Key 为设备编号
    private bool _isConnectedServer;

    private readonly Timer _heartBeatTimer;
    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly ILogger _logger;

    private object SyncLock => _connectors;

    public DriverConnectorManager(DeviceInfoManager deviceInfoManager, ILogger<DriverConnectorManager> logger)
    {
        _deviceInfoManager = deviceInfoManager;
        _logger = logger;

        var state = new WeakReference<DriverConnectorManager>(this);
        _heartBeatTimer = new Timer(Heartbeat, state, 2000, 5000); // 5s 监听一次是否服务器能 ping 通
    }

    /// <summary>
    /// 获取指定的连接驱动
    /// </summary>
    /// <param name="id">设备Id</param>
    /// <returns></returns>
    public DriverConnector this[string id] => _connectors[id];

    /// <summary>
    /// 获取指定的连接驱动
    /// </summary>
    /// <param name="id">设备Id</param>
    /// <returns></returns>
    public DriverConnector? GetConnector(string id)
    {
        if (_connectors.TryGetValue(id, out var connector))
        {
            return connector;
        }
        return default;
    }

    /// <summary>
    /// 获取所有的连接驱动
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<DriverConnector> GetAllDriver()
    {
        return _connectors.Values;
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
                DriverModel.Melsec_CIP => new MelsecCipNet(schema.Host),
                DriverModel.Melsec_A1E => new MelsecA1ENet(schema.Host, schema.Port),
                DriverModel.Melsec_MC => new MelsecMcNet(schema.Host, schema.Port),
                DriverModel.Melsec_MCR => new MelsecMcRNet(schema.Host, schema.Port),
                DriverModel.Omron_FinsTcp => new OmronFinsNet(schema.Host, schema.Port),
                DriverModel.AllenBradley_CIP => new AllenBradleyNet(schema.Host),
                _ => throw new NotImplementedException(),
            };

            // 设置 SocketKeepAliveTime 心跳时间
            driverNet.SocketKeepAliveTime = 60_000;
            _connectors.Add(deviceInfo.Name, new DriverConnector(deviceInfo.Name, deviceInfo.Schema.Host, deviceInfo.Schema.Port, driverNet));
        }
    }

    /// <summary>
    /// 驱动连接到服务
    /// </summary>
    /// <returns></returns>
    public async Task ConnectServerAsync()
    {
        if (!_isConnectedServer)
        {
            foreach (var connector in _connectors.Values)
            {
                var result = await (connector.Driver as NetworkDeviceBase)!.ConnectServerAsync();
                if (result.IsSuccess)
                {
                    connector.Status = ConnectingStatus.Connected;
                }
            }
        }
        _isConnectedServer = true;
    }

    /// <summary>
    /// 连接重置。会断开并清空所有连接。
    /// </summary>
    public void Reset()
    {
        lock (SyncLock)
        {
            foreach (var connector in _connectors.Values)
            {
                (connector.Driver as NetworkDeviceBase)!.Dispose();
                connector.Status = ConnectingStatus.Aborted;
            }

            _connectors.Clear();
            _isConnectedServer = false;
        }
    }

    public void Dispose()
    {
        Reset();

        _heartBeatTimer.Dispose();
    }

    /// <summary>
    /// 轮询监听是否能访问服务器
    /// </summary>
    private void Heartbeat(object state)
    {
        var weakReference = (WeakReference<DriverConnectorManager>)state;
        if (weakReference.TryGetTarget(out var target))
        {
            target.Heartbeat2();
        }
    }

    private void Heartbeat2()
    {
        DriverConnector[] driverConnectors;
        lock (SyncLock)
        {
            driverConnectors = _connectors.Values.ToArray();
        }

        var tasks = new List<Task>(driverConnectors.Length);

        // 考虑站很多，执行时间会超过轮询周期
        foreach (var connector in driverConnectors)
        {
            if (connector.Driver is NetworkDeviceBase networkDevice
                && (connector.Status == ConnectingStatus.Connected || connector.Status == ConnectingStatus.Disconnected))
            {
                var task = new Task(() =>
                {
                    var reply = networkDevice.PingIpAddress(1000);
                    if (reply != IPStatus.Success)
                    {
                        connector.Status = ConnectingStatus.Disconnected;
                    }
                    else
                    {
                        connector.Status = ConnectingStatus.Connected;
                    }
                });

                tasks.Add(task);
            }
        }

        Task.WaitAll(tasks.ToArray());
    }
}
