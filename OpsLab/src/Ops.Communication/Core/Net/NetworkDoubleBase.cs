using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Ops.Communication.Core.Message;
using Ops.Communication.Extensions;
using Ops.Communication.Utils;

namespace Ops.Communication.Core.Net;

/// <summary>
/// 支持长连接，短连接两个模式的通用客户端基类
/// </summary>
public abstract class NetworkDoubleBase : NetworkBase, IDisposable
{
	private string _ipAddress = "127.0.0.1";
    private bool _disposedValue = false;
    private readonly Lazy<Ping> ping = new(() => new Ping());

    private readonly SimpleHybirdLock _interactiveLock; // 交互的混合锁，保证交互操作的安全性。
    private readonly AsyncSimpleHybirdLock _asyncInteractiveLock; // 异步交互的混合锁，保证交互操作的安全性。

    /// <summary>
    /// 是否是长连接的状态。
    /// </summary>
    public bool IsPersistentConn { get; private set; }

    /// <summary>
    /// 指示长连接的套接字是否处于错误的状态。
    /// </summary>
	/// <remarks>当读取/写入数据时有接收数据超时、远端Socket关闭、Socket异常都会影响该值。</remarks>
    public bool IsSocketError { get; private set; }

    /// <summary>
    /// 设置日志记录报文是否二进制，如果为False，那就使用ASCII码。
    /// </summary>
    public bool LogMsgFormatBinary { get; set; } = true;

	/// <summary>
	/// 当前的数据变换机制，当你需要从字节数据转换类型数据的时候需要。
	/// </summary>
	public IByteTransform ByteTransform { get; set; }

	/// <summary>
	/// 获取或设置连接的超时时间，单位毫秒，默认10s。
	/// </summary>
	public int ConnectTimeOut { get; set; } = 10_000;

	/// <summary>
	/// 获取或设置接收服务器反馈的时间，如果为负数，则不接收反馈
	/// </summary>
	/// <remarks>
	/// 超时的通常原因是服务器端没有配置好，导致访问失败，为了不卡死软件，所以有了这个超时的属性。
	/// </remarks>
	public int ReceiveTimeOut { get; set; } = 5_000;

	/// <summary>
	/// 在读取/写入数据期间还未创建Socket或是Socket有异常时，是否自动取连接服务创建Socket，默认为true。
	/// </summary>
	/// <remarks>
	/// 在有大量请求时，若服务器突然断开，因为锁的缘故会导致后续的请求还会去读取数据，
	/// 在读取数据期间因异常会主动创建Socket并尝试进行连接，又因有超时缘故导致这些请求会在后续依次排队处理。设置不自动连接能请求快速返回响应。
	/// 若设置为false，必须先手动调用 ConnectServer[Async] 方法。
	/// </remarks>
	public bool AutoConnectServerWhenSocketIsErrorOrNull { get; set; } = true;

	/// <summary>
	/// 获取或是设置远程服务器的IP地址，如果是本机测试，那么需要设置为127.0.0.1 
	/// </summary>
	/// <remarks>
	/// 最好实在初始化的时候进行指定，当使用短连接的时候，支持动态更改，切换；当使用长连接后，无法动态更改
	/// </remarks>
	public virtual string IpAddress
    {
        get => _ipAddress;
        set => _ipAddress = OpsHelper.GetIpAddressFromInput(value);
    }

	/// <summary>
	/// 获取或设置服务器的端口号，具体的值需要取决于对方的配置
	/// </summary>
	/// <remarks>
	/// 最好实在初始化的时候进行指定，当使用短连接的时候，支持动态更改，切换；当使用长连接后，无法动态更改
	/// </remarks>
	public int Port { get; set; } = 10000;

	public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置在正式接收对方返回数据前的时候，需要休息的时间，当设置为0的时候，不需要休息。
    /// </summary>
    public int SleepTime { get; set; }

