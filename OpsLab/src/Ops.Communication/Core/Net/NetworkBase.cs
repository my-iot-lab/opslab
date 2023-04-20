using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Ops.Communication.Core.Message;
using Ops.Communication.Utils;

namespace Ops.Communication.Core.Net;

/// <summary>
/// 本系统所有网络类的基类，该类为抽象类，无法进行实例化，如果想使用里面的方法来实现自定义的网络通信，请通过继承使用。
/// </summary>
/// <remarks>
/// 本类提供了丰富的底层数据的收发支持，包含<see cref="INetMessage" />消息的接收。
/// </remarks>
public abstract class NetworkBase
{
	/// <summary>
	/// 对客户端而言是的通讯用的套接字，对服务器来说是用于侦听的套接字。
	/// </summary>
	protected Socket CoreSocket = null;

	/// <summary>
	/// 文件传输的时候的缓存大小，直接影响传输的速度，值越大，传输速度越快，越占内存，默认为100K大小。
	/// </summary>
	protected int fileCacheSize = 102400;

	private int connectErrorCount = 0;

	/// <summary>
	/// 组件的日志工具，支持日志记录，只要实例化后，当前网络的基本信息，就以DEBUG等级进行输出。
	/// </summary>
	/// <remarks>
	/// 只要实例化即可以记录日志，实例化的对象需要实现接口 <see cref="ILogger" /> ，
	/// 你可以实现基于 <see cref="ILogger" />  的对象。
	/// </remarks>
	public ILogger Logger { get; set; }

	/// <summary>
	/// 网络类的身份令牌，在hsl协议的模式下会有效，在和设备进行通信的时候是无效的。
	/// </summary>
	/// <remarks>
	/// 适用于Hsl协议相关的网络通信类，不适用于设备交互类。
	/// </remarks>
	public Guid Token { get; set; } = Guid.Empty;

	/// <summary>
	/// 接收固定长度的字节数组，允许指定超时时间，默认为60秒，当length大于0时，接收固定长度的数据内容，当length小于0时，接收不大于2048长度的随机数据信息
	/// </summary>
	/// <param name="socket">网络通讯的套接字</param>
	/// <param name="length">准备接收的数据长度，当length大于0时，接收固定长度的数据内容，当length小于0时，接收不大于2048长度的随机数据信息</param>
	/// <param name="timeOut">单位：毫秒，超时时间，默认为60秒，如果设置小于0，则不检查超时时间</param>
	/// <param name="reportProgress">当前接收数据的进度报告，有些协议支持传输非常大的数据内容，可以给与进度提示的功能</param>
	/// <returns>包含了字节数据的结果类</returns>
	/// <exception cref="RemoteCloseException"></exception>
	protected OperateResult<byte[]> Receive(Socket socket, int length, int timeOut = 60000, Action<long, long> reportProgress = null)
	{
		if (length == 0)
		{
			return OperateResult.Ok(Array.Empty<byte>());
		}

		try
		{
			socket.ReceiveTimeout = timeOut;
			if (length > 0)
			{
				byte[] value = NetSupport.ReadBytesFromSocket(socket, length, reportProgress);
				return OperateResult.Ok(value);
			}

			byte[] array = new byte[2048];
			int num = socket.Receive(array);
			if (num == 0)
			{
				throw new RemoteCloseException();
			}

			return OperateResult.Ok(SoftBasic.ArraySelectBegin(array, num));
		}
		catch (RemoteCloseException)
		{
			socket?.Close();
			if (connectErrorCount < 1000000000)
			{
				connectErrorCount++;
			}
			return new OperateResult<byte[]>(-connectErrorCount, "Socket Exception -> RemoteClosedConnection");
		}
		catch (Exception ex2)
		{
			socket?.Close();
			if (connectErrorCount < 1000000000)
			{
				connectErrorCount++;
			}
			return new OperateResult<byte[]>(-connectErrorCount, "Socket Exception -> " + ex2.Message);
		}
	}

	/// <summary>
	/// 接收一条完整的 <seealso cref="INetMessage" /> 数据内容，需要指定超时时间，单位为毫秒。
	/// </summary>
	/// <param name="socket">网络的套接字</param>
	/// <param name="timeOut">超时时间，单位：毫秒</param>
	/// <param name="netMessage">消息的格式定义</param>
	/// <param name="reportProgress">接收消息的时候的进度报告</param>
	/// <returns>带有是否成功的byte数组对象</returns>
	protected OperateResult<byte[]> ReceiveByMessage(Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null)
	{
		if (netMessage == null)
		{
			return Receive(socket, -1, timeOut);
		}

		var operateResult = Receive(socket, netMessage.ProtocolHeadBytesLength, timeOut);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		netMessage.HeadBytes = operateResult.Content;
		int contentLengthByHeadBytes = netMessage.GetContentLengthByHeadBytes();
		var operateResult2 = Receive(socket, contentLengthByHeadBytes, timeOut, reportProgress);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		netMessage.ContentBytes = operateResult2.Content;
		return OperateResult.Ok(SoftBasic.SpliceArray(operateResult.Content, operateResult2.Content));
	}

