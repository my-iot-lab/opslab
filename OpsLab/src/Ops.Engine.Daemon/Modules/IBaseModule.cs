namespace Ops.Engine.Daemon.Modules;

public interface IBaseModule
{
    void AddModuleRoutes(IEndpointRouteBuilder builder);
}