	/// <summary>
	/// 获取或设置客户端的Socket的心跳时间信息
	/// </summary>
	public int SocketKeepAliveTime { get; set; } = -1;

	/// <summary>
	/// 获取或设置连接后处理方法，参数表示连接成功还是失败。
	/// </summary>
	public Action<bool> ConnectServerPostDelegate { get; set; }

    /// <summary>
    /// 表示长连接 Socket 读写异常主动关闭后处理方法。
    /// </summary>
    public Action<int> SocketReadErrorClosedDelegate { get; set; }

    /// <summary>
    /// 默认的无参构造函数 <br />
    /// Default no-parameter constructor
    /// </summary>
    public NetworkDoubleBase()
	{
		_interactiveLock = new();
		_asyncInteractiveLock = new();
        ConnectionId = SoftBasic.GetUniqueStringByGuidAndRandom();
	}

	/// <summary>
	/// 获取一个新的消息对象的方法，需要在继承类里面进行重写。
	/// </summary>
	/// <returns>消息类对象</returns>
	protected virtual INetMessage GetNewNetMessage()
	{
		return null;
	}

	/// <summary>
	/// 在读取数据之前可以调用本方法将客户端设置为长连接模式，相当于跳过了ConnectServer的结果验证，对异形客户端无效，当第一次进行通信时再进行创建连接请求。
	/// </summary>
	public void SetPersistentConnection()
	{
		IsPersistentConn = true;
	}

	/// <summary>
	/// 对当前设备的IP地址进行PING的操作，返回PING的结果，正常来说，返回<see cref="IPStatus.Success" />
	/// </summary>
	/// <param name="timeout">Ping 超时时间</param>
	/// <returns>返回PING的结果</returns>
	/// <exception cref="PingException"></exception>
	public IPStatus PingIpAddress(int timeout = 5000)
	{
		return ping.Value.Send(IpAddress, timeout).Status;
	}

    /// <summary>
    /// 对当前设备的IP地址进行PING的操作，返回PING的结果，正常来说，返回<see cref="IPStatus.Success" />
    /// </summary>
    /// <param name="timeout">Ping 超时时间</param>
    /// <returns>返回PING的结果</returns>
    /// <exception cref="PingException"></exception>
    public async Task<IPStatus> PingIpAddressAsync(int timeout = 5000)
	{
		var pingReply = await ping.Value.SendPingAsync(IpAddress, timeout).ConfigureAwait(false);
		return pingReply.Status;
	}

	/// <summary>
	/// 尝试连接远程的服务器，如果连接成功，就切换短连接模式到长连接模式，后面的每次请求都共享一个通道，使得通讯速度更快速
	/// </summary>
	/// <returns>返回连接结果，如果失败的话（也即IsSuccess为False），包含失败信息</returns>
	public OperateResult ConnectServer()
	{
		IsPersistentConn = true;
		CoreSocket?.Close();

		OperateResult<Socket> operateResult = CreateSocketAndInitialication();
		ConnectServerPostDelegate?.Invoke(operateResult.IsSuccess);
		if (!operateResult.IsSuccess)
		{
			IsSocketError = true;
			operateResult.Content = null;
			return operateResult;
		}

        IsSocketError = false;
        CoreSocket = operateResult.Content;
		if (SocketKeepAliveTime > 0)
		{
			CoreSocket.SetKeepAlive(SocketKeepAliveTime, SocketKeepAliveTime);
		}

		Logger?.LogDebug("{str0} -- NetEngineStart", ToString());
		return operateResult;
	}

	/// <summary>
	/// 手动断开与远程服务器的连接，如果当前是长连接模式，那么就会切换到短连接模式
	/// </summary>
	/// <returns>关闭连接，不需要查看IsSuccess属性查看</returns>
	public OperateResult ConnectClose()
	{
		var operateResult = new OperateResult();
		IsPersistentConn = false;
		_interactiveLock.Enter();
		try
		{
			operateResult = ExtraOnDisconnect(CoreSocket);
			CoreSocket?.Close();
			CoreSocket = null;
		}
		catch
		{
			throw;
		}
		finally
		{
			_interactiveLock.Leave();
		}

		Logger?.LogDebug("{str0} -- NetEngineClose", ToString());
		return operateResult;
	}

