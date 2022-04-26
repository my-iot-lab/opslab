namespace Ops.Communication.Core;

/// <summary>
/// 一个高级的混合线程同步锁，采用了基元用户加基元内核同步构造实现，并包含了自旋和线程所有权
/// </summary>
/// <remarks>
/// 当竞争的频率很高的时候，锁的时间很短的时候，当前的锁可以获得最大性能。
/// </remarks>
internal sealed class AdvancedHybirdLock : IDisposable
{
	private bool disposedValue = false;
	private int m_waiters = 0;
	private readonly Lazy<AutoResetEvent> m_waiterLock = new(() => new AutoResetEvent(initialState: false));
	private int m_owningThreadId = 0;
	private int m_recursion = 0;

	/// <summary>
	/// 自旋锁的自旋周期，当竞争频率小，就要设置小，当竞争频率大，就要设置大，锁时间长就设置小，锁时间短就设置大，这样才能达到真正的高性能，默认为1000
	/// </summary>
	public int SpinCount { get; set; } = 1000;

	private void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				m_waiterLock.Value.Close();
				disposedValue = true;
			}
		}
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>
	/// 获取锁
	/// </summary>
	public void Enter()
	{
		int managedThreadId = Environment.CurrentManagedThreadId;
		if (managedThreadId == m_owningThreadId)
		{
			m_recursion++;
			return;
		}

		SpinWait spinWait = default;
		for (int i = 0; i < SpinCount; i++)
		{
			if (Interlocked.CompareExchange(ref m_waiters, 1, 0) == 0)
			{
				m_owningThreadId = Environment.CurrentManagedThreadId;
				m_recursion = 1;
				return;
			}
			spinWait.SpinOnce();
		}
		if (Interlocked.Increment(ref m_waiters) > 1)
		{
			m_waiterLock.Value.WaitOne();
		}
		m_owningThreadId = Environment.CurrentManagedThreadId;
		m_recursion = 1;
	}

	/// <summary>
	/// 离开锁
	/// </summary>
	public void Leave()
	{
		if (Environment.CurrentManagedThreadId != m_owningThreadId)
		{
			throw new SynchronizationLockException("Current Thread have not the owning thread.");
		}

		if (--m_recursion <= 0)
		{
			m_owningThreadId = 0;
			if (Interlocked.Decrement(ref m_waiters) != 0)
			{
				m_waiterLock.Value.Set();
			}
		}
	}
}
