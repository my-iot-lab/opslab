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
public class NetworkDoubleBase : NetworkBase, IDisposable
{
	private string ipAddress = "127.0.0.1";

	private int port = 10000;

	private int connectTimeOut = 10000;

	private string connectionId = string.Empty;

	private bool isUseSpecifiedSocket = false;

	/// <summary>
	/// 接收数据的超时时间，单位：毫秒
	/// </summary>
	protected int receiveTimeOut = 5000;

	/// <summary>
	/// 是否是长连接的状态
	/// </summary>
	protected bool isPersistentConn = false;

	/// <summary>
	/// 交互的混合锁，保证交互操作的安全性
	/// </summary>
	protected SimpleHybirdLock InteractiveLock;

	/// <summary>
	/// 指示长连接的套接字是否处于错误的状态
	/// </summary>
	protected bool IsSocketError = false;

	/// <summary>
	/// 设置日志记录报文是否二进制，如果为False，那就使用ASCII码
	/// </summary>
	protected bool LogMsgFormatBinary = true;

	/// <summary>
	/// 是否使用账号登录，这个账户登录的功能是<c>HSL</c>组件创建的服务器特有的功能。
	/// </summary>
	protected bool isUseAccountCertificate = false;

	private string userName = string.Empty;

	private string password = string.Empty;

	private bool disposedValue = false;

	private readonly Lazy<Ping> ping = new(() => new Ping());

	/// <summary>
	/// 当前的数据变换机制，当你需要从字节数据转换类型数据的时候需要。
	/// </summary>
	public IByteTransform ByteTransform { get; set; }

	/// <summary>
	/// 获取或设置连接的超时时间，单位是毫秒
	/// </summary>
	/// <remarks>
	/// 不适用于异形模式的连接。
	/// </remarks>
	public virtual int ConnectTimeOut
	{
		get
		{
			return connectTimeOut;
		}
		set
		{
			if (value >= 0)
			{
				connectTimeOut = value;
			}
		}
	}

	/// <summary>
	/// 获取或设置接收服务器反馈的时间，如果为负数，则不接收反馈
	/// </summary>
	/// <remarks>
	/// 超时的通常原因是服务器端没有配置好，导致访问失败，为了不卡死软件，所以有了这个超时的属性。
	/// </remarks>
	public int ReceiveTimeOut
	{
		get
		{
			return receiveTimeOut;
		}
		set
		{
			receiveTimeOut = value;
		}
	}

	/// <summary>
	/// 获取或是设置远程服务器的IP地址，如果是本机测试，那么需要设置为127.0.0.1 
	/// </summary>
	/// <remarks>
	/// 最好实在初始化的时候进行指定，当使用短连接的时候，支持动态更改，切换；当使用长连接后，无法动态更改
	/// </remarks>
	public virtual string IpAddress
	{
		get
		{
			return ipAddress;
		}
		set
		{
			ipAddress = OpsHelper.GetIpAddressFromInput(value);
		}
	}

	/// <summary>
	/// 获取或设置服务器的端口号，具体的值需要取决于对方的配置
	/// </summary>
	/// <remarks>
	/// 最好实在初始化的时候进行指定，当使用短连接的时候，支持动态更改，切换；当使用长连接后，无法动态更改
	/// </remarks>
	public virtual int Port
	{
		get
		{
			return port;
		}
		set
		{
			port = value;
		}
	}

	public string ConnectionId
	{
		get
		{
			return connectionId;
		}
		set
		{
			connectionId = value;
		}
	}

	/// <summary>
	/// 获取或设置在正式接收对方返回数据前的时候，需要休息的时间，当设置为0的时候，不需要休息。<br />
	/// </summary>
	public int SleepTime { get; set; }

	/// <summary>
	/// 获取或设置绑定的本地的IP地址和端口号信息，如果端口设置为0，代表任何可用的端口<br />
	/// </summary>
	public IPEndPoint LocalBinding { get; set; }

