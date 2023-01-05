using Ops.Exchange.Bus;
using Ops.Exchange.Forwarder;

namespace Ops.Exchange.Handlers.Notice;

/// <summary>
/// 基于通知的事件处理。
/// <para>仅有请求的事件</para>
/// </summary>
internal sealed class NoticeEventHandler : IEventHandler<NoticeEventData>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public NoticeEventHandler(IServiceProvider serviceProvider, ILogger<NoticeEventHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task HandleAsync(NoticeEventData eventData, CancellationToken cancellationToken = default)
    {
        var forwardData = new ForwardData(eventData.RequestId, eventData.Schema, eventData.Tag, eventData.Name, new[] { eventData.Value });

        try
        {
            // 采用 Scope 作用域
            using var scope = _serviceProvider.CreateScope();
            var noticeForwarder = scope.ServiceProvider.GetRequiredService<INoticeForwarder>();

            if (eventData.HandleTimeout > 0)
            {
                CancellationTokenSource cts2 = new(eventData.HandleTimeout);
                using var cts0 = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts2.Token);

                await noticeForwarder.ExecuteAsync(forwardData, cts0.Token);
            }
            else
            {
                await noticeForwarder.ExecuteAsync(forwardData, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("[NoticeEventHandler] 任务超时取消 -- RequestId：{0}，工站：{1}，触发点：{2}",
                eventData.RequestId,
                eventData.Schema.Station,
                eventData.Tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NoticeEventHandler] 任务异常 -- RequestId：{0}，工站：{1}，触发点：{2}",
                eventData.RequestId,
                eventData.Schema.Station,
                eventData.Tag);
        }
    }
}
