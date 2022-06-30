using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ops.Exchange.Bus;
using Ops.Exchange.Configuration;
using Ops.Exchange.Handlers.Heartbeat;
using Ops.Exchange.Handlers.Notice;
using Ops.Exchange.Handlers.Reply;
using Ops.Exchange.Management;
using Ops.Exchange.Monitors;
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

        services.AddSingleton<CallbackTaskQueue>();

        // EventHandlers
        services.AddSingleton<HeartbeatEventHandler>();
        services.AddSingleton<ReplyEventHandler>();
        services.AddSingleton<NoticeEventHandler>();

        // Managers
        services.AddSingleton<StateManager>();
        services.AddSingleton<DriverConnectorManager>();
        services.AddSingleton<DeviceInfoManager>();
        services.AddSingleton<MonitorManager>();

        return services;
    }

    public static IServiceCollection LoadFromConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // options
        services.Configure<OpsConfig>(configuration.GetSection("Ops"));

        return services;
    }
}