	/// <summary>
	/// 当前的异形连接对象，如果设置了异形连接的话，仅用于异形模式的情况使用<br />
	/// </summary>
	/// <remarks>
	/// 具体的使用方法请参照Demo项目中的异形modbus实现。
	/// </remarks>
	public AlienSession AlienSession { get; set; }

	/// <summary>
	/// 默认的无参构造函数 <br />
	/// Default no-parameter constructor
	/// </summary>
	public NetworkDoubleBase()
	{
		InteractiveLock = new SimpleHybirdLock();
		connectionId = SoftBasic.GetUniqueStringByGuidAndRandom();
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
		isPersistentConn = true;
	}

	/// <summary>
	/// 对当前设备的IP地址进行PING的操作，返回PING的结果，正常来说，返回<see cref="IPStatus.Success" /><br />
	/// </summary>
	/// <returns>返回PING的结果</returns>
	public IPStatus IpAddressPing()
	{
		return ping.Value.Send(IpAddress).Status;
	}

	/// <summary>
	/// 尝试连接远程的服务器，如果连接成功，就切换短连接模式到长连接模式，后面的每次请求都共享一个通道，使得通讯速度更快速
	/// </summary>
	/// <returns>返回连接结果，如果失败的话（也即IsSuccess为False），包含失败信息</returns>
	public OperateResult ConnectServer()
	{
		isPersistentConn = true;
		CoreSocket?.Close();
		OperateResult<Socket> operateResult = CreateSocketAndInitialication();
		if (!operateResult.IsSuccess)
		{
			IsSocketError = true;
			operateResult.Content = null;
			return operateResult;
		}

		CoreSocket = operateResult.Content;
		Logger?.LogDebug(ToString() + " -- NetEngineStart");
		return operateResult;
	}

	/// <summary>
	/// 使用指定的套接字创建异形客户端，在异形客户端的模式下，网络通道需要被动创建。
	/// </summary>
	/// <param name="session">异形客户端对象，查看<seealso cref="NetworkAlienClient" />类型创建的客户端</param>
	/// <returns>通常都为成功</returns>
	/// <remarks>
	/// 不能和之前的长连接和短连接混用
	/// </remarks>
	public OperateResult ConnectServer(AlienSession session)
	{
		isPersistentConn = true;
		isUseSpecifiedSocket = true;
		if (session != null)
		{
			AlienSession?.Socket?.Close();
			if (string.IsNullOrEmpty(ConnectionId))
			{
				ConnectionId = session.DTU;
			}

			if (ConnectionId == session.DTU)
			{
				CoreSocket = session.Socket;
				IsSocketError = !session.IsStatusOk;
				AlienSession = session;
				if (session.IsStatusOk)
				{
					return InitializationOnConnect(session.Socket);
				}
				return new OperateResult();
			}

			IsSocketError = true;
			return new OperateResult();
		}
		IsSocketError = true;
		return new OperateResult();
	}

	/// <summary>
	/// 手动断开与远程服务器的连接，如果当前是长连接模式，那么就会切换到短连接模式
	/// </summary>
	/// <returns>关闭连接，不需要查看IsSuccess属性查看</returns>
	public OperateResult ConnectClose()
	{
		var operateResult = new OperateResult();
		isPersistentConn = false;
		InteractiveLock.Enter();
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
			InteractiveLock.Leave();
		}

		Logger?.LogDebug(ToString() + "-- NetEngineClose");
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
	/// 设置当前的登录的账户名和密码信息，并启用账户验证的功能，账户名为空时设置不生效
	/// </summary>
	/// <param name="userName">账户名</param>
	/// <param name="password">密码</param>
	public void SetLoginAccount(string userName, string password)
	{
		if (!string.IsNullOrEmpty(userName.Trim()))
		{
			isUseAccountCertificate = true;
			this.userName = userName;
			this.password = password;
		}
		else
		{
			isUseAccountCertificate = false;
		}
	}

