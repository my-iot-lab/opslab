using Microsoft.Extensions.Logging;
using Ops.Exchange.Bus;

namespace Ops.Exchange.Handlers.Notice;

/// <summary>
/// 基于通知的事件处理。
/// <para>仅有请求的事件</para>
/// </summary>
internal sealed class NoticeEventHandler : IEventHandler<NoticeEventData>
{
    private readonly ILogger _logger;

    public NoticeEventHandler(ILogger<NoticeEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(NoticeEventData eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"{eventData.Schema.Station} - {eventData.Tag} - {eventData.Value.Value}");

        return Task.CompletedTask;
    }
}
