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

        // services
        services.AddFreeSql();
        services.AddHostAppServices();

        // viewmodels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<NonClientAreaContentViewModel>(); 
        services.AddTransient<KibanaViewModel>();
        services.AddTransient<UserViewModel>();

        return services;
    }
}
