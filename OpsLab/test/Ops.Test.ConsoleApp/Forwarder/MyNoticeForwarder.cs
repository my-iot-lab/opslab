using Microsoft.Extensions.Logging;
using Ops.Exchange.Forwarder;

namespace Ops.Test.ConsoleApp.Forwarder
{
    internal sealed class MyNoticeForwarder : INoticeForwarder
    {
        private readonly ILogger _logger;

        public MyNoticeForwarder(ILogger<MyNoticeForwarder> logger)
        {
            _logger = logger;
        }

        public Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{data.RequestId} - {data.Schema.Station} - {data.Tag} - {data.Values[0].Value}");

            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}
