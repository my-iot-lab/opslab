namespace Ops.Exchange.Utils;

/// <summary>
/// 定时器接口
/// </summary>
internal interface ITimer : IDisposable
{
    void Change(long dueTime, long period);
}