	/// <summary>
	/// 根据实际的协议选择是否重写本方法，有些协议在创建连接之后，需要进行一些初始化的信号握手，才能最终建立网络通道。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <returns>是否初始化成功，依据具体的协议进行重写</returns>
	protected virtual OperateResult InitializationOnConnect(Socket socket)
	{
		return OperateResult.Ok();
	}

	/// <summary>
	/// 根据实际的协议选择是否重写本方法，有些协议在断开连接之前，需要发送一些报文来关闭当前的网络通道
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <example>
	/// 目前暂无相关的示例，组件支持的协议都不用实现这个方法。
	/// </example>
	/// <returns>当断开连接时额外的操作结果</returns>
	protected virtual OperateResult ExtraOnDisconnect(Socket socket)
	{
		return OperateResult.Ok();
	}

	/// <summary>
	/// 和服务器交互完成的时候调用的方法，可以根据读写结果进行一些额外的操作，具体的操作需要根据实际的需求来重写实现
	/// </summary>
	/// <param name="read">读取结果</param>
	protected virtual void ExtraAfterReadFromCoreServer(OperateResult read)
	{
	}

	/// <summary>
	/// 连接成功后进行一些数据的初始化操作。
	/// </summary>
	/// <param name="socket"></param>
	/// <returns></returns>
	protected virtual Task<OperateResult> InitializationOnConnectAsync(Socket socket)
	{
		return Task.FromResult(OperateResult.Ok());
	}

	/// <summary>
	/// 当断开连接前可做的额外事情。
	/// </summary>
	/// <param name="socket"></param>
	/// <returns></returns>
	protected virtual Task<OperateResult> ExtraOnDisconnectAsync(Socket socket)
	{
		return Task.FromResult(OperateResult.Ok());
	}

    /// <summary>
    /// 创建一个新的Socket对象并尝试连接远程主机。再连接成功后会进行一些数据的初始化操作。
    /// </summary>
    /// <returns></returns>
    private async Task<OperateResult<Socket>> CreateSocketAndInitialicationAsync()
	{
		var result = await CreateSocketAndConnectAsync(new IPEndPoint(IPAddress.Parse(_ipAddress), Port), ConnectTimeOut).ConfigureAwait(false);
		if (result.IsSuccess)
		{
			OperateResult initi = await InitializationOnConnectAsync(result.Content).ConfigureAwait(false);
			if (!initi.IsSuccess)
			{
				result.Content?.Close();
				result.IsSuccess = initi.IsSuccess;
				result.CopyErrorFromOther(initi);
			}
		}
		return result;
	}

	/// <summary>
	/// 获取可用的 Socket 对象。
	/// </summary>
	/// <returns></returns>
	/// <remarks>
	/// 对于长连接，若是有Socket异常或是没有进行连接服务器，会调用连接服务接口进行长连接，否则会使用已连接的Socket；
	/// 非长连接，会创建一个新的Socket并进行数据的初始化操作。
	/// </remarks>
	private async Task<OperateResult<Socket>> GetAvailableSocketAsync()
	{
        // 非长连接，每个请求都会创建一个新的 Socket。
        if (!IsPersistentConn)
		{
            return await CreateSocketAndInitialicationAsync().ConfigureAwait(false);
        }

		if (IsSocketError || CoreSocket is null)
		{
            // 有Socket异常或是还没有进行连接服务器，会重新连接服务器。
            if (AutoConnectServerWhenSocketIsErrorOrNull)
			{
                OperateResult connect = await ConnectServerAsync().ConfigureAwait(false);
                if (!connect.IsSuccess)
                {
                    IsSocketError = true;
                    return OperateResult.Error<Socket>(connect);
                }

                IsSocketError = false;
                return OperateResult.Ok(CoreSocket);
            }

			// 不自动连接服务，抛出异常。
			string err = IsSocketError ? "Socket error" : "Must connect server firstly";
            return new OperateResult<Socket>((int)ErrorCode.SocketException, err);
        }

		// 使用已创建的Socket。
		return OperateResult.Ok(CoreSocket);
	}