	/// <summary>
	/// 发送消息给套接字，直到完成的时候返回，经过测试，本方法是线程安全的。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="data">字节数据</param>
	/// <returns>发送是否成功的结果</returns>
	protected OperateResult Send(Socket socket, byte[] data)
	{
		if (data == null)
		{
			return OperateResult.Ok();
		}

		return Send(socket, data, 0, data.Length);
	}

	/// <summary>
	/// 发送消息给套接字，直到完成的时候返回，经过测试，本方法是线程安全的。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="data">字节数据</param>
	/// <param name="offset">偏移的位置信息</param>
	/// <param name="size">发送的数据总数</param>
	/// <returns>发送是否成功的结果</returns>
	protected OperateResult Send(Socket socket, byte[] data, int offset, int size)
	{
		if (data == null)
		{
			return OperateResult.Ok();
		}

		try
		{
			int num = 0;
			do
			{
				int num2 = socket.Send(data, offset, size - num, SocketFlags.None);
				num += num2;
				offset += num2;
			}
			while (num < size);

			return OperateResult.Ok();
		}
		catch (Exception ex)
		{
			socket?.Close();
			if (connectErrorCount < 1000000000)
			{
				connectErrorCount++;
			}
			return new OperateResult<byte[]>(-connectErrorCount, ex.Message);
		}
	}

	/// <summary>
	/// 创建一个新的socket对象并连接到远程的地址，需要指定远程终结点，超时时间（单位是毫秒），如果需要绑定本地的IP或是端口，传入 local对象。
	/// </summary>
	/// <param name="endPoint">连接的目标终结点</param>
	/// <param name="timeOut">连接的超时时间</param>
	/// <param name="local">如果需要绑定本地的IP地址，就需要设置当前的对象</param>
	/// <returns>返回套接字的封装结果对象</returns>
	protected OperateResult<Socket> CreateSocketAndConnect(IPEndPoint endPoint, int timeOut, IPEndPoint local = null)
	{
		int num = 0;
		while (true)
		{
			num++;
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			var opsTimeOut = OpsTimeOut.HandleTimeOutCheck(socket, timeOut);

			try
			{
				if (local != null)
				{
					socket.Bind(local);
				}
				socket.Connect(endPoint);
				connectErrorCount = 0;
				opsTimeOut.IsSuccessful = true;

				return OperateResult.Ok(socket);
			}
			catch (Exception ex)
			{
				socket?.Close();
				opsTimeOut.IsSuccessful = true;
				if (connectErrorCount < 1000000000)
				{
					connectErrorCount++;
				}

				// 超时之前（500ms），可重试一次。
				if (opsTimeOut.GetConsumeTime() < TimeSpan.FromMilliseconds(500.0) && num < 2)
				{
					Thread.Sleep(100);
					continue;
				}

				if (opsTimeOut.IsTimeout)
				{
					return new OperateResult<Socket>(-connectErrorCount, $"ConnectTimeout, endPoint: { endPoint}, timeOut: {timeOut} ms");
				}

				return new OperateResult<Socket>(-connectErrorCount, $"Socket Connect {endPoint} Exception -> {ex.Message}");
			}
		}
	}

	/// <summary>
	/// 检查当前的头子节信息的令牌是否是正确的，仅用于某些特殊的协议实现
	/// </summary>
	/// <param name="headBytes">头子节数据</param>
	/// <returns>令牌是验证成功</returns>
	protected bool CheckRemoteToken(byte[] headBytes)
	{
		return SoftBasic.IsByteTokenEqual(headBytes, Token);
	}

	/// <summary>
	/// [自校验] 发送字节数据并确认对方接收完成数据，如果结果异常，则结束通讯。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="headCode">头指令</param>
	/// <param name="customer">用户指令</param>
	/// <param name="send">发送的数据</param>
	/// <returns>是否发送成功</returns>
	protected OperateResult SendBaseAndCheckReceive(Socket socket, int headCode, int customer, byte[] send)
	{
		send = OpsProtocol.CommandBytes(headCode, customer, Token, send);
		var operateResult = Send(socket, send);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReceiveLong(socket);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		if (operateResult2.Content != send.Length)
		{
			socket?.Close();
			return new OperateResult("CommandLengthCheckFailed");
		}
		return operateResult2;
	}

