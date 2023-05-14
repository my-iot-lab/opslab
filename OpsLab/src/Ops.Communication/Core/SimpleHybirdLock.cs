namespace Ops.Communication.Core;

/// <summary>
/// 一个简单的混合线程同步锁，采用了基元用户加基元内核同步构造实现。
/// </summary>
/// <remarks>
/// 当前的锁适用于，竞争频率比较低，锁部分的代码运行时间比较久的情况，当前的简单混合锁可以达到最大性能。
/// </remarks>
public sealed class SimpleHybirdLock : IDisposable
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
    private readonly Lazy<AutoResetEvent> m_waiterLock = new(() => new AutoResetEvent(false));

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
                m_waiterLock.Value.Close();
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
	public bool Enter()
	{
		Interlocked.Increment(ref simpleHybirdLockCount);
		if (Interlocked.Increment(ref m_waiters) == 1)
		{
			return true;
		}

        Interlocked.Increment(ref simpleHybirdLockWaitCount);
        Interlocked.Increment(ref m_lock_tick);
        return m_waiterLock.Value.WaitOne();
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

        var flag = m_waiterLock.Value.Set(); // 设置事件信号，一次只允许执行一个线程, 其它线程继续 WaitOne 。
        Interlocked.Decrement(ref simpleHybirdLockWaitCount);
        Interlocked.Decrement(ref m_lock_tick);
        return flag;
    }
}