	/// <summary>
	/// 连接服务器，并设置状态为长连接状态。
	/// </summary>
	/// <returns></returns>
	public async Task<OperateResult> ConnectServerAsync()
	{
		IsPersistentConn = true;
		CoreSocket?.Close();
        CoreSocket = null;

        OperateResult<Socket> rSocket = await CreateSocketAndInitialicationAsync().ConfigureAwait(false);
		ConnectServerPostDelegate?.Invoke(rSocket.IsSuccess);
		if (!rSocket.IsSuccess)
		{
			IsSocketError = true;
			rSocket.Content = null;
			return rSocket;
		}

        IsSocketError = false;
        CoreSocket = rSocket.Content;
		Logger?.LogDebug("{str0}-- NetEngineStart", ToString());
		return rSocket;
	}

	/// <summary>
	/// 关闭连接。
	/// </summary>
	/// <returns></returns>
	public async Task<OperateResult> ConnectCloseAsync()
	{
		IsPersistentConn = false;

        await _asyncInteractiveLock.EnterAsync().ConfigureAwait(false);
		OperateResult result;
		try
		{
			result = await ExtraOnDisconnectAsync(CoreSocket).ConfigureAwait(false);
			CoreSocket?.Close();
			CoreSocket = null;
		}
		catch
		{
			throw;
		}
		finally
		{
            _asyncInteractiveLock.Leave();
		}

		Logger?.LogDebug("{str0} -- NetEngineClose", ToString());
		return result;
	}

    /// <summary>
    /// 发送数据并接收响应数据（若有设置响应）。
    /// </summary>
    /// <param name="socket">Socket对象</param>
    /// <param name="send">要发生的数据</param>
    /// <param name="hasResponseData">是否有响应数据。注：若有响应数据，会在发送后并接收响应数据，否则不会进行数据接收操作。</param>
    /// <param name="usePackAndUnpack">是否对命令进行打包处理。</param>
    /// <returns></returns>
	/// <remarks>若有设置接收响应数据，<see cref="ReceiveTimeOut"/> 必须大于 0，允许在一定时间内等待接收数据。</remarks>
    public virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(Socket socket, byte[] send, bool hasResponseData = true, bool usePackAndUnpack = true)
	{
		byte[] sendValue = usePackAndUnpack ? PackCommandWithHeader(send) : send;
		Logger?.LogDebug("{str0} Send : {str1}", ToString(), LogMsgFormatBinary ? sendValue.ToHexString(' ') : Encoding.ASCII.GetString(sendValue));

		var netMessage = GetNewNetMessage();
		if (netMessage != null)
		{
			netMessage.SendBytes = sendValue;
		}

		OperateResult sendResult = await SendAsync(socket, sendValue).ConfigureAwait(false);
		if (!sendResult.IsSuccess)
		{
            return OperateResult.Error<byte[]>(sendResult);
		}

		if (ReceiveTimeOut < 0)
		{
			return OperateResult.Ok(Array.Empty<byte>());
		}

		if (!hasResponseData)
		{
			return OperateResult.Ok(Array.Empty<byte>());
		}

		if (SleepTime > 0)
		{
			await Task.Delay(SleepTime).ConfigureAwait(false);
		}

		var resultReceive = await ReceiveByMessageAsync(socket, ReceiveTimeOut, netMessage).ConfigureAwait(false);
		if (!resultReceive.IsSuccess)
		{
            return resultReceive;
		}

		Logger?.LogDebug("{str0} Receive: {str1}", ToString(), LogMsgFormatBinary ? resultReceive.Content.ToHexString(' ') : Encoding.ASCII.GetString(resultReceive.Content));
		if (netMessage != null && !netMessage.CheckHeadBytesLegal(base.Token.ToByteArray()))
		{
			socket?.Close();
            return new OperateResult<byte[]>((int)ErrorCode.CommandHeadCodeCheckFailed, $"CommandHeadCodeCheckFailed{Environment.NewLine}" +
				$"Send: {SoftBasic.ByteToHexString(sendValue, ' ')}{Environment.NewLine}" +
				$"Receive: {SoftBasic.ByteToHexString(resultReceive.Content, ' ')}");
		}

		return usePackAndUnpack ? UnpackResponseContent(sendValue, resultReceive.Content) : resultReceive;
	}

