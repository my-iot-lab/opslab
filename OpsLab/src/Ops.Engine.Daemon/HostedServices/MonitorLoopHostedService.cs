namespace Ops.Engine.Daemon.HostedServices
{
    public sealed class MonitorLoopHostedService : BackgroundService
    {
        private readonly ILogger<MonitorLoopHostedService> _logger;

        public IServiceProvider Services { get; }

        public MonitorLoopHostedService(IServiceProvider services, ILogger<MonitorLoopHostedService> logger)
        {
            Services = services;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service running.");

            using (var scope = Services.CreateScope())
            {

            }
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
