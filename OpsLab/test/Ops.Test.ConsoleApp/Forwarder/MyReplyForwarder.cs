using Microsoft.Extensions.Logging;
using Ops.Exchange.Forwarder;

namespace Ops.Test.ConsoleApp.Forwarder
{
    internal class MyReplyForwarder : IReplyForwarder
    {
        private readonly ILogger _logger;

        public MyReplyForwarder(ILogger<MyReplyForwarder> logger)
        {
            _logger = logger;
        }

        public async Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("请求数据 -- RequestId：{0}，工站：{1}, 数据：{2}",
                data.RequestId,
                data.Schema.Station,
                string.Join(";", data.Values.Select(s => $"{s.Tag}={s.Value}")));

            await Task.Delay(new Random().Next(1000, 10000), cancellationToken);

            var result = new ReplyResult();
            result.AddValue("Normal_1", (short)new Random().Next(10, 255));
            result.AddValue("Normal_2", (short)new Random().Next(10, 255));
            result.AddValue("Normal_3", (short)new Random().Next(10, 255));

            return result;
        }

        public void Dispose()
        {
            
        }
    }
}
