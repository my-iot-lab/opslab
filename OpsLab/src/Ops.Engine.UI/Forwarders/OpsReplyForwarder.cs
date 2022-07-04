using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Forwarders;

internal sealed class OpsReplyForwarder : IReplyForwarder
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public OpsReplyForwarder(
        IHttpClientFactory httpClientFactory,
        ILogger<OpsReplyForwarder> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Reply] RequestId：{0}，工站：{1}, 触发点：{2}，数据：{3}",
                data.RequestId,
                data.Schema.Station,
                data.Tag,
                string.Join("; ", data.Values.Select(s => $"{s.Tag}={s.Value}")));

        // 派发数据
        var action = data.Tag switch
        {
            OpsSymbol.PLC_Sign_Inbound => "",
            OpsSymbol.PLC_Sign_Outbound => "",
            OpsSymbol.PLC_Sign_Critical_Material => "",
            OpsSymbol.PLC_Sign_Batch_Material => "",
            _ => throw new System.NotImplementedException(),
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var httpClient = _httpClientFactory.CreateClient();
        using var httpResponseMessage = await httpClient.PostAsync($"api/xxx/{action}", jsonContent, cancellationToken);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<HttpResult<ReplyResult>>(contentStream);
            if (result?.IsOk() == true)
            {
                return result.Data;
            }

            // 记录数据推送失败
        }

        return new()
        {
            Result = 4,
        };
    }

    public void Dispose()
    {
    }
}
