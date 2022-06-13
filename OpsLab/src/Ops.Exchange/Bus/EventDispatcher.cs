namespace Ops.Exchange.Bus;

/// <summary>
/// 事件派发器。
/// </summary>
internal sealed class EventDispatcher
{
    private readonly TransitionEventState _transitionEventState;
    private readonly IEventHandler _eventHandler;

    public EventDispatcher(TransitionEventState transitionEventState, IEventHandler eventHandler)
    {
        _transitionEventState = transitionEventState;
        _eventHandler = eventHandler;
    }

    /// <summary>
    /// 派发任务
    /// </summary>
    /// <param name="context">事件上下文对象</param>
    public void Dispatch(EventContext context)
    {
        // 检查任务是否可以下发

        if (context.Mode == EventMode.RequestReply)
        {
            string key = "";
            var state = new EventState(key);
            if (!_transitionEventState.CanTransfer(new EventStateKey(key), state))
            {
                return;
            }
        }

        // 将数据推送给下游处理
        _eventHandler.Handle(context.Data);
    }
}
