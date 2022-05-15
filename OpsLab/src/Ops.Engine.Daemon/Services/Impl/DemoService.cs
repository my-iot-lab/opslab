namespace Ops.Engine.Daemon.Services.Impl;

internal class DemoService : IDemoService
{
    private readonly ILogger _logger;

    public DemoService(ILogger<DemoService> logger)
    {
        _logger = logger;
    }

    public string Demo(string name)
    {
        return $"Demo: {name}";
    }
}
