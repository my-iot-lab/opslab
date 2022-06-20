namespace Ops.Exchange.Bus;

/// <summary>
/// 定义事件处理器公共接口，所有的事件处理都要实现该接口。
/// </summary>
public interface IEventHandler
{
}

/// <summary>
/// 泛型事件处理器接口。
/// </summary>
public interface IEventHandler<TEventData> : IEventHandler
    where TEventData : IEventData
{
    /// <summary>
    /// 事件处理器实现该方法来处理事件。
    /// </summary>
    /// <param name="eventData">要处理的事件数据</param>
    Task HandleAsync(TEventData eventData, CancellationToken cancellationToken);
}
