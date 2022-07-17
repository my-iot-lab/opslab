using BootstrapAdmin.Caching;
using BootstrapAdmin.Caching.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注入 ICacheManager 服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCacheManager(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.TryAddSingleton<ICacheManager>(provider =>
        {
            var cache = provider.GetRequiredService<IMemoryCache>();
            var cacheManager = new DefaultCacheManager(cache);
            CacheManager.Init(cacheManager);
            return cacheManager;
        });
        return services;
    }
}
