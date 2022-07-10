using Microsoft.Extensions.DependencyInjection;

namespace Ops.Extensions.Zero.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZeroEntityFrameworkCore(this IServiceCollection services)
    {
        return services;
    }
}
