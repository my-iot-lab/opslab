using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ops.Exchange.Bus;
using Ops.Exchange.Configuration;
using Ops.Exchange.Handlers.Reply;
using Ops.Exchange.Management;
using Ops.Exchange.Stateless;

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

        services.AddSingleton<EventBus>();

        // EventHandler
        services.AddSingleton<ReplyEventHandler>();

        // manager
        services.AddSingleton<DeviceInfoManager>();
        services.AddSingleton<StateManager>();

        return services;
    }

    public static IServiceCollection LoadFromConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // options
        services.Configure<OpsConfig>(configuration.GetSection("Ops"));

        return services;
    }
}
