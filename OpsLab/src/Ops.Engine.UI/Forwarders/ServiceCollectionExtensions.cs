using Microsoft.Extensions.DependencyInjection;
using Ops.Engine.UI.Forwarders.HttpForwarders;
using Ops.Engine.UI.Forwarders.LocalForwarders;
using Ops.Engine.UI.Forwarders.SignalRForwarders;
using Ops.Exchange.Forwarder;
using System.Net.Http;

namespace Ops.Engine.UI.Forwarders;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpForwarder(this IServiceCollection services)
    {
        services.AddSingleton<INoticeForwarder, OpsHttpNoticeForwarder>();
        services.AddSingleton<IReplyForwarder, OpsHttpReplyForwarder>();

        return services;
    }

    public static IServiceCollection AddLocalForwarder(this IServiceCollection services)
    {
        services.AddSingleton<INoticeForwarder, OpsLocalNoticeForwarder>();
        services.AddSingleton<IReplyForwarder, OpsLocalReplyForwarder>();

        return services;
    }

    public static IServiceCollection AddSignalRForwarder(this IServiceCollection services)
    {
        services.AddSingleton<SignalRForwarderManager>();
        services.AddSingleton<INoticeForwarder, OpsSignalRNoticeForwarder>();
        services.AddSingleton<IReplyForwarder, OpsSignalRReplyForwarder>();

        return services;
    }

    public static IServiceCollection AddGrpcRForwarder(this IServiceCollection services)
    {
        // services.AddGrpcClient<Notify.NotifyClient>(o =>
        // {
        //     o.Address = new("https://localhost:5000");
        // })
        //.ConfigurePrimaryHttpMessageHandler(() =>
        // {
        //     var handler = new HttpClientHandler();
        //     //handler.ClientCertificates.Add(LoadCertificate());
        //     return handler;
        // });

        return services;
    }
}
