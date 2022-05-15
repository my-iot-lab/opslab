using Ops.Engine.Daemon.Services;

namespace Ops.Engine.Daemon.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// 添加领域服务
    /// </summary>
    public static void AddDomainServices(this IServiceCollection services)
    {
        foreach (var (serviceType, implType) in GetDomainServices<ITransientDomainService>())
        {
            services.AddTransient(serviceType, implType);
        }

        foreach (var (serviceType, implType) in GetDomainServices<ISingletonDomainService>())
        {
            services.AddSingleton(serviceType, implType);
        }
    }

    private static IEnumerable<(Type, Type)> GetDomainServices<T>()
    {
        var services = typeof(T).Assembly
            .GetTypes()
            .Where(p => p.IsClass
                && p.IsAssignableTo(typeof(T))
                && p.FindInterfaces((s, _) => s.IsAssignableTo(typeof(T)) && s != typeof(T), null).Any());

        foreach (var service in services)
        {
            var interfaces = service.FindInterfaces((p, _) => p != typeof(T), null);
            yield return (interfaces[0], service);
        }
    }
}
