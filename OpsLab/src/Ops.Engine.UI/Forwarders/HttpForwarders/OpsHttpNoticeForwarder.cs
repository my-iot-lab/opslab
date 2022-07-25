using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ops.Engine.UI.Config;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Forwarders.HttpForwarders;

internal sealed class OpsHttpNoticeForwarder : INoticeForwarder
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpsUIOptions _opsUIOptions;
    private readonly ILogger _logger;

    public OpsHttpNoticeForwarder(
        IHttpClientFactory httpClientFactory,
        IOptions<OpsUIOptions> opsUIOptions,
        ILogger<OpsHttpNoticeForwarder> logger)
    {
        _httpClientFactory = httpClientFactory;
        _opsUIOptions = opsUIOptions.Value;
        _logger = logger;
    }

    public async Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        string action = "notice";
        // 警报消息，0 表示无任何异常，不用推送
        if (data.Tag == OpsSymbol.PLC_Sys_Alarm)
        {
            if ((uint)data.Values[0].Value == 0)
            {
                return;
            }

            action = "alarm";
        }

        _logger.LogInformation("[Notice] HTTP 数据推送，RequestId：{0}，工站：{1}, 触发点：{2}，数据：{3}",
                data.RequestId,
                data.Schema.Station,
                data.Tag,
                JsonSerializer.Serialize(data.Values.Select(s => new { s.Tag, s.Value })));

        // 推送执行通知
        // TODO: 添加 HTTP 请求验证功能
        var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var httpClient = _httpClientFactory.CreateClient();

        var stopWatch = Stopwatch.StartNew();
        try
        {
            using var httpResponseMessage = await httpClient.PostAsync($"{_opsUIOptions.Api.BaseAddress}/api/scada/{action}", jsonContent, cancellationToken);

            stopWatch.Stop();

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; // 需忽略大小写才能反序列成功
                var result = await JsonSerializer.DeserializeAsync<HttpResult>(contentStream, options, cancellationToken: cancellationToken);
                if (result?.IsOk() != true)
                {
                    // 记录数据推送失败信息
                    _logger.LogError("[Notice] HTTP 数据推送处理失败，RequestId：{0}，工站：{1}, 触发点：{2}，错误状态码：{3}，错误消息：{4}，Elapsed：{5}ms",
                            data.RequestId,
                            data.Schema.Station,
                            data.Tag,
                            result?.Code,
                            result?.Message,
                            stopWatch.Elapsed.TotalMilliseconds);
                    return;
                }

                // 记录成功回执信息
                _logger.LogInformation("[Notice] HTTP 数据推送成功，RequestId：{0}，工站：{1}, 触发点：{2}，Elapsed：{3}ms",
                            data.RequestId,
                            data.Schema.Station,
                            data.Tag,
                            stopWatch.Elapsed.TotalMilliseconds);
                return;
            }

            // 记录数据推送失败
            _logger.LogError("[Notice] HTTP 数据推送失败，RequestId：{0}，工站：{1}, 触发点：{2}，HTTP状态码：{3}",
                   data.RequestId,
                   data.Schema.Station,
                   data.Tag,
                   (int)httpResponseMessage.StatusCode);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "[Notice] HTTP 数据推送取消异常，RequestId：{0}，工站：{1}, 触发点：{2}",
                data.RequestId,
                data.Schema.Station,
                data.Tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Notice] HTTP 数据推送异常，RequestId：{0}，工站：{1}, 触发点：{2}",
                data.RequestId,
                data.Schema.Station,
                data.Tag);
        }
    }
}
