using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ops.Exchange.Configuration;
using Ops.Exchange.Management;

namespace Ops.Exchange.DependencyInjection;

public static class ExchangeServiceCollectionExtensions
{
    /// <summary>
    /// 添加 PLC 服务
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddOpsExchange(this IServiceCollection services)
    {
        services.AddMemoryCache();

        // manager
        services.AddSingleton<DeviceInfoManager>();

        return services;
    }

    public static IServiceCollection LoadFromConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // options
        services.Configure<OpsConfig>(configuration.GetSection("Ops"));

        return services;
    }
}
