namespace Ops.Exchange.Diagnostics;

public sealed class BeforeDispatcherEventData : EventData
{
    public const string EventName = EventNamespace + "BeforeDispatcher";

    public BeforeDispatcherEventData()
    {

    }

    protected override int Count => 3;

    protected override KeyValuePair<string, object> this[int index] => index switch
    {
        _ => throw new IndexOutOfRangeException(nameof(index)),
    };
}

public sealed class AfterDispatcherEventData : EventData
{
    public const string EventName = EventNamespace + "AfterDispatcher";

    public AfterDispatcherEventData()
    {

    }

    protected override int Count => 3;

    protected override KeyValuePair<string, object> this[int index] => index switch
    {
        _ => throw new IndexOutOfRangeException(nameof(index)),
    };
}