	/// <summary>
	/// 认证账号，根据已经设置的用户名和密码，进行发送服务器进行账号认证。
	/// </summary>
	/// <param name="socket">套接字</param>
	/// <returns>认证结果</returns>
	protected OperateResult AccountCertificate(Socket socket)
	{
		OperateResult operateResult = SendAccountAndCheckReceive(socket, 1, userName, password);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReceiveStringArrayContentFromSocket(socket);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		if (operateResult2.Content1 == 0)
		{
			return new OperateResult(operateResult2.Content2[0]);
		}
		return OperateResult.Ok();
	}

	protected async Task<OperateResult> AccountCertificateAsync(Socket socket)
	{
		OperateResult send = await SendAccountAndCheckReceiveAsync(socket, 1, userName, password);
		if (!send.IsSuccess)
		{
			return send;
		}

		var read = await ReceiveStringArrayContentFromSocketAsync(socket);
		if (!read.IsSuccess)
		{
			return read;
		}

		if (read.Content1 == 0)
		{
			return new OperateResult(read.Content2[0]);
		}
		return OperateResult.Ok();
	}

	protected virtual Task<OperateResult> InitializationOnConnectAsync(Socket socket)
	{
		return Task.FromResult(OperateResult.Ok());
	}

	protected virtual Task<OperateResult> ExtraOnDisconnectAsync(Socket socket)
	{
		return Task.FromResult(OperateResult.Ok());
	}

