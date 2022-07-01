using Microsoft.Extensions.Logging;
using Ops.Exchange.Bus;
using Ops.Exchange.Management;

namespace Ops.Exchange.Handlers.Heartbeat;

/// <summary>
/// 基于心跳的事件处理。
/// <para>此事件处理器，用于处理心跳响应事件。</para>
/// </summary>
internal sealed class HeartbeatEventHandler : IEventHandler<HeartbeatEventData>
{
    private readonly CallbackTaskQueueManager _callbackTaskQueueManager;
    private readonly ILogger _logger;

    public HeartbeatEventHandler(CallbackTaskQueueManager callbackTaskQueueManager,
        ILogger<HeartbeatEventHandler> logger)
    {
        _callbackTaskQueueManager = callbackTaskQueueManager;
        _logger = logger;
    }

    public async Task HandleAsync(HeartbeatEventData eventData, CancellationToken cancellationToken = default)
    {
        eventData.Context.SetResponseValue(eventData.Tag, ExStatusCode.Success);
        await _callbackTaskQueueManager.QueueAsync(eventData.Context, cancellationToken);
    }
}
