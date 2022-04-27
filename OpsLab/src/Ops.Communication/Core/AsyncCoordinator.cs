namespace Ops.Communication.Core;

/// <summary>
/// 一个线程协调逻辑类，详细参考书籍《CLR Via C#》page:681
/// 这个类可惜没有报告进度的功能
/// </summary>
internal sealed class AsyncCoordinator
{
	private int m_opCount = 1;

	private int m_statusReported = 0;

	private Action<CoordinationStatus> m_callback;

	private Timer m_timer;

	/// <summary>
	/// 每次的操作任务开始前必须调用该方法
	/// </summary>
	/// <param name="opsToAdd"></param>
	public void AboutToBegin(int opsToAdd = 1)
	{
		Interlocked.Add(ref m_opCount, opsToAdd);
	}

	/// <summary>
	/// 在一次任务处理好操作之后，必须调用该方法
	/// </summary>
	public void JustEnded()
	{
		if (Interlocked.Decrement(ref m_opCount) == 0)
		{
			ReportStatus(CoordinationStatus.AllDone);
		}
	}

	/// <summary>
	/// 该方法必须在发起所有的操作之后调用
	/// </summary>
	/// <param name="callback">回调方法</param>
	/// <param name="timeout">超时时间</param>
	public void AllBegun(Action<CoordinationStatus> callback, int timeout = -1)
	{
		m_callback = callback;
		if (timeout != -1)
		{
			m_timer = new Timer(TimeExpired, null, timeout, -1);
		}
		JustEnded();
	}

	/// <summary>
	/// 超时的方法
	/// </summary>
	/// <param name="o"></param>
	private void TimeExpired(object o)
	{
		ReportStatus(CoordinationStatus.Timeout);
	}

	/// <summary>
	/// 取消任务的执行
	/// </summary>
	public void Cancel()
	{
		ReportStatus(CoordinationStatus.Cancel);
	}

	/// <summary>
	/// 生成一次报告
	/// </summary>
	/// <param name="status">报告的状态</param>
	private void ReportStatus(CoordinationStatus status)
	{
		if (Interlocked.Exchange(ref m_statusReported, 1) == 0)
		{
			m_callback(status);
		}
	}

	/// <summary>
	/// 乐观的并发方法模型，具体参照《CLR Via C#》page:686
	/// </summary>
	/// <param name="target">唯一的目标数据</param>
	/// <param name="change">修改数据的算法</param>
	/// <returns></returns>
	public static int Maxinum(ref int target, Func<int, int> change)
	{
		int num = target;
		int num2;
		int num3;
		do
		{
			num2 = num;
			num3 = change(num2);
			num = Interlocked.CompareExchange(ref target, num3, num2);
		}
		while (num2 != num);

		return num3;
	}
}
