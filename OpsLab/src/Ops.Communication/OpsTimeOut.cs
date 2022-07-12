using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace Ops.Communication;

/// <summary>
/// 超时操作的类。
/// </summary>
/// <remarks>
/// 本类自动启动一个静态线程来处理
/// </remarks>
public sealed class OpsTimeOut
{
	private static long s_opsTimeoutId = 0L;
	private static readonly List<OpsTimeOut> s_waitHandleTimeOut = new(128);
	private static readonly object listLock = new();
	private static Thread s_threadCheckTimeOut;
	private static long s_threadUniqueId = 0L;
	private static DateTime s_threadActiveTime;
	private static int s_activeDisableCount = 0;

	/// <summary>
	/// 当前超时对象的唯一ID信息，没实例化一个对象，id信息就会自增1
	/// </summary>
	public long UniqueId { get; private set; }

	/// <summary>
	/// 操作的开始时间
	/// </summary>
	public DateTime StartTime { get; set; }

	/// <summary>
	/// 操作是否成功，当操作完成的时候，需要设置为<c>True</c>，超时检测自动结束。如果一直为<c>False</c>，
	/// 超时检测到超时，设置 <see cref="IsTimeout" /> 为 <c>True</c>
	/// </summary>
	public bool IsSuccessful { get; set; }

	/// <summary>
	/// 延时的时间，单位毫秒。
	/// </summary>
	public int DelayTime { get; set; }

	/// <summary>
	/// 连接超时用的Socket，本超时对象主要针对套接字的连接，接收数据的超时检测，也可以设置为空，用作其他用途的超时检测。
	/// </summary>
	[JsonIgnore]
	public Socket WorkSocket { get; set; }

	/// <summary>
	/// 是否发生了超时的操作，当调用方因为异常结束的时候，需要对<see cref="IsTimeout" />进行判断，是否因为发送了超时导致的异常。
	/// </summary>
	public bool IsTimeout { get; set; }

	/// <summary>
	/// 获取当前检查超时对象的个数
	/// </summary>
	public static int TimeOutCheckCount => s_waitHandleTimeOut.Count;

	/// <summary>
	/// 实例化一个默认的对象
	/// </summary>
	public OpsTimeOut()
	{
		UniqueId = Interlocked.Increment(ref s_opsTimeoutId);
		StartTime = DateTime.Now;
		IsSuccessful = false;
		IsTimeout = false;
	}

	static OpsTimeOut()
	{
		CreateTimeoutCheckThread();
	}

	/// <summary>
	/// 获取到目前为止所花费的时间。
	/// </summary>
	/// <returns>时间信息</returns>
	public TimeSpan GetConsumeTime()
	{
		return DateTime.Now - StartTime;
	}

	public override string ToString()
	{
		return $"TimeOut [{DelayTime}]";
	}

	/// <summary>
	/// 新增一个超时检测的对象，当操作完成的时候，需要自行标记<see cref="OpsTimeOut" />对象的<see cref="IsSuccessful" />为<c>True</c>
	/// </summary>
	/// <param name="timeOut">超时对象</param>
	public static void HandleTimeOutCheck(OpsTimeOut timeOut)
	{
		lock (listLock)
		{
			if ((DateTime.Now - s_threadActiveTime).TotalSeconds > 60.0)
			{
				s_threadActiveTime = DateTime.Now;
				if (Interlocked.Increment(ref s_activeDisableCount) >= 2)
				{
					CreateTimeoutCheckThread();
				}
			}
			s_waitHandleTimeOut.Add(timeOut);
		}
	}

	/// <summary>
	/// 获取当前的所有的等待超时检查对象列表，请勿手动更改对象的属性值
	/// </summary>
	/// <returns>HslTimeOut数组，请勿手动更改对象的属性值</returns>
	public static OpsTimeOut[] GetHslTimeOutsSnapShoot()
	{
		lock (listLock)
		{
			return s_waitHandleTimeOut.ToArray();
		}
	}

	/// <summary>
	/// 新增一个超时检测的对象，需要指定socket，超时时间，返回<see cref="OpsTimeOut" />对象，用作标记完成信息
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="timeout">超时时间，单位为毫秒</param>
	public static OpsTimeOut HandleTimeOutCheck(Socket socket, int timeout)
	{
		var hslTimeOut = new OpsTimeOut
		{
			DelayTime = timeout,
			IsSuccessful = false,
			StartTime = DateTime.Now,
			WorkSocket = socket
		};

		if (timeout > 0)
		{
			HandleTimeOutCheck(hslTimeOut);
		}
		return hslTimeOut;
	}

	private static void CreateTimeoutCheckThread()
	{
		s_threadActiveTime = DateTime.Now;
		s_threadCheckTimeOut = new Thread(CheckTimeOut)
		{
			IsBackground = true,
			Priority = ThreadPriority.AboveNormal,
		};
		s_threadCheckTimeOut.Start(Interlocked.Increment(ref s_threadUniqueId));
	}

	/// <summary>
	/// 检测超时的核心方法，由一个单独的线程运行，线程的优先级很高，当前其他所有的超时信息都可以放到这里处理
	/// </summary>
	/// <param name="obj">需要传入线程的id信息</param>
	private static void CheckTimeOut(object obj)
	{
		long num = (long)obj;
		while (true)
		{
			Thread.Sleep(100);
			if (num != s_threadUniqueId)
			{
				break;
			}

			s_threadActiveTime = DateTime.Now;
			s_activeDisableCount = 0;
			lock (listLock)
			{
				for (int num2 = s_waitHandleTimeOut.Count - 1; num2 >= 0; num2--)
				{
					OpsTimeOut opsTimeOut = s_waitHandleTimeOut[num2];
					if (opsTimeOut.IsSuccessful)
					{
						s_waitHandleTimeOut.RemoveAt(num2);
					}
					else if ((DateTime.Now - opsTimeOut.StartTime).TotalMilliseconds > opsTimeOut.DelayTime)
					{
						if (!opsTimeOut.IsSuccessful)
						{
							opsTimeOut.WorkSocket?.Close();
							opsTimeOut.IsTimeout = true;
						}
						s_waitHandleTimeOut.RemoveAt(num2);
					}
				}
			}
		}
	}
}