using Ops.Exchange.Bus;

namespace Ops.Exchange.Handlers.Notice;

/// <summary>
/// 基于通知的事件处理。
/// <para>仅有请求的事件</para>
/// </summary>
internal class NoticeEventHandler : IEventHandler<NoticeEventData>
{
    public Task HandleAsync(NoticeEventData eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
