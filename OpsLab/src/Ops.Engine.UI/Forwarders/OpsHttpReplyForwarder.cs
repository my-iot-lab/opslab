using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ops.Engine.UI.Config;
using Ops.Exchange;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Forwarders;

internal sealed class OpsHttpReplyForwarder : IReplyForwarder
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpsUIOptions _opsUIOptions;
    private readonly ILogger _logger;

    public OpsHttpReplyForwarder(
        IHttpClientFactory httpClientFactory,
        IOptions<OpsUIOptions> opsUIOptions,
        ILogger<OpsHttpReplyForwarder> logger)
    {
        _httpClientFactory = httpClientFactory;
        _opsUIOptions = opsUIOptions.Value;
        _logger = logger;
    }

    public async Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Reply] HTTP 数据推送，RequestId：{0}，工站：{1}, 触发点：{2}，数据：{3}",
                data.RequestId,
                data.Schema.Station,
                data.Tag,
                JsonSerializer.Serialize(data.Values.Select(s => new { s.Tag, s.Value })));

        // 派发数据
        var action = data.Tag switch
        {
            OpsSymbol.PLC_Sign_Inbound => "inbound",
            OpsSymbol.PLC_Sign_Outbound => "outbound",
            OpsSymbol.PLC_Sign_Critical_Material => "materialcritical",
            OpsSymbol.PLC_Sign_Batch_Material => "materialbatch",
            _ => throw new NotImplementedException(),
        };

        // TODO: 添加 HTTP 请求验证功能
        var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var httpClient = _httpClientFactory.CreateClient();

        try
        {
            using var httpResponseMessage = await httpClient.PostAsync($"{_opsUIOptions.Api.BaseAddress}/api/scada/{action}", jsonContent, cancellationToken);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
                var result = await JsonSerializer.DeserializeAsync<HttpResult>(contentStream, cancellationToken: cancellationToken);
                if (result?.IsOk() == true)
                {
                    // 记录成功回执信息
                    _logger.LogInformation("[Reply] HTTP 数据推送成功，RequestId：{0}，工站：{1}, 触发点：{2}，响应结果：{3}，响应数据：{4}",
                                data.RequestId,
                                data.Schema.Station,
                                data.Tag,
                                result.Code,
                                JsonSerializer.Serialize(data.Values.Select(s => new { s.Tag, s.Value })));

                    return result.ToReplyResult();
                }

                // 记录数据推送失败
                _logger.LogError("[Reply] HTTP 数据推送失败，RequestId：{0}，工站：{1}, 触发点：{2}，错误状态码：{3}，错误消息：{4}",
                            data.RequestId,
                            data.Schema.Station,
                            data.Tag,
                            result?.Code,
                            result?.Message);
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "[Reply] HTTP 数据推送取消异常，RequestId：{0}，工站：{1}, 触发点：{2}",
                data.RequestId,
                data.Schema.Station,
                data.Tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Reply] HTTP 数据推送异常，RequestId：{0}，工站：{1}, 触发点：{2}",
                data.RequestId,
                data.Schema.Station,
                data.Tag);
        }

        return new()
        {
            Result = ExStatusCode.RemoteException,
        };
    }

    public void Dispose()
    {
    }
}