	/// <summary>
	/// [自校验] 直接发送字符串数组并确认对方接收完成数据，如果结果异常，则结束通讯。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="customer">用户指令</param>
	/// <param name="name">用户名</param>
	/// <param name="pwd">密码</param>
	/// <returns>是否发送成功</returns>
	protected OperateResult SendAccountAndCheckReceive(Socket socket, int customer, string name, string pwd)
	{
		return SendBaseAndCheckReceive(socket, 5, customer, OpsProtocol.PackStringArrayToByte(new string[2] { name, pwd }));
	}

	/// <summary>
	/// [自校验] 接收一条完整的同步数据，包含头子节和内容字节，基础的数据，如果结果异常，则结束通讯。
	/// </summary>
	/// <param name="socket">套接字</param>
	/// <param name="timeOut">超时时间设置，如果为负数，则不检查超时</param>
	/// <returns>包含是否成功的结果对象</returns>
	/// <exception cref="ArgumentNullException">result</exception>
	protected OperateResult<byte[], byte[]> ReceiveAndCheckBytes(Socket socket, int timeOut)
	{
		var operateResult = Receive(socket, 32, timeOut);
		if (!operateResult.IsSuccess)
		{
			return operateResult.ConvertError<byte[], byte[]>();
		}

		if (!CheckRemoteToken(operateResult.Content))
		{
			socket?.Close();
			return new OperateResult<byte[], byte[]>("TokenCheckFailed");
		}

		int num = BitConverter.ToInt32(operateResult.Content, 28);
		var operateResult2 = Receive(socket, num, timeOut);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2.ConvertError<byte[], byte[]>();
		}

