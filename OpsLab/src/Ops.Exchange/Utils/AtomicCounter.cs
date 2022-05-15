namespace Ops.Exchange.Utils;

/// <summary>
/// 基于 Atomic 原子操作的计数器
/// </summary>
internal sealed class AtomicCounter
{
    private int _value;

    /// <summary>
    /// 获取当前计数器的值。
    /// </summary>
    public int Value
    {
        get => Volatile.Read(ref _value);
        set => Volatile.Write(ref _value, value);
    }

    /// <summary>
    /// 原子操作 + 1。
    /// </summary>
    public int Increment()
    {
        return Interlocked.Increment(ref _value);
    }

    /// <summary>
    /// 原子操作 - 1。
    /// </summary>
    public int Decrement()
    {
        return Interlocked.Decrement(ref _value);
    }

    /// <summary>
    /// 重置为 0。
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _value, 0);
    }
}
