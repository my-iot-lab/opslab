using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Forwarders;

internal sealed class OpsNoticeForwarder : INoticeForwarder
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public OpsNoticeForwarder(
        IHttpClientFactory httpClientFactory,
        ILogger<OpsNoticeForwarder> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Notice] RequestId：{0}，工站：{1}, 触发点：{2}，数据：{3}",
                data.RequestId,
                data.Schema.Station,
                data.Tag,
                string.Join("; ", data.Values.Select(s => $"{s.Tag}={s.Value}")));

        // 推送执行通知
        var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var httpClient = _httpClientFactory.CreateClient();
        using var httpResponseMessage = await httpClient.PostAsync("api/xxx", jsonContent, cancellationToken);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<HttpResult>(contentStream);
            if (result?.IsOk() != true)
            {
                // 记录数据推送失败
                return;
            }
        }

        // 记录数据推送失败
    }

    public void Dispose()
    {
        
    }
}
