namespace Ops.Communication.Infrastructure.ConnectPool;

/// <summary>
/// 一个连接池管理器，负责维护多个可用的连接，并且自动清理，扩容，用于快速读写服务器或是PLC时使用。
/// <para>
/// 关于连接池管理，可参考 <see cref="SocketsHttpHandler"/> 中引用的 System.Net.Http.HttpConnectionPoolManager 对象。
/// </para>
/// </summary>
/// <typeparam name="TConnector">管理的连接类，需要支持IConnector接口</typeparam>
public class ConnectPool<TConnector> where TConnector : IConnector
{
	private readonly Func<TConnector> _createConnector;
	private bool _canGetConnector = true;
	private readonly Timer _checkTimer;
	private readonly object _syncLock = new();
	private readonly List<TConnector> _connectors = [];

	/// <summary>
	/// 获取或设置最大的连接数，当实际的连接数超过最大的连接数的时候，就会进行阻塞，直到有新的连接对象为止。
	/// </summary>
	public int MaxConnector { get; set; } = 10;

	/// <summary>
	/// 获取或设置当前连接过期的时间，单位秒，默认30秒，也就是说，当前的连接在设置的时间段内未被使用，就进行释放连接，减少内存消耗。
	/// </summary>
	public int ConectionExpireTime { get; set; } = 30;

	/// <summary>
	/// 当前已经使用的连接数，会根据使用的频繁程度进行动态的变化。
	/// </summary>
	public int UsedConnector { get; private set; }

	/// <summary>
	/// 当前已经使用的连接数的峰值，可以用来衡量当前系统的适用的连接池上限。
	/// </summary>
	public int UsedConnectorMax { get; private set; }

	/// <summary>
	/// 实例化一个连接池对象，需要指定如果创建新实例的方法。
	/// </summary>
	/// <param name="createConnector">创建连接对象的委托</param>
	public ConnectPool(Func<TConnector> createConnector)
	{
		_createConnector = createConnector;
		// 采用基于线程池的计时器处理过期连接，其是一个简单的轻型计时器。所有的 Timer 对象只使用了一个线程来管理。
		var state = new WeakReference<ConnectPool<TConnector>>(this);
		_checkTimer = new Timer(TimerCheckBackground, state, 10000, 30000);
	}

	/// <summary>
	/// 获取一个可用的连接对象，如果已经达到上限，就进行阻塞等待。当使用完连接对象的时候，需要调用 ConnectPool 的 ReturnConnector 方法归还连接对象。
	/// </summary>
	/// <returns>可用的连接对象</returns>
	public TConnector GetAvailableConnector()
	{
		while (!_canGetConnector)
		{
			Thread.Sleep(20);
		}

		TConnector connector = default;
		lock (_syncLock)
		{
			for (int i = 0; i < _connectors.Count; i++)
			{
				if (!_connectors[i].IsUsing)
				{
					var connector2 = _connectors[i];
					connector2.IsUsing = true;
					connector = connector2;
					break;
				}
			}

			if (connector == null)
			{
				connector = _createConnector();
				connector.IsUsing = true;
				connector.Open();
				_connectors.Add(connector);
				UsedConnector = _connectors.Count;
				UsedConnectorMax = Math.Max(UsedConnector, UsedConnectorMax);
				if (UsedConnector == MaxConnector)
				{
					_canGetConnector = false;
				}
			}
			connector.LatestTime = DateTime.Now;
		}

		return connector;
	}

	/// <summary>
	/// 使用完之后需要通知连接池的管理器，本方法调用之前需要获取到连接对象信息。
	/// </summary>
	/// <param name="connector">连接对象</param>
	public void ReturnConnector(TConnector connector)
	{
		lock (_syncLock)
		{
			int num = _connectors.IndexOf(connector);
			if (num != -1)
			{
				TConnector val = _connectors[num];
				val.IsUsing = false;
			}
		}
	}

	/// <summary>
	/// 将目前连接中的所有对象进行关闭，然后移除队列。
	/// </summary>
	public void ResetAllConnector()
	{
		lock (_syncLock)
		{
			for (int num = _connectors.Count - 1; num >= 0; num--)
			{
				_connectors[num].Close();
				_connectors.RemoveAt(num);
			}
		}
	}

	public void Dispose()
    {
		_checkTimer.Dispose();
	}

	private void TimerCheckBackground(object obj)
	{
		var weakReference = (WeakReference<ConnectPool<TConnector>>)obj;
		if (weakReference.TryGetTarget(out var target))
		{
			target.RemoveStalePools();
		}
	}

	private void RemoveStalePools()
	{
		lock (_syncLock)
		{
			for (int num = _connectors.Count - 1; num >= 0; num--)
			{
				if ((DateTime.Now - _connectors[num].LatestTime).TotalSeconds > ConectionExpireTime && !_connectors[num].IsUsing)
				{
					_connectors[num].Close();
					_connectors.RemoveAt(num);
				}
			}

			UsedConnector = _connectors.Count;
			if (UsedConnector < MaxConnector)
			{
				_canGetConnector = true;
			}
		}
	}
}
