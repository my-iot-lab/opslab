using Ops.Exchange.Bus;

namespace Ops.Exchange.Handlers.Heartbeat;

/// <summary>
/// 基于心跳的事件处理。
/// <para>此事件处理器，用于处理心跳响应事件。</para>
/// </summary>
internal class HeartbeatEventHandler : IEventHandler<HeartbeatEventData>
{
    public Task HandleAsync(HeartbeatEventData eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
