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
	private int _waiters = 0;
    private int _lock_tick = 0;

    /// <summary>
    /// 基元内核模式构造同步锁
    /// </summary>
    private readonly Lazy<AutoResetEvent> _waiterLock = new(() => new AutoResetEvent(false));

	private static long s_simpleHybirdLockCount;

	private static long s_simpleHybirdLockWaitCount;

	/// <summary>
	/// 获取当前锁是否在等待当中
	/// </summary>
	public bool IsWaitting => _waiters != 0;

    public int LockingTick => _lock_tick;

    /// <summary>
    /// 获取当前总的所有进入锁的信息
    /// </summary>
    public static long SimpleHybirdLockCount => s_simpleHybirdLockCount;

	/// <summary>
	/// 当前正在等待的锁的统计信息，此时已经发生了竞争了
	/// </summary>
	public static long SimpleHybirdLockWaitCount => s_simpleHybirdLockWaitCount;

	private void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
                _waiterLock.Value.Close();
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
		Interlocked.Increment(ref s_simpleHybirdLockCount);
		if (Interlocked.Increment(ref _waiters) == 1)
		{
			return true;
		}

        Interlocked.Increment(ref s_simpleHybirdLockWaitCount);
        Interlocked.Increment(ref _lock_tick);
        return _waiterLock.Value.WaitOne();
    }

	/// <summary>
	/// 离开锁
	/// </summary>
	public bool Leave()
	{
		Interlocked.Decrement(ref s_simpleHybirdLockCount);
		if (Interlocked.Decrement(ref _waiters) == 0)
		{
			return true;
		}

        var flag = _waiterLock.Value.Set(); // 设置事件信号，一次只允许执行一个线程, 其它线程继续 WaitOne 。
        Interlocked.Decrement(ref s_simpleHybirdLockWaitCount);
        Interlocked.Decrement(ref _lock_tick);
        return flag;
    }
}
