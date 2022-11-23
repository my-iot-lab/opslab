using Ops.Exchange.Bus;
using Ops.Exchange.Configuration;
using Ops.Exchange.Handlers.Heartbeat;
using Ops.Exchange.Handlers.Notice;
using Ops.Exchange.Handlers.Reply;
using Ops.Exchange.Handlers.Switch;
using Ops.Exchange.Management;
using Ops.Exchange.Monitors;
using Ops.Exchange.Stateless;

namespace Ops.Exchange.DependencyInjection;

/// <summary>
/// 选项
/// </summary>
public class OpsExchangeOptions
{
    /// <summary>
    /// 事件处理器注入时采用的生命周期。
    /// </summary>
    public ServiceLifetime? EventHandlerLifetime { get; set; }
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
        services.AddSingleton<SwitchEventHandler>();

        // Managers
        services.AddSingleton<TriggerStateManager>();
        services.AddSingleton<DriverConnectorManager>();
        services.AddSingleton<DeviceInfoManager>();
        services.AddSingleton<MonitorManager>();
        services.AddSingleton<DeviceHealthManager>();

        // Options
        //OpsExchangeOptions exOptions = new();
        //configureDelegate?.Invoke(exOptions);
        //services.AddSingleton(exOptions);

        return services;
    }
}
