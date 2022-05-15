using Ops.Engine.Daemon.Modules;

namespace Ops.Engine.Daemon.Extensions;

public static class ModuleExtensions
{
    /// <summary>
    /// 映射 Module 路由
    /// </summary>
    public static void MapRouters(this IEndpointRouteBuilder builder)
    {
        foreach (var module in GetModules())
        {
            module.AddModuleRoutes(builder);
        }
    }

    private static IEnumerable<IBaseModule> GetModules()
    {
        var modules = typeof(IBaseModule).Assembly
            .GetTypes()
            .Where(p => p.IsClass && p.IsAssignableTo(typeof(IBaseModule)))
            .Select(Activator.CreateInstance)
            .Cast<IBaseModule>();

        return modules;
    }
}


