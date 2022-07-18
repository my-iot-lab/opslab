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
public enum ConnectionStatus
{
    /// <summary>
    /// 初始化，等待连接
    /// </summary>
    Wait = 1,

    /// <summary>
    /// 已连接
    /// </summary>
    Connected,

    /// <summary>
    /// 已与服务断开
    /// </summary>
    Disconnected,

    /// <summary>
    /// 连接终止，表示不会再连接。
    /// </summary>
    Aborted,
}

/// <summary>
/// 驱动状态
/// </summary>
public enum DriverStatus
{
    /// <summary>
    /// 可正常通信
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 驱动挂起中
    /// </summary>
    Suspended = 2,
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
    /// 连接处于的状态
    /// </summary>
    public ConnectionStatus ConnectedStatus { get; set; } = ConnectionStatus.Wait;

    /// <summary>
    /// 表示可与服务器进行连接（能 Ping 通）。
    /// </summary>
    public bool Available { get; internal set; }

    /// <summary>
    /// 驱动状态
    /// </summary>
    public DriverStatus DriverStatus { get; internal set; } = DriverStatus.Normal;

    public DriverConnector(string id, string host, int port, IReadWriteNet driver)
    {
        Id = id;
        Host = host;
        Port = port;
        Driver = driver;
    }

    /// <summary>
    /// 是否可连接
    /// </summary>
    public bool CanConnect => Available && DriverStatus == DriverStatus.Normal && ConnectedStatus == ConnectionStatus.Connected;
}

/// <summary>
/// 设备连接器管理对象
/// </summary>
public sealed class DriverConnectorManager : IDisposable
{
    private readonly Dictionary<string, DriverConnector> _connectors = new(); // Key 为设备编号
    private bool _isConnectedServer;
    private Timer? _heartbeatTimer;

    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly ILogger _logger;

    private object SyncLock => _connectors;

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
    public async Task ConnectAsync()
    {
        if (!_isConnectedServer)
        {
            foreach (var connector in _connectors.Values)
            {
                if (connector.Driver is NetworkDeviceBase networkDevice)
                {
                    networkDevice.SetPersistentConnection(); // 设置为长连接

                    // 每次连接成功或失败后设置状态
                    networkDevice.ConnectServerPostDelegate = (ok) =>
                    {
                        connector.ConnectedStatus = ok ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
                    };

                    // 先检查服务器能否访问
                    var ipStatus = await networkDevice.PingIpAddressAsync(1_000);
                    if (ipStatus == IPStatus.Success)
                    {
                        connector.Available = true;
                        _ = await networkDevice.ConnectServerAsync();
                    }
                    else
                    {
                        connector.ConnectedStatus = ConnectionStatus.Disconnected;
                    }
                }
            }

            _isConnectedServer = true;

            // 开启心跳检测
            var state = new WeakReference<DriverConnectorManager>(this);
            int period = Math.Max(5_000, _connectors.Count * 500 + 2_000); // 计算全部Ping一次的时长
            _heartbeatTimer = new Timer(Heartbeat, state, 1000, period); // 5s 监听一次是否服务器能 ping 通
        }
    }

    /// <summary>
    /// 驱动连接挂起
    /// </summary>
    public void Suspend()
    {
        lock (SyncLock)
        {
            foreach (var connector in _connectors.Values)
            {
                if (connector.DriverStatus == DriverStatus.Normal)
                {
                    connector.DriverStatus = DriverStatus.Suspended;
                }
            }
        }
    }

    public void Restart()
    {
        lock (SyncLock)
        {
            foreach (var connector in _connectors.Values)
            {
                if (connector.DriverStatus == DriverStatus.Suspended)
                {
                    connector.DriverStatus = DriverStatus.Normal;
                }
            }
        }
    }

    /// <summary>
    /// 关闭并释放所有连接。
    /// </summary>
    public void Close()
    {
        lock (SyncLock)
        {
            if (_isConnectedServer)
            {
                foreach (var connector in _connectors.Values)
                {
                    connector.ConnectedStatus = ConnectionStatus.Aborted;
                    if (connector.Driver is NetworkDeviceBase networkDevice)
                    {
                        networkDevice.Dispose();
                    }
                }

                _connectors.Clear();
                _heartbeatTimer?.Dispose();
                _isConnectedServer = false;
            }
        }
    }

    public void Dispose()
    {
        Close();
    }

    /// <summary>
    /// 轮询监听是否能访问服务器
    /// </summary>
    private void Heartbeat(object? state)
    {
        var weakReference = (WeakReference<DriverConnectorManager>)state!;
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

        // 可考虑采用 Task.Run 快速执行执行。
        foreach (var connector in driverConnectors)
        {
            if (connector.Driver is NetworkDeviceBase networkDevice
                && connector.ConnectedStatus == ConnectionStatus.Disconnected)
            {
                var reply = networkDevice.PingIpAddress(500);
                connector.Available = reply == IPStatus.Success;

                if (connector.Available)
                {
                    _ = networkDevice.ConnectServer();
                }
            }
        }
    }
}
