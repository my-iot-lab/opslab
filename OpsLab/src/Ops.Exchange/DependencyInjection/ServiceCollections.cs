using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ops.Exchange.Config;
using Ops.Exchange.Manager;

namespace Ops.Exchange.DependencyInjection;

public static class ServiceCollections
{
    /// <summary>
    /// 添加 PLC 服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddScada(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        // options
        services.Configure<OpsConfig>(configuration.GetSection("Ops"));

        // manager
        services.AddSingleton<DeviceInfoManager>();

        return services;
    }
}
