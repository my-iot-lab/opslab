namespace Ops.Engine.Daemon.Services;

public interface IDemoService : ITransientDomainService
{
    string Demo(string name);
}
