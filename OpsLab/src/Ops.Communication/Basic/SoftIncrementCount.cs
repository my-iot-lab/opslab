using Ops.Communication.Core;

namespace Ops.Communication.Basic;

/// <summary>
/// 一个简单的不持久化的序号自增类，采用线程安全实现，并允许指定最大数字，将包含该最大值，到达后清空从指定数开始
/// </summary>
public sealed class SoftIncrementCount : IDisposable
{
	private long start = 0L;

	private long current = 0L;

	private long max = long.MaxValue;

	private readonly SimpleHybirdLock hybirdLock;

	private bool disposedValue = false;

	/// <summary>
	/// 增加的单元，如果设置为0，就是不增加。如果为小于0，那就是减少，会变成负数的可能。
	/// </summary>
	public int IncreaseTick { get; set; } = 1;

	/// <summary>
	/// 获取当前的计数器的最大的设置值。
	/// </summary>
	public long MaxValue => max;

	/// <summary>
	/// 实例化一个自增信息的对象，包括最大值，初始值，增量值
	/// </summary>
	/// <param name="max">数据的最大值，必须指定</param>
	/// <param name="start">数据的起始值，默认为0</param>
	/// <param name="tick">每次的增量值</param>
	public SoftIncrementCount(long max, long start = 0L, int tick = 1)
	{
		this.start = start;
		this.max = max;
		current = start;
		IncreaseTick = tick;
		hybirdLock = new SimpleHybirdLock();
	}

	/// <summary>
	/// 获取自增信息，获得数据之后，下一次获取将会自增，如果自增后大于最大值，则会重置为最小值，如果小于最小值，则会重置为最大值。
	/// </summary>
	/// <returns>计数自增后的值</returns>
	public long GetCurrentValue()
	{
		hybirdLock.Enter();
		long num = current;
		current += IncreaseTick;
		if (current > max)
		{
			current = start;
		}
		else if (current < start)
		{
			current = max;
		}
		hybirdLock.Leave();
		return num;
	}

	/// <summary>
	/// 重置当前序号的最大值，最大值应该大于初始值，如果当前值大于最大值，则当前值被重置为最大值
	/// </summary>
	/// <param name="max">最大值</param>
	public void ResetMaxValue(long max)
	{
		hybirdLock.Enter();
		if (max > start)
		{
			if (max < current)
			{
				current = start;
			}
			this.max = max;
		}
		hybirdLock.Leave();
	}

	/// <summary>
	/// 重置当前序号的初始值，需要小于最大值，如果当前值小于初始值，则当前值被重置为初始值。
	/// </summary>
	/// <param name="start">初始值</param>
	public void ResetStartValue(long start)
	{
		hybirdLock.Enter();
		if (start < max)
		{
			if (current < start)
			{
				current = start;
			}
			this.start = start;
		}
		hybirdLock.Leave();
	}

	/// <summary>
	/// 将当前的值重置为初始值。
	/// </summary>
	public void ResetCurrentValue()
	{
		hybirdLock.Enter();
		current = start;
		hybirdLock.Leave();
	}

	/// <summary>
	/// 将当前的值重置为指定值，该值不能大于max，如果大于max值，就会自动设置为max
	/// </summary>
	/// <param name="value">指定的数据值</param>
	public void ResetCurrentValue(long value)
	{
		hybirdLock.Enter();
		if (value > max)
		{
			current = max;
		}
		else if (value < start)
		{
			current = start;
		}
		else
		{
			current = value;
		}
		hybirdLock.Leave();
	}

	public override string ToString()
	{
		return $"SoftIncrementCount[{current}]";
	}

	private void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				hybirdLock.Dispose();
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
