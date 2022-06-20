using Ops.Exchange.Bus;
using Ops.Exchange.Stateless;

namespace Ops.Exchange.Handlers.Reply;

/// <summary>
/// 基于响应的事件处理。
/// <para>请求响应事件</para>
/// </summary>
internal sealed class ReplyEventHandler : IEventHandler<ReplyEventData>
{
    private readonly StateManager _stateManager;

    public ReplyEventHandler(StateManager stateManager)
    {
        _stateManager = stateManager;
    }

    public Task HandleAsync(ReplyEventData eventData, CancellationToken cancellationToken)
    {
        if (_stateManager.CanTransfer(new StateKey("", ""), 1))
        {

        }

        return Task.CompletedTask;
    }
}
