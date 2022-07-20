using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ops.Exchange.Configuration;
using Ops.Exchange.Model;

namespace Ops.Exchange.Management;

/// <summary>
/// 设备信息管理者
/// </summary>
public sealed class DeviceInfoManager
{
    private const string CacheKey = "_cache.ops.deviceinfo";

    private readonly OpsConfig _config;
    private readonly IMemoryCache _cache;

    public DeviceInfoManager(IOptions<OpsConfig> config, IMemoryCache cache)
    {
        _config = config.Value;
        _cache = cache;
    }

    /// <summary>
    /// 获取所有设备信息
    /// </summary>
    /// <returns></returns>
    public List<DeviceInfo> GetAll()
    {
        return _cache.GetOrCreate(CacheKey, cacheEntry =>
        {
            //cacheEntry.SetSlidingExpiration(TimeSpan.FromDays(1));
            return GetFromLocal();
        });
    }

    /// <summary>
    /// 获取所有设备信息
    /// </summary>
    /// <returns></returns>
    public async Task<List<DeviceInfo>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey, async cacheEntry =>
        {
            //cacheEntry.SetSlidingExpiration(TimeSpan.FromDays(1));
            return await GetFromLocalAsync();
        });
    }

    /// <summary>
    /// 获取指定的设备信息。
    /// </summary>
    /// <param name="id">设备 Id</param>
    /// <returns></returns>
    public async Task<DeviceInfo?> GetAsync(string code)
    {
        var deviceInfos = await GetAllAsync();
        return deviceInfos.SingleOrDefault(s => s.Name == code);
    }

    /// <summary>
    /// 获取指定的设备信息。
    /// </summary>
    /// <param name="line">产线编号</param>
    /// <param name="station">设备工站编号</param>
    /// <returns></returns>
    public DeviceInfo? Get(string line, string station)
    {
        var deviceInfos = GetAll();
        return deviceInfos.SingleOrDefault(s => s.Schema.Line == line && s.Schema.Station == station);
    }

    /// <summary>
    /// 添加设备
    /// </summary>
    /// <param name="deviceInfo">要添加的设备信息</param>
    public async Task<(bool ok, string err)> AddAsync(DeviceInfo deviceInfo)
    {
        var deviceInfos = await GetAllAsync();
        if (deviceInfos.Exists(s => s.Schema == deviceInfo.Schema))
        {
            return (false, "已存在相同的设备信息");
        }

        deviceInfos.Add(deviceInfo);

        return (true, string.Empty);
    }

    /// <summary>
    /// 更新设备
    /// </summary>
    /// <param name="deviceInfo">要更新的设备信息</param>
    public (bool ok, string err) Update(DeviceInfo deviceInfo)
    {
        var deviceInfos = GetAll();
        if (deviceInfos.Any())
        {
            if (deviceInfos.Exists(s => s.Schema == deviceInfo.Schema))
            {
                return (false, "已存在相同的设备信息");
            }

            deviceInfos.RemoveAll(s => s.Name == deviceInfo.Name);
            deviceInfos.Add(deviceInfo);
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// 删除设备
    /// </summary>
    /// <param name="deviceInfo"></param>
    public void Remove(DeviceInfo deviceInfo)
    {
        var deviceInfos = GetAll();
        if (deviceInfos.Any())
        {
            deviceInfos.RemoveAll(s => s.Name == deviceInfo.Name);
        }
    }

    public void Clear()
    {
        _cache.Remove(CacheKey);
    }

    /// <summary>
    /// 将缓存中的数据写入到文件中
    /// </summary>
    public void Flush()
    {
        var deviceInfos = GetAll();
        foreach (var deviceInfo in deviceInfos)
        {
            Flush(deviceInfo);
        }
    }

    public void Flush(DeviceInfo deviceInfo)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, _config.DeviceDir);
        var fileName = $"{deviceInfo.Name}.json";

        var content = JsonSerializer.Serialize(deviceInfo, new JsonSerializerOptions { WriteIndented = true });
        var path = Path.Combine(dir, fileName);
        File.WriteAllText(path, content);
    }

    private List<DeviceInfo> GetFromLocal()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, _config.DeviceDir);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            return new List<DeviceInfo>(0);
        }

        var filePaths = Directory.GetFiles(dir);
        List<DeviceInfo> deviceInfos = new(filePaths.Length);
        foreach (var filePath in filePaths)
        {
            var content = File.ReadAllText(filePath);
            var deviceInfo = JsonSerializer.Deserialize<DeviceInfo>(content);
            deviceInfos.Add(deviceInfo!);
        }

        return deviceInfos;
    }

    /// <summary>
    /// 从本地存储获取设备信息。
    /// </summary>
    /// <returns></returns>
    private Task<List<DeviceInfo>> GetFromLocalAsync()
    {
        return Task.FromResult(GetFromLocal());
    }
}