		var operateResult3 = SendLong(socket, 32 + num);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3.ConvertError<byte[], byte[]>();
		}

		byte[] content = operateResult.Content;
		byte[] content2 = operateResult2.Content;
		content2 = OpsProtocol.CommandAnalysis(content, content2);
		return OperateResult.Ok(content, content2);
	}

	/// <summary>
	/// [自校验] 从网络中接收一个字符串数组，如果结果异常，则结束通讯。
	/// </summary>
	/// <param name="socket">套接字</param>
	/// <param name="timeOut">接收数据的超时时间</param>
	/// <returns>包含是否成功的结果对象</returns>
	protected OperateResult<int, string[]> ReceiveStringArrayContentFromSocket(Socket socket, int timeOut = 30000)
	{
		var operateResult = ReceiveAndCheckBytes(socket, timeOut);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<int, string[]>(operateResult);
		}
		if (BitConverter.ToInt32(operateResult.Content1, 0) != 1005)
		{
			socket?.Close();
			return new OperateResult<int, string[]>("CommandHeadCodeCheckFailed");
		}

		operateResult.Content2 ??= new byte[4];
		return OperateResult.Ok(BitConverter.ToInt32(operateResult.Content1, 4), OpsProtocol.UnPackStringArrayFromByte(operateResult.Content2));
	}

	/// <summary>
	/// 从网络中接收Long数据。
	/// </summary>
	/// <param name="socket">套接字网络</param>
	/// <returns>long数据结果</returns>
	private OperateResult<long> ReceiveLong(Socket socket)
	{
		var operateResult = Receive(socket, 8, -1);
		if (operateResult.IsSuccess)
		{
			return OperateResult.Ok(BitConverter.ToInt64(operateResult.Content, 0));
		}
		return OperateResult.Error<long>(operateResult);
	}

	/// <summary>
	/// 将long数据发送到套接字。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="value">long数据</param>
	/// <returns>是否发送成功</returns>
	private OperateResult SendLong(Socket socket, long value)
	{
		return Send(socket, BitConverter.GetBytes(value));
	}

	/// <summary>
	/// 创建 Socket，并连接远程主机。
	/// </summary>
	/// <param name="endPoint">终结点</param>
	/// <param name="timeOut"></param>
	/// <param name="local">不为 null 时，表示该 Socket 是一个 Bind 的侦听套接字</param>
	/// <returns></returns>
	protected async Task<OperateResult<Socket>> CreateSocketAndConnectAsync(IPEndPoint endPoint, int timeOut, IPEndPoint local = null)
	{
		int connectCount = 0;
		while (true)
		{
			connectCount++;
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			var connectTimeout = OpsTimeOut.HandleTimeOutCheck(socket, timeOut);
			try
			{
				if (local != null)
				{
					socket.Bind(local);
				}
				await Task.Factory.FromAsync(socket.BeginConnect(endPoint, null, socket), socket.EndConnect);
				connectErrorCount = 0;
				connectTimeout.IsSuccessful = true;
				return OperateResult.Ok(socket);
			}
			catch (Exception ex)
			{
				connectTimeout.IsSuccessful = true;
				socket?.Close();
				if (connectErrorCount < 1_000_000_000)
				{
					connectErrorCount++;
				}

				if (!(connectTimeout.GetConsumeTime() < TimeSpan.FromMilliseconds(500.0)) || connectCount >= 2)
				{
					if (connectTimeout.IsTimeout)
					{
						return new OperateResult<Socket>(-connectErrorCount, $"ConnectTimeout, endPoint:{endPoint}, timeOut: {timeOut}ms");
					}
					return new OperateResult<Socket>(-connectErrorCount, "Socket Exception -> " + ex.Message);
				}

				await Task.Delay(100);
			}
		}
	}

	protected async Task<OperateResult<byte[]>> ReceiveAsync(Socket socket, int length, int timeOut = 60000, Action<long, long> reportProgress = null)
	{
		if (length == 0)
		{
			return OperateResult.Ok(Array.Empty<byte>());
		}

		var hslTimeOut = OpsTimeOut.HandleTimeOutCheck(socket, timeOut);
		try
		{
			if (length > 0)
			{
				byte[] buffer = new byte[length];
				int alreadyCount = 0;
				do
				{
					int currentReceiveLength = ((length - alreadyCount > 16384) ? 16384 : (length - alreadyCount));
					int count2 = await Task.Factory.FromAsync(socket.BeginReceive(buffer, alreadyCount, currentReceiveLength, SocketFlags.None, null, socket), socket.EndReceive).ConfigureAwait(false);
					alreadyCount += count2;
					if (count2 > 0)
					{
						hslTimeOut.StartTime = DateTime.Now;
						reportProgress?.Invoke(alreadyCount, length);
						continue;
					}

					throw new RemoteCloseException();
				}
				while (alreadyCount < length);

				hslTimeOut.IsSuccessful = true;
				return OperateResult.Ok(buffer);
			}

			byte[] buffer2 = new byte[2048];
			int count = await Task.Factory.FromAsync(socket.BeginReceive(buffer2, 0, buffer2.Length, SocketFlags.None, null, socket), socket.EndReceive).ConfigureAwait(false);
			if (count == 0)
			{
				throw new RemoteCloseException();
			}

			hslTimeOut.IsSuccessful = true;
			return OperateResult.Ok(SoftBasic.ArraySelectBegin(buffer2, count));
		}
		catch (RemoteCloseException)
		{
			socket?.Close();
			if (connectErrorCount < 1000000000)
			{
				connectErrorCount++;
			}
			hslTimeOut.IsSuccessful = true;
			return new OperateResult<byte[]>(-connectErrorCount, "RemoteClosedConnection");
		}
		catch (Exception ex)
		{
			socket?.Close();
			hslTimeOut.IsSuccessful = true;
			if (connectErrorCount < 1000000000)
			{
				connectErrorCount++;
			}
			if (hslTimeOut.IsTimeout)
			{
				return new OperateResult<byte[]>(-connectErrorCount, $"ReceiveDataTimeout: {hslTimeOut.DelayTime}");
			}
			return new OperateResult<byte[]>(-connectErrorCount, "Socket Exception -> " + ex.Message);
		}
	}

	protected async Task<OperateResult> SendAsync(Socket socket, byte[] data)
	{
		if (data == null)
		{
			return OperateResult.Ok();
		}
		return await SendAsync(socket, data, 0, data.Length).ConfigureAwait(false);
	}

	protected async Task<OperateResult> SendAsync(Socket socket, byte[] data, int offset, int size)
	{
		if (data == null)
		{
			return OperateResult.Ok();
		}

		int alreadyCount = 0;
		try
		{
			do
			{
				int count = await Task.Factory.FromAsync(socket.BeginSend(data, offset, size - alreadyCount, SocketFlags.None, null, socket), socket.EndSend).ConfigureAwait(false);
				alreadyCount += count;
				offset += count;
			}
			while (alreadyCount < size);

			return OperateResult.Ok();
		}
		catch (Exception ex)
		{
			socket?.Close();
			if (connectErrorCount < 1000000000)
			{
				connectErrorCount++;
			}
			return new OperateResult<byte[]>(-connectErrorCount, ex.Message);
		}
	}

	protected async Task<OperateResult<byte[]>> ReceiveByMessageAsync(Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null)
	{
		if (netMessage == null)
		{
			return await ReceiveAsync(socket, -1, timeOut).ConfigureAwait(false);
		}

		var headResult = await ReceiveAsync(socket, netMessage.ProtocolHeadBytesLength, timeOut).ConfigureAwait(false);
		if (!headResult.IsSuccess)
		{
			return headResult;
		}

		netMessage.HeadBytes = headResult.Content;
		int contentLength = netMessage.GetContentLengthByHeadBytes();
		OperateResult<byte[]> contentResult = await ReceiveAsync(socket, contentLength, timeOut, reportProgress).ConfigureAwait(false);
		if (!contentResult.IsSuccess)
		{
			return contentResult;
		}

		netMessage.ContentBytes = contentResult.Content;
		return OperateResult.Ok(SoftBasic.SpliceArray(headResult.Content, contentResult.Content));
	}

	private async Task<OperateResult<long>> ReceiveLongAsync(Socket socket)
	{
		var read = await ReceiveAsync(socket, 8, -1).ConfigureAwait(false);
		if (read.IsSuccess)
		{
			return OperateResult.Ok(BitConverter.ToInt64(read.Content, 0));
		}
		return OperateResult.Error<long>(read);
	}

	private async Task<OperateResult> SendLongAsync(Socket socket, long value)
	{
		return await SendAsync(socket, BitConverter.GetBytes(value)).ConfigureAwait(false);
	}

	protected async Task<OperateResult> SendBaseAndCheckReceiveAsync(Socket socket, int headCode, int customer, byte[] send)
	{
		var send0 = OpsProtocol.CommandBytes(headCode, customer, Token, send);
		OperateResult sendResult = await SendAsync(socket, send0).ConfigureAwait(false);
		if (!sendResult.IsSuccess)
		{
			return sendResult;
		}

		OperateResult<long> checkResult = await ReceiveLongAsync(socket).ConfigureAwait(false);
		if (!checkResult.IsSuccess)
		{
			return checkResult;
		}

		if (checkResult.Content != send0.Length)
		{
			socket?.Close();
			return new OperateResult("CommandLengthCheckFailed");
		}
		return checkResult;
	}

	protected async Task<OperateResult> SendAccountAndCheckReceiveAsync(Socket socket, int customer, string name, string pwd)
	{
		return await SendBaseAndCheckReceiveAsync(socket, 5, customer, OpsProtocol.PackStringArrayToByte(new string[2] { name, pwd })).ConfigureAwait(false);
	}

	protected async Task<OperateResult<byte[], byte[]>> ReceiveAndCheckBytesAsync(Socket socket, int timeout)
	{
		OperateResult<byte[]> headResult = await ReceiveAsync(socket, 32, timeout).ConfigureAwait(false);
		if (!headResult.IsSuccess)
		{
			return OperateResult.Error<byte[], byte[]>(headResult);
		}

		if (!CheckRemoteToken(headResult.Content))
		{
			socket?.Close();
			return new OperateResult<byte[], byte[]>("TokenCheckFailed");
		}

		int contentLength = BitConverter.ToInt32(headResult.Content, 28);
		OperateResult<byte[]> contentResult = await ReceiveAsync(socket, contentLength, timeout).ConfigureAwait(false);
		if (!contentResult.IsSuccess)
		{
			return OperateResult.Error<byte[], byte[]>(contentResult);
		}

		OperateResult checkResult = await SendLongAsync(socket, 32 + contentLength).ConfigureAwait(false);
		if (!checkResult.IsSuccess)
		{
			return OperateResult.Error<byte[], byte[]>(checkResult);
		}

		byte[] head = headResult.Content;
		byte[] content2 = contentResult.Content;
		content2 = OpsProtocol.CommandAnalysis(head, content2);
		return OperateResult.Ok(head, content2);
	}

	protected async Task<OperateResult<int, string[]>> ReceiveStringArrayContentFromSocketAsync(Socket socket, int timeOut = 30000)
	{
		var receive = await ReceiveAndCheckBytesAsync(socket, timeOut).ConfigureAwait(false);
		if (!receive.IsSuccess)
		{
			return OperateResult.Error<int, string[]>(receive);
		}

		if (BitConverter.ToInt32(receive.Content1, 0) != 1005)
		{
			socket?.Close();
			return new OperateResult<int, string[]>("CommandHeadCodeCheckFailed");
		}

		receive.Content2 ??= new byte[4];
		return OperateResult.Ok(BitConverter.ToInt32(receive.Content1, 4), OpsProtocol.UnPackStringArrayFromByte(receive.Content2));
	}

	public override string ToString()
	{
		return "NetworkBase";
	}
}
