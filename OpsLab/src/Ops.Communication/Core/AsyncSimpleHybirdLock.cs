using Nito.AsyncEx;

namespace Ops.Communication.Core;

/// <summary>
/// 一个简单的异步混合线程同步锁，采用了基元用户加基元内核同步构造实现。
/// </summary>
public sealed class AsyncSimpleHybirdLock : IDisposable
{
    private bool disposedValue = false;

    /// <summary>
    /// 基元用户模式构造同步锁
    /// </summary>
    private int m_waiters = 0;
    private int m_lock_tick = 0;

    /// <summary>
    /// 基元内核模式构造同步锁
    /// </summary>
    private readonly Lazy<AsyncAutoResetEvent> m_waiterLock = new(() => new AsyncAutoResetEvent(false));

    private static long simpleHybirdLockCount;

    private static long simpleHybirdLockWaitCount;

    /// <summary>
    /// 获取当前锁是否在等待当中
    /// </summary>
    public bool IsWaitting => m_waiters != 0;

    public int LockingTick => m_lock_tick;

    /// <summary>
    /// 获取当前总的所有进入锁的信息
    /// </summary>
    public static long SimpleHybirdLockCount => simpleHybirdLockCount;

    /// <summary>
    /// 当前正在等待的锁的统计信息，此时已经发生了竞争了
    /// </summary>
    public static long SimpleHybirdLockWaitCount => simpleHybirdLockWaitCount;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                m_waiterLock.Value.Set();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
    }

    /// <summary>
    /// 获取锁
    /// </summary>
    public async Task<bool> EnterAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref simpleHybirdLockCount);
        if (Interlocked.Increment(ref m_waiters) == 1)
        {
            return true;
        }

        Interlocked.Increment(ref simpleHybirdLockWaitCount);
        Interlocked.Increment(ref m_lock_tick);
        await m_waiterLock.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// 离开锁
    /// </summary>
    public bool Leave()
    {
        Interlocked.Decrement(ref simpleHybirdLockCount);
        if (Interlocked.Decrement(ref m_waiters) == 0)
        {
            return true;
        }

        m_waiterLock.Value.Set();  // 设置事件信号，一次只允许执行一个线程, 其它线程继续 Wait。
        Interlocked.Decrement(ref simpleHybirdLockWaitCount);
        Interlocked.Decrement(ref m_lock_tick);
        return true;
    }
}
