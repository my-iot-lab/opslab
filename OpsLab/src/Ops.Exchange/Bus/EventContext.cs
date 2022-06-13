namespace Ops.Exchange.Bus;

/// <summary>
/// 事件上下文对象。
/// </summary>
internal sealed class EventContext
{
    public EventContext(EventData data, EventMode mode)
    {
        Data = data;
        Mode = mode;
    }

    /// <summary>
    /// 事件数据
    /// </summary>
    public EventData Data { get; }

    /// <summary>
    /// 事件模式
    /// </summary>
    public EventMode Mode { get; }
}
