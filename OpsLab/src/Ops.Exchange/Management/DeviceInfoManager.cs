﻿using System.Text.Json;
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
    private const string FileName = "_deviceinfo.json";

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
    public async Task<List<DeviceInfo>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey, async cacheEntry =>
        {
            //cacheEntry.SetSlidingExpiration(TimeSpan.FromDays(1));
            return await GetFromLocalAsync();
        });
    }

    /// <summary>
    /// 获取指定的设备信息
    /// </summary>
    /// <param name="id">设备 Id</param>
    /// <returns></returns>
    public async Task<DeviceInfo?> GetAsync(long id)
    {
        var deviceInfos = await GetAllAsync();
        return deviceInfos.SingleOrDefault(s => s.Id == id);
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

        var id = deviceInfos.Count == 0 ? 1 : deviceInfos.Max(s => s.Id);
        deviceInfo.Id = id;
        deviceInfos.Add(deviceInfo);

        return (true, string.Empty);
    }

    /// <summary>
    /// 更新设备
    /// </summary>
    /// <param name="deviceInfo">要更新的设备信息</param>
    public async Task<(bool ok, string err)> UpdateAsync(DeviceInfo deviceInfo)
    {
        var deviceInfos = await GetAllAsync();
        if (deviceInfos.Any())
        {
            if (deviceInfos.Exists(s => s.Schema == deviceInfo.Schema))
            {
                return (false, "已存在相同的设备信息");
            }

            deviceInfos.RemoveAll(s => s.Id == deviceInfo.Id);
            deviceInfos.Add(deviceInfo);
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// 删除设备
    /// </summary>
    /// <param name="deviceInfo"></param>
    public async Task RemoveAsync(DeviceInfo deviceInfo)
    {
        var deviceInfos = await GetAllAsync();
        if (deviceInfos.Any())
        {
            deviceInfos.RemoveAll(s => s.Id == deviceInfo.Id);
        }
    }

    public void Clear()
    {
        _cache.Remove(CacheKey);
    }

    /// <summary>
    /// 将缓存中的数据写入到文件中
    /// </summary>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        var deviceInfos = await GetAllAsync();
        var content = JsonSerializer.Serialize(deviceInfos);

        var path = Path.Combine(AppContext.BaseDirectory, _config.DeviceDir, FileName);
        await File.WriteAllTextAsync(path, content, cancellationToken);
    }

    /// <summary>
    /// 从本地存储获取设备信息。
    /// </summary>
    /// <returns></returns>
    private async Task<List<DeviceInfo>> GetFromLocalAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, _config.DeviceDir, FileName);
        if (!File.Exists(path))
        {
            return new();
        }

        // TODO: 文件解析失败如何处理？
        var content = await File.ReadAllTextAsync(path);
        var deviceInfos = JsonSerializer.Deserialize<List<DeviceInfo>>(content);
        return deviceInfos ?? new(0);
    }
}
