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
            var dataString = System.Text.Json.JsonSerializer.Serialize(data.Values.Select(s => new { s.Tag, s.Value }));
            _logger.LogInformation("请求数据 -- RequestId：{0}，工站：{1}, 数据：{2}",
                data.RequestId,
                data.Schema.Station,
                dataString);

            await Task.Delay(new Random().Next(0, 8) * 1000, cancellationToken);

            var result = new ReplyResult();
            result.AddValue("MES_ProdTask_Productcode", $"Hello{new Random().Next(100, 999)}");
            result.AddValue("MES_ProdTask_Amount", (short)new Random().Next(10, 255));
            result.AddValue("MES_ProdTask_Prior", (short)new Random().Next(10, 255));

            return result;
        }
    }
}
