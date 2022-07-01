using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ops.Exchange.Bus;
using Ops.Exchange.Configuration;
using Ops.Exchange.Forwarder;
using Ops.Exchange.Handlers.Heartbeat;
using Ops.Exchange.Handlers.Notice;
using Ops.Exchange.Handlers.Reply;
using Ops.Exchange.Management;
using Ops.Exchange.Monitors;
using Ops.Exchange.Stateless;

namespace Ops.Exchange.DependencyInjection;

public class OpsExchangeOptions
{
    public Type? NoticeForwarderType { get; private set; }

    public Type? ReplyForwarderType { get; private set; }

    /// <summary>
    /// 添加通知事件处理器。
    /// <para>对象会采用 Scope 作用域。</para>
    /// </summary>
    /// <typeparam name="TNoticeForwarder"></typeparam>
    public void AddNoticeForword<TNoticeForwarder>()
        where TNoticeForwarder : INoticeForwarder
    {
        NoticeForwarderType = typeof(TNoticeForwarder);
    }

    /// <summary>
    /// 添加触发事件处理器。
    /// <para>对象会采用 Scope 作用域。</para>
    /// </summary>
    /// <typeparam name="TReplyForwarder"></typeparam>
    public void AddReplyForword<TReplyForwarder>()
        where TReplyForwarder : IReplyForwarder
    {
        ReplyForwarderType = typeof(TReplyForwarder);
    }
}

public static class ExchangeServiceCollectionExtensions
{
    /// <summary>
    /// 添加 OPS Exchange 服务
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddOpsExchange(this IServiceCollection services, IConfiguration configuration, Action<OpsExchangeOptions> configureDelegate)
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

        // Options
        OpsExchangeOptions exOptions = new();
        configureDelegate(exOptions);

        services.AddScoped(typeof(INoticeForwarder), exOptions.NoticeForwarderType ?? typeof(NullNoticeForwarder));
        services.AddScoped(typeof(IReplyForwarder), exOptions.ReplyForwarderType ?? typeof(NullReplyForwarder));

        return services;
    }
}
