namespace Ops.Exchange.Bus;

/// <summary>
/// 事件源：描述事件信息，用于参数传递。
/// </summary>
public abstract class EventData : IEventData
{
    public DateTime EventTime { get; set; }

    public object? EventSource { get; set; }

    public EventData()
    {
        EventTime = DateTime.Now;
    }
}
