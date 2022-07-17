using Microsoft.Extensions.Caching.Memory;

namespace BootstrapAdmin.Caching;

/// <summary>
/// CacheManager 接口类
/// </summary>
public interface ICacheManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    T GetOrAdd<T>(string key, Func<ICacheEntry, T> factory);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> factory);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    void Clear(string? key = null);
}
