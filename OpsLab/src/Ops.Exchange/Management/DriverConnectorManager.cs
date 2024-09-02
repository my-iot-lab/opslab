using Ops.Communication;
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

    private bool _hasTryConnectServer;
    private bool _fristConnectSuccessful;
    private PeriodicTimer? _periodicTimer;

    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly ILogger _logger;

    private object SyncLock => _connectors;

    public DriverConnectorManager(DeviceInfoManager deviceInfoManager,
        ILogger<DriverConnectorManager> logger)
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
                DriverModel.Omron_CipNet => new OmronCipNet(schema.Host, schema.Port),
                DriverModel.Omron_HostLinkOverTcp => new OmronHostLinkOverTcp(schema.Host, schema.Port),
                DriverModel.Omron_HostLinkCModeOverTcp => new OmronHostLinkCModeOverTcp(schema.Host, schema.Port),
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
        if (!_hasTryConnectServer)
        {
            foreach (var connector in _connectors.Values)
            {
                if (connector.Driver is NetworkDeviceBase networkDevice)
                {
                    connector.ConnectedStatus = ConnectionStatus.Disconnected; // 初始化

                    // 关闭自动连接
                    networkDevice.AutoConnectServerWhenSocketIsErrorOrNull = false;

                    // 注册方法，在每次连接成功或失败后重置连接状态。
                    networkDevice.ConnectServerPostDelegate = (ok) =>
                    {
                        if (ok)
                        {
                            connector.ConnectedStatus = ConnectionStatus.Connected;
                        }
                    };

                    // 回调，在长连接异常关闭后设置连接状态为 Disconnected。
                    networkDevice.SocketReadErrorClosedDelegate = code =>
                    {
                        // TODO: 根据错误代码来判断是否断开连接
                        if (networkDevice.IsSocketError)
                        {
                            connector.ConnectedStatus = ConnectionStatus.Disconnected;

                            if (code is (int)OpsErrorCode.SocketConnectionAborted
                                    or (int)OpsErrorCode.RemoteClosedConnection
                                    or (int)OpsErrorCode.ReceiveDataTimeout
                                    or (int)OpsErrorCode.SocketSendException
                                    or (int)OpsErrorCode.SocketReceiveException)
                            {
                                _logger.LogWarning("已与服务器断开，主机：{Host}，错误代码：{Code}", connector.Host, code);
                            }
                        }
                    };

                    // 先检查服务器能否访问
                    try
                    {
                        var ipStatus = await networkDevice.PingIpAddressAsync(1_000).ConfigureAwait(false);
                        if (ipStatus == IPStatus.Success)
                        {
                            connector.Available = true;
                            var ret = await networkDevice.ConnectServerAsync().ConfigureAwait(false);
                            if (!ret.IsSuccess)
                            {
                                _logger.LogWarning("尝试连接服务失败，主机：{Host}，端口：{Port}", connector.Host, connector.Port);
                            }

                            _fristConnectSuccessful = ret.IsSuccess;
                        }
                        else
                        {
                            _logger.LogWarning("尝试 Ping 服务失败，主机：{Host}", connector.Host);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "尝试连接服务错误，主机：{Host}，端口：{Port}", connector.Host, connector.Port);
                    }
                }
            }

            _hasTryConnectServer = true;

            // 开启心跳检测
            // 采用 PeriodicTimer 而不是普通的 Timer 定时器，是为了防止产生任务重叠执行。
            _ = PeriodicHeartbeat();
        }
    }

    private Task PeriodicHeartbeat()
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000).ConfigureAwait(false); // 延迟3s后开始监听

            HashSet<string> pingSuccessHosts = new(); // 存放已 Ping 成功的主机信息。

            // PeriodicTimer 定时器，可以让任务不堆积，不会因上一个任务阻塞在下个任务开始时导致多个任务同时进行。
            _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await _periodicTimer.WaitForNextTickAsync().ConfigureAwait(false))
            {
                foreach (var connector in _connectors.Values)
                {
                    // 若连接状态处于断开状态，网络检查 OK 后会进行重连。
                    // 对于初始时设备不可用，后续可用的情况下会自动进行连接。
                    if (connector.Driver is NetworkDeviceBase networkDevice)
                    {
                        try
                        {
                            // 若连接器 Host 相同，每次轮询只需要 Ping 一次即可
                            if (pingSuccessHosts.Contains(connector.Host))
                            {
                                connector.Available = true;
                            }
                            else
                            {
                                connector.Available = networkDevice.PingIpAddress(1000) == IPStatus.Success;
                                if (connector.Available)
                                {
                                    pingSuccessHosts.Add(connector.Host);
                                }
                            }

                            // 注： networkDevice 中连接成功一次，即使服务器断开一段时间后再恢复，连接依旧可用，
                            // 所以，在连接成功一次后，不要再重复连接。
                            if (connector.Available && connector.ConnectedStatus == ConnectionStatus.Disconnected)
                            {
                                // 内部 Socket 异常，或是第一次尝试连接过服务器失败
                                if (networkDevice.IsSocketError || !_fristConnectSuccessful)
                                {
                                    var result = await networkDevice.ConnectServerAsync().ConfigureAwait(false);
                                    if (result.IsSuccess)
                                    {
                                        _logger.LogInformation("已连接上服务，主机：{Host}", connector.Host);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            connector.Available = false;
                            _logger.LogError(ex, "[DriverConnectorManager] Ping 驱动服务器出现异常，主机：{Host}。", connector.Host);
                        }
                    }
                }

                // 一次循环结束后，清空已 Ping 的主机
                pingSuccessHosts.Clear();
            }
        });

        return Task.CompletedTask;
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
    /// 关闭并释放所有连接，同时会清空连接缓存。
    /// </summary>
    public void Close()
    {
        lock (SyncLock)
        {
            if (_hasTryConnectServer)
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
                _periodicTimer?.Dispose();
                _hasTryConnectServer = false;
            }
        }
    }

    public void Dispose()
    {
        Close();
    }
}
