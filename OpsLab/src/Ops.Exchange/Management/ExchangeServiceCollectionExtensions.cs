using Ops.Exchange.Bus;
using Ops.Exchange.Configuration;
using Ops.Exchange.Handlers.Heartbeat;
using Ops.Exchange.Handlers.Notice;
using Ops.Exchange.Handlers.Reply;
using Ops.Exchange.Management;
using Ops.Exchange.Monitors;
using Ops.Exchange.Stateless;

namespace Ops.Exchange.DependencyInjection;

public class OpsExchangeOptions
{
}

public static class ExchangeServiceCollectionExtensions
{
    /// <summary>
    /// 添加 OPS Exchange 服务
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddOpsExchange(this IServiceCollection services, IConfiguration configuration, Action<OpsExchangeOptions>? configureDelegate = null)
    {
        // options
        services.Configure<OpsConfig>(configuration.GetSection("Ops"));

        services.AddMemoryCache();
        services.AddSingleton<EventBus>();
        services.AddSingleton<CallbackTaskQueueManager>();

        // EventHandlers
        services.AddSingleton<HeartbeatEventHandler>();
        services.AddSingleton<ReplyEventHandler>();
        services.AddSingleton<NoticeEventHandler>();

        // Managers
        services.AddSingleton<TriggerStateManager>();
        services.AddSingleton<DriverConnectorManager>();
        services.AddSingleton<DeviceInfoManager>();
        services.AddSingleton<MonitorManager>();
        services.AddSingleton<DeviceHealthManager>();

        // Options
        OpsExchangeOptions exOptions = new();
        configureDelegate?.Invoke(exOptions);

        return services;
    }
}
