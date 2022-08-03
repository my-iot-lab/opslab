using Microsoft.Extensions.DependencyInjection;
using Ops.Exchange.Forwarder;
using Ops.Host.App.Core.Extensions;
using Ops.Host.App.Forwarders;
using Ops.Host.App.ViewModels;

namespace Ops.Host.App.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHostApp(this IServiceCollection services)
    {
        // forwarders
        services.AddSingleton<INoticeForwarder, OpsLocalNoticeForwarder>();
        services.AddSingleton<IReplyForwarder, OpsLocalReplyForwarder>();

        // viewmodels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<NonClientAreaContentViewModel>();

        // services
        services.AddHostAppServices();

        return services;
    }
}
