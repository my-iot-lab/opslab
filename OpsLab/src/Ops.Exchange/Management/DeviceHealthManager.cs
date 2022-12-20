using Ops.Exchange.Model;

namespace Ops.Exchange.Management;

/// <summary>
/// 设备状态信息，可用于查询设备的连接状态信息。
/// </summary>
public sealed class DeviceHealthManager
{
    private readonly ConcurrentDictionary<string, DeviceHealthItem> _map = new(); // Key 为设备编号

    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly ILogger _logger;

    private List<DeviceInfo>? _deviceInfos;
    private Timer? _heartbeatTimer;
    private bool _isChecking;

    private object SyncLock => _map;

    /// <summary>
    /// 检查后注册的事件
    /// </summary>
    public EventHandler<HealthEventArgs>? OnChecked { get; set; }

    public DeviceHealthManager(DeviceInfoManager deviceInfoManager, ILogger<DeviceHealthManager> logger)
    {
        _deviceInfoManager = deviceInfoManager;
        _logger = logger;
    }

    /// <summary>
    /// 开始检测，如果已开启检查，则不再运行。
    /// </summary>
    public void Check()
    {
        if (_isChecking)
        {
            return;
        }
        _isChecking = true;

        _deviceInfos = _deviceInfoManager.GetAll();

        // 开启心跳检测
        var state = new WeakReference<DeviceHealthManager>(this);
        int period = Math.Max(5_000, _deviceInfos.Count * 500 + 1_000); // 计算全部Ping一次的时长
        _heartbeatTimer = new Timer(Heartbeat, state, 1000, period); // 5+s 监听一次能否 ping 通服务器
    }

    /// <summary>
    /// 是否可进行连接
    /// </summary>
    /// <param name="deviceName">设备 名称</param>
    /// <returns></returns>
    public bool CanConnect(string deviceName)
    {
        if (_map.TryGetValue(deviceName, out var item))
        {
            return item.CanConnect;
        }

        return false;
    }

    /// <summary>
    /// 是否可进行连接
    /// </summary>
    public bool CanConnect(string line, string station)
    {
        var deviceInfo = _deviceInfos?.FirstOrDefault(s => s.Schema.Line == line && s.Schema.Station == station);
        if (deviceInfo != null)
        {
            return CanConnect(deviceInfo.Name);
        }

        return false;
    }

    /// <summary>
    /// 终止检测
    /// </summary>
    public void Abort()
    {
        lock (SyncLock)
        {
            if (_isChecking)
            {
                _heartbeatTimer?.Dispose();
                _map.Clear();
                _isChecking = false;
            }
        }
    }

    private void Set(DeviceInfo deviceInfo, bool canConnect)
    {
        _map.AddOrUpdate(deviceInfo.Name, k => new(), (k, v) =>
        {
            v.CanConnect = canConnect;
            return v;
        });

        OnChecked?.Invoke(this, new(deviceInfo.Schema.Line, deviceInfo.Schema.Station, canConnect));
    }

    /// <summary>
    /// 轮询监听是否能访问服务器
    /// </summary>
    private void Heartbeat(object? state)
    {
        var weakReference = (WeakReference<DeviceHealthManager>)state!;
        if (weakReference.TryGetTarget(out var target))
        {
            target.Heartbeat2();
        }
    }

    private void Heartbeat2()
    {
        DeviceInfo[] driverConnectors;
        lock (SyncLock)
        {
            driverConnectors = _deviceInfos!.ToArray();
        }

        Ping ping = new();

        foreach (var deviceInfo in _deviceInfos)
        {
            bool canConnect = false;
            try
            {
                var reply = ping.Send(deviceInfo.Schema.Host, 1000); // 可能会出现异常
                canConnect = reply.Status == IPStatus.Success;
                if (!canConnect)
                {
                    _logger.LogWarning($"Ping '{deviceInfo.Schema.Host}' 失败, 返回状态：{reply.Status}");
                }
            }
            catch (PingException ex)
            {
                _logger.LogError(ex, $"Ping '{deviceInfo.Schema.Host}' 异常");
            }
            
            Set(deviceInfo, canConnect);
        }
    }

    private class DeviceHealthItem
    {
        /// <summary>
        /// 是否可连接
        /// </summary>
        public bool CanConnect { get; set; }
    }
}

public sealed class HealthEventArgs : EventArgs
{
    /// <summary>
    /// 产线
    /// </summary>
    public string Line { get; }

    /// <summary>
    /// 工站
    /// </summary>
    public string Station { get; }

    /// <summary>
    /// 是否可连接
    /// </summary>
    public bool CanConnect { get; }

    public HealthEventArgs(string line, string station, bool canConnect)
    {
        Line = line;
        Station = station;
        CanConnect = canConnect;
    }
}
