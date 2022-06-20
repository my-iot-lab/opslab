using Ops.Exchange.Bus;

namespace Ops.Exchange.Handlers.Heartbeat;

/// <summary>
/// 基于心跳的事件处理。
/// </summary>
internal class HeartbeatEventHandler : IEventHandler<HeartbeatEventData>
{
    public Task HandleAsync(HeartbeatEventData eventData, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