    /// <summary>
    /// 发送数据并接收响应数据。
    /// </summary>
    /// <param name="send">要发送的数据。</param>
    /// <returns></returns>
	/// <remarks></remarks>
    public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send)
	{
		return await ReadFromCoreServerAsync(send, hasResponseData: true).ConfigureAwait(false);
	}

    private async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send, bool hasResponseData, bool usePackAndUnpack = true)
	{
		var result = new OperateResult<byte[]>();

        // 锁，只允许单线程执行
        await _asyncInteractiveLock.EnterAsync().ConfigureAwait(false); 
		OperateResult<Socket> resultSocket;
		try
		{
			resultSocket = await GetAvailableSocketAsync().ConfigureAwait(false);
			if (!resultSocket.IsSuccess)
			{
                SocketReadErrorClosedDelegate?.Invoke(resultSocket.ErrorCode);

                IsSocketError = true;
				result.CopyErrorFromOther(resultSocket);
				return result;
			}

			OperateResult<byte[]> read = await ReadFromCoreServerAsync(resultSocket.Content, send, hasResponseData, usePackAndUnpack).ConfigureAwait(false);
			if (read.IsSuccess)
			{
				IsSocketError = false;
				result.IsSuccess = read.IsSuccess;
				result.Content = read.Content;
				result.Message = "Success";
			}
			else
			{
                SocketReadErrorClosedDelegate?.Invoke(read.ErrorCode);

                IsSocketError = true;
				result.CopyErrorFromOther(read);
			}
			ExtraAfterReadFromCoreServer(read);
		}
		catch
		{
			throw;
		}
		finally
		{
            _asyncInteractiveLock.Leave();
		}

		if (!IsPersistentConn)
		{
			resultSocket?.Content?.Close();
		}
		return result;
	}

	/// <summary>
	/// 对当前的命令进行打包处理，通常是携带命令头内容，标记当前的命令的长度信息，需要进行重写，否则默认不打包
	/// </summary>
	/// <param name="command">发送的数据命令内容</param>
	/// <returns>打包之后的数据结果信息</returns>
	protected virtual byte[] PackCommandWithHeader(byte[] command)
	{
		return command;
	}

	/// <summary>
	/// 根据对方返回的报文命令，对命令进行基本的拆包，例如各种Modbus协议拆包为统一的核心报文，还支持对报文的验证
	/// </summary>
	/// <param name="send">发送的原始报文数据</param>
	/// <param name="response">设备方反馈的原始报文内容</param>
	/// <returns>返回拆包之后的报文信息，默认不进行任何的拆包操作</returns>
	protected virtual OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
	{
		return OperateResult.Ok(response);
	}

	/// <summary>
	/// 获取本次操作的可用的网络通道，如果是短连接，就重新生成一个新的网络通道，如果是长连接，就复用当前的网络通道。
	/// </summary>
	/// <returns>是否成功，如果成功，使用这个套接字</returns>
	private OperateResult<Socket> GetAvailableSocket()
	{
        // 非长连接，每个请求都会创建一个新的 Socket。
        if (!IsPersistentConn)
		{
			return CreateSocketAndInitialication();
		}
       
        if (IsSocketError || CoreSocket is null)
		{
            // 有Socket异常或是还没有进行连接服务器，会重新连接服务器。
            if (AutoConnectServerWhenSocketIsErrorOrNull)
			{
                OperateResult operateResult = ConnectServer();
                if (!operateResult.IsSuccess)
                {
                    IsSocketError = true;
                    return OperateResult.Error<Socket>(operateResult);
                }

                IsSocketError = false;
                return OperateResult.Ok(CoreSocket);
            }

            // 不自动连接服务，抛出异常。
            string err = IsSocketError ? "Socket error" : "Must connect server firstly";
            return new OperateResult<Socket>((int)ErrorCode.SocketException, err);
        }

        // 使用已创建的Socket。
        return OperateResult.Ok(CoreSocket);
	}

	/// <summary>
	/// 尝试连接服务器，如果成功，并执行<see cref="InitializationOnConnect(Socket)" />的方法进行数据初始化操作，并返回最终的结果。
	/// </summary>
	/// <returns>带有socket的结果对象</returns>
	private OperateResult<Socket> CreateSocketAndInitialication()
	{
		OperateResult<Socket> operateResult = CreateSocketAndConnect(new IPEndPoint(IPAddress.Parse(_ipAddress), Port), ConnectTimeOut);
		if (operateResult.IsSuccess)
		{
			OperateResult operateResult2 = InitializationOnConnect(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				operateResult.Content?.Close();
				operateResult.IsSuccess = operateResult2.IsSuccess;
				operateResult.CopyErrorFromOther(operateResult2);
			}
		}
		return operateResult;
	}

	/// <summary>
	/// 将数据报文发送指定的网络通道上，根据当前指定的<see cref="INetMessage" />类型，返回一条完整的数据指令。
	/// </summary>
	/// <param name="socket">指定的套接字</param>
	/// <param name="send">发送的完整的报文信息</param>
	/// <param name="hasResponseData">是否有等待的数据返回，默认为 true</param>
	/// <param name="usePackAndUnpack">是否需要对命令重新打包，在重写<see cref="PackCommandWithHeader(byte[])" />方法后才会有影响</param>
	/// <remarks>
	/// 无锁的基于套接字直接进行叠加协议的操作。
	/// </remarks>
	/// <returns>接收的完整的报文信息</returns>
	public virtual OperateResult<byte[]> ReadFromCoreServer(Socket socket, byte[] send, bool hasResponseData = true, bool usePackAndUnpack = true)
	{
		byte[] array = usePackAndUnpack ? PackCommandWithHeader(send) : send;
		Logger?.LogDebug("{str0} Send: {str1}", ToString(), LogMsgFormatBinary ? array.ToHexString(' ') : Encoding.ASCII.GetString(array));

		INetMessage newNetMessage = GetNewNetMessage();
		if (newNetMessage != null)
		{
			newNetMessage.SendBytes = array;
		}

		OperateResult operateResult = Send(socket, array);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}
		if (ReceiveTimeOut < 0)
		{
			return OperateResult.Ok(Array.Empty<byte>());
		}
		if (!hasResponseData)
		{
			return OperateResult.Ok(Array.Empty<byte>());
		}

		if (SleepTime > 0)
		{
			Thread.Sleep(SleepTime);
		}

		var operateResult2 = ReceiveByMessage(socket, ReceiveTimeOut, newNetMessage);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		Logger?.LogDebug("{str0} Receive: {str1}", ToString(), LogMsgFormatBinary ? operateResult2.Content.ToHexString(' ') : Encoding.ASCII.GetString(operateResult2.Content));
		if (newNetMessage != null && !newNetMessage.CheckHeadBytesLegal(base.Token.ToByteArray()))
		{
			socket?.Close();
			return new OperateResult<byte[]>((int)ErrorCode.CommandHeadCodeCheckFailed, $"CommandHeadCodeCheckFailed {Environment.NewLine}" +
				$"Send: {SoftBasic.ByteToHexString(array, ' ')}{Environment.NewLine}" +
				$"Receive: {SoftBasic.ByteToHexString(operateResult2.Content, ' ')}");
		}

		return usePackAndUnpack ? UnpackResponseContent(array, operateResult2.Content) : OperateResult.Ok(operateResult2.Content);
	}

    /// <summary>
    /// 将数据发送到当前的网络通道中，并从网络通道中接收一个<see cref="INetMessage" />指定的完整的报文，网络通道将根据<see cref="GetAvailableSocket" />方法自动获取，本方法是线程安全的。
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <returns></returns>
    public OperateResult<byte[]> ReadFromCoreServer(byte[] send)
	{
		return ReadFromCoreServer(send, hasResponseData: true);
	}

	/// <summary>
	/// 将数据发送到当前的网络通道中，并从网络通道中接收一个<see cref="INetMessage" />指定的完整的报文，网络通道将根据<see cref="GetAvailableSocket" />方法自动获取，本方法是线程安全的。
	/// </summary>
	/// <param name="send">发送的完整的报文信息</param>
	/// <param name="hasResponseData">是否有等待的数据返回，默认为 true</param>
	/// <param name="usePackAndUnpack">是否需要对命令重新打包，在重写<see cref="PackCommandWithHeader(byte[])" />方法后才会有影响</param>
	/// <returns>接收的完整的报文信息</returns>
	/// <remarks>
	/// 本方法用于实现本组件还未实现的一些报文功能，例如有些modbus服务器会有一些特殊的功能码支持，需要收发特殊的报文，详细请看示例
	/// </remarks>
	private OperateResult<byte[]> ReadFromCoreServer(byte[] send, bool hasResponseData, bool usePackAndUnpack = true)
	{
		var operateResult = new OperateResult<byte[]>();
		OperateResult<Socket> operateResult2;
		_interactiveLock.Enter();
		try
		{
			operateResult2 = GetAvailableSocket();
			if (!operateResult2.IsSuccess)
			{
                SocketReadErrorClosedDelegate?.Invoke(operateResult2.ErrorCode);

                IsSocketError = true;
				operateResult.CopyErrorFromOther(operateResult2);
				return operateResult;
			}

			var operateResult3 = ReadFromCoreServer(operateResult2.Content, send, hasResponseData, usePackAndUnpack);
			if (operateResult3.IsSuccess)
			{
				IsSocketError = false;
				operateResult.IsSuccess = operateResult3.IsSuccess;
				operateResult.Content = operateResult3.Content;
				operateResult.Message = "Success";
			}
			else
			{
                SocketReadErrorClosedDelegate?.Invoke(operateResult3.ErrorCode);

                IsSocketError = true;
				operateResult.CopyErrorFromOther(operateResult3);
			}
			ExtraAfterReadFromCoreServer(operateResult3);
		}
		catch
		{
			throw;
		}
		finally
		{
			_interactiveLock.Leave();
		}

		if (!IsPersistentConn)
		{
			operateResult2?.Content?.Close();
		}
		return operateResult;
	}

	/// <summary>
	/// 释放当前的资源，并自动关闭长连接，如果设置了的话
	/// </summary>
	/// <param name="disposing">是否释放托管的资源信息</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				ConnectClose();
			
				_interactiveLock?.Dispose();
                _asyncInteractiveLock?.Dispose();
            }
			_disposedValue = true;
		}
	}

	/// <summary>
	/// 释放当前的资源
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public override string ToString()
	{
		return $"NetworkDoubleBase<{GetNewNetMessage().GetType()}, {ByteTransform.GetType()}>[{IpAddress}:{Port}]";
	}
}
