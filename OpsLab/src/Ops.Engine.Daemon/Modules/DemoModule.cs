using Microsoft.AspNetCore.Mvc;
using Ops.Engine.Daemon.Services;

namespace Ops.Engine.Daemon.Modules;

public class DemoModule : IBaseModule
{
    public void AddModuleRoutes(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/demo", () =>
        {
            return "Mini API Demo";
        });

        // 有服务注入，需要指明 FromServicesAttribute 属性。
        builder.MapGet("/demo/info", ([FromServices] IDemoService demoService, string name) =>
        {
            return demoService.Demo(name);
        });
    }
}
