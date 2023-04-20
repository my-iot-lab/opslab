using Ops.Exchange.Model;

namespace Ops.Exchange.Management;

/// <summary>
/// 设备状态信息，可用于查询设备的连接状态信息。
/// </summary>
public sealed class DeviceHealthManager
{
    private readonly ConcurrentDictionary<string, DeviceHealthItem> _map = new(); // Key 为设备编号

    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;

    private List<DeviceInfo>? _deviceInfos;
    private Timer? _heartbeatTimer;
    private PeriodicTimer? _heartbeatTimer2;
    private bool _isChecking;

    private object SyncLock => _map;

    /// <summary>
    /// 检查后注册的事件
    /// </summary>
    public EventHandler<HealthEventArgs>? OnChecked { get; set; }

    public DeviceHealthManager(DeviceInfoManager deviceInfoManager, IMemoryCache memoryCache, ILogger<DeviceHealthManager> logger)
    {
        _deviceInfoManager = deviceInfoManager;
        _memoryCache = memoryCache;
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
        _heartbeatTimer = new Timer(Heartbeat, state, 1000, period); // 5s+ 监听一次能否 ping 通服务器
    }

    /// <summary>
    /// 开始检测，如果已开启检查，则不再运行。
    /// </summary>
    public async ValueTask CheckAsync(CancellationToken cancellationToken = default)
    {
        // 此计时器可异步执行，且在当前任务还没有结束之前下一次任务是不会开始的。
        _heartbeatTimer2 = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await _heartbeatTimer2.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            Heartbeat2();
        }
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
                _heartbeatTimer2?.Dispose();

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
            if (canConnect)
            {
                v.Retry = 1;
            }
            else
            {
                v.Retry++;
            }

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
            // 采用缓存，同一主机在规定时间内不重复 Ping
            var cacheName = $"__heartbeat_ping:{deviceInfo.Schema.Host}";
            bool hasPing = _memoryCache.TryGetValue(cacheName, out bool canConnect);
            if (!hasPing)
            {
                try
                {
                    var reply = ping.Send(deviceInfo.Schema.Host, 1000); // 可能会出现异常（如网线）
                    canConnect = reply.Status == IPStatus.Success;
                    if (canConnect)
                    {
                        _memoryCache.Set(cacheName, true, TimeSpan.FromSeconds(5)); // 指定时间内不重复 Ping 同一服务器
                    }
                    else
                    {
                        _memoryCache.Set(cacheName, false, TimeSpan.FromSeconds(3));
                        if (_map.TryGetValue(deviceInfo.Name, out var item))
                        {
                            _logger.LogWarning("Ping '{Host}' 失败, 返回状态：{Status}", deviceInfo.Schema.Host, reply.Status);
                        }
                    }
                }
                catch (PingException ex)
                {
                    _memoryCache.Set(cacheName, false, TimeSpan.FromSeconds(3));
                    if (_map.TryGetValue(deviceInfo.Name, out var item))
                    {
                        _logger.LogWarning("Ping '{Host}' 异常, 消息：{Message}", deviceInfo.Schema.Host, ex.Message);
                    }
                }
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

        /// <summary>
        /// 尝试连接次数
        /// </summary>
        public long Retry { get; set; }
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