	private async Task<OperateResult<Socket>> CreateSocketAndInitialicationAsync()
	{
		var result = await CreateSocketAndConnectAsync(new IPEndPoint(IPAddress.Parse(ipAddress), port), connectTimeOut, LocalBinding);
		if (result.IsSuccess)
		{
			OperateResult initi = await InitializationOnConnectAsync(result.Content);
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
	/// Core 获取可用的 Socket 对象。
	/// </summary>
	/// <returns></returns>
	protected async Task<OperateResult<Socket>> GetAvailableSocketAsync()
	{
		if (isPersistentConn)
		{
			if (isUseSpecifiedSocket)
			{
				if (IsSocketError)
				{
					return new OperateResult<Socket>(ErrorCode.ConnectionIsNotAvailable.Desc());
				}
				return OperateResult.Ok(CoreSocket);
			}

			if (IsSocketError || CoreSocket == null)
			{
				OperateResult connect = await ConnectServerAsync();
				if (!connect.IsSuccess)
				{
					IsSocketError = true;
					return OperateResult.Error<Socket>(connect);
				}

				IsSocketError = false;
				return OperateResult.Ok(CoreSocket);
			}
			return OperateResult.Ok(CoreSocket);
		}

		// 非长连接，每个请求都会创建一个新的 Socket。
		return await CreateSocketAndInitialicationAsync();
	}

	public async Task<OperateResult> ConnectServerAsync()
	{
		isPersistentConn = true;
		CoreSocket?.Close();
		OperateResult<Socket> rSocket = await CreateSocketAndInitialicationAsync();
		if (!rSocket.IsSuccess)
		{
			IsSocketError = true;
			rSocket.Content = null;
			return rSocket;
		}

		CoreSocket = rSocket.Content;
		Logger?.LogDebug(ToString() + "-- NetEngineStart");
		return rSocket;
	}

	public async Task<OperateResult> ConnectCloseAsync()
	{
		isPersistentConn = false;
		InteractiveLock.Enter();
		OperateResult result;
		try
		{
			result = await ExtraOnDisconnectAsync(CoreSocket);
			CoreSocket?.Close();
			CoreSocket = null;
		}
		catch
		{
			throw;
		}
		finally
		{
			InteractiveLock.Leave();
		}

		Logger?.LogDebug(ToString() + "-- NetEngineClose");
		return result;
	}

	public virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(Socket socket, byte[] send, bool hasResponseData = true, bool usePackAndUnpack = true)
	{
		byte[] sendValue = (usePackAndUnpack ? PackCommandWithHeader(send) : send);
		Logger?.LogDebug($"{ToString()} Send : {(LogMsgFormatBinary ? sendValue.ToHexString(' ') : Encoding.ASCII.GetString(sendValue))}");

		var netMessage = GetNewNetMessage();
		if (netMessage != null)
		{
			netMessage.SendBytes = sendValue;
		}

		OperateResult sendResult = await SendAsync(socket, sendValue);
		if (!sendResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(sendResult);
		}

		if (receiveTimeOut < 0)
		{
			return OperateResult.Ok(new byte[0]);
		}

		if (!hasResponseData)
		{
			return OperateResult.Ok(new byte[0]);
		}

		if (SleepTime > 0)
		{
			Thread.Sleep(SleepTime);
		}

		var resultReceive = await ReceiveByMessageAsync(socket, receiveTimeOut, netMessage);
		if (!resultReceive.IsSuccess)
		{
			return resultReceive;
		}

		Logger?.LogDebug($"{ToString()} Receive: {(LogMsgFormatBinary ? resultReceive.Content.ToHexString(' ') : Encoding.ASCII.GetString(resultReceive.Content))}");
		if (netMessage != null && !netMessage.CheckHeadBytesLegal(base.Token.ToByteArray()))
		{
			socket?.Close();
			return new OperateResult<byte[]>($"CommandHeadCodeCheckFailed{Environment.NewLine}" +
				$"Send: {SoftBasic.ByteToHexString(sendValue, ' ')}{Environment.NewLine}" +
				$"Receive: {SoftBasic.ByteToHexString(resultReceive.Content, ' ')}");
		}

		return usePackAndUnpack ? UnpackResponseContent(sendValue, resultReceive.Content) : resultReceive;
	}

	public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send)
	{
		return await ReadFromCoreServerAsync(send, hasResponseData: true);
	}

	public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send, bool hasResponseData, bool usePackAndUnpack = true)
	{
		var result = new OperateResult<byte[]>();
		InteractiveLock.Enter();
		OperateResult<Socket> resultSocket;
		try
		{
			resultSocket = await GetAvailableSocketAsync();
			if (!resultSocket.IsSuccess)
			{
				IsSocketError = true;
				AlienSession?.Offline();
				result.CopyErrorFromOther(resultSocket);
				return result;
			}
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(resultSocket.Content, send, hasResponseData, usePackAndUnpack);
			if (read.IsSuccess)
			{
				IsSocketError = false;
				result.IsSuccess = read.IsSuccess;
				result.Content = read.Content;
				result.Message = "Success";
			}
			else
			{
				IsSocketError = true;
				AlienSession?.Offline();
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
			InteractiveLock.Leave();
		}

		if (!isPersistentConn)
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
	protected OperateResult<Socket> GetAvailableSocket()
	{
		if (isPersistentConn)
		{
			if (isUseSpecifiedSocket)
			{
				if (IsSocketError)
				{
					return new OperateResult<Socket>("ConnectionIsNotAvailable");
				}
				return OperateResult.Ok(CoreSocket);
			}

			if (IsSocketError || CoreSocket == null)
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
			return OperateResult.Ok(CoreSocket);
		}
		return CreateSocketAndInitialication();
	}

	/// <summary>
	/// 尝试连接服务器，如果成功，并执行<see cref="InitializationOnConnect(Socket)" />的初始化方法，并返回最终的结果。
	/// </summary>
	/// <returns>带有socket的结果对象</returns>
	private OperateResult<Socket> CreateSocketAndInitialication()
	{
		OperateResult<Socket> operateResult = CreateSocketAndConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), connectTimeOut, LocalBinding);
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
	/// 将数据报文发送指定的网络通道上，根据当前指定的<see cref="INetMessage" />类型，返回一条完整的数据指令<br />
	/// </summary>
	/// <param name="socket">指定的套接字</param>
	/// <param name="send">发送的完整的报文信息</param>
	/// <param name="hasResponseData">是否有等待的数据返回，默认为 true</param>
	/// <param name="usePackAndUnpack">是否需要对命令重新打包，在重写<see cref="PackCommandWithHeader(Byte[])" />方法后才会有影响</param>
	/// <remarks>
	/// 无锁的基于套接字直接进行叠加协议的操作。
	/// </remarks>
	/// <returns>接收的完整的报文信息</returns>
	public virtual OperateResult<byte[]> ReadFromCoreServer(Socket socket, byte[] send, bool hasResponseData = true, bool usePackAndUnpack = true)
	{
		byte[] array = (usePackAndUnpack ? PackCommandWithHeader(send) : send);
		Logger?.LogDebug($"{ToString()} Send: {(LogMsgFormatBinary ? array.ToHexString(' ') : Encoding.ASCII.GetString(array))}");

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
		if (receiveTimeOut < 0)
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

		var operateResult2 = ReceiveByMessage(socket, receiveTimeOut, newNetMessage);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		Logger?.LogDebug($"{ToString()} Receive : {(LogMsgFormatBinary ? operateResult2.Content.ToHexString(' ') : Encoding.ASCII.GetString(operateResult2.Content))}");
		if (newNetMessage != null && !newNetMessage.CheckHeadBytesLegal(base.Token.ToByteArray()))
		{
			socket?.Close();
			return new OperateResult<byte[]>($"CommandHeadCodeCheckFailed {Environment.NewLine}" +
				$"Send: {SoftBasic.ByteToHexString(array, ' ')}{Environment.NewLine}" +
				$"Receive: {SoftBasic.ByteToHexString(operateResult2.Content, ' ')}");
		}
		return usePackAndUnpack ? UnpackResponseContent(array, operateResult2.Content) : OperateResult.Ok(operateResult2.Content);
	}

	public OperateResult<byte[]> ReadFromCoreServer(byte[] send)
	{
		return ReadFromCoreServer(send, hasResponseData: true);
	}

	/// <summary>
	/// 将数据发送到当前的网络通道中，并从网络通道中接收一个<see cref="INetMessage" />指定的完整的报文，网络通道将根据<see cref="GetAvailableSocket" />方法自动获取，本方法是线程安全的。
	/// </summary>
	/// <param name="send">发送的完整的报文信息</param>
	/// <param name="hasResponseData">是否有等待的数据返回，默认为 true</param>
	/// <param name="usePackAndUnpack">是否需要对命令重新打包，在重写<see cref="PackCommandWithHeader(Byte[])" />方法后才会有影响</param>
	/// <returns>接收的完整的报文信息</returns>
	/// <remarks>
	/// 本方法用于实现本组件还未实现的一些报文功能，例如有些modbus服务器会有一些特殊的功能码支持，需要收发特殊的报文，详细请看示例
	/// </remarks>
	public OperateResult<byte[]> ReadFromCoreServer(byte[] send, bool hasResponseData, bool usePackAndUnpack = true)
	{
		var operateResult = new OperateResult<byte[]>();
		OperateResult<Socket> operateResult2;
		InteractiveLock.Enter();
		try
		{
			operateResult2 = GetAvailableSocket();
			if (!operateResult2.IsSuccess)
			{
				IsSocketError = true;
				AlienSession?.Offline();
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
				IsSocketError = true;
				AlienSession?.Offline();
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
			InteractiveLock.Leave();
		}

		if (!isPersistentConn)
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
		if (!disposedValue)
		{
			if (disposing)
			{
				ConnectClose();
				InteractiveLock?.Dispose();
			}
			disposedValue = true;
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
