using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Ops.Communication.Basic;
using Ops.Communication.Core.Message;

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
	/// 接收一行命令数据，需要自己指定这个结束符，默认超时时间为60秒，也即是60000，单位是毫秒
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="endCode">结束符信息</param>
	/// <param name="timeout">超时时间，默认为60000，单位为毫秒，也就是60秒</param>
	/// <returns>带有结果对象的数据信息</returns>
	protected OperateResult<byte[]> ReceiveCommandLineFromSocket(Socket socket, byte endCode, int timeout = 60000)
	{
		var list = new List<byte>();
		try
		{
			DateTime now = DateTime.Now;
			bool flag = false;
			while ((DateTime.Now - now).TotalMilliseconds < (double)timeout)
			{
				if (socket.Poll(timeout, SelectMode.SelectRead))
				{
					OperateResult<byte[]> operateResult = Receive(socket, 1, timeout);
					if (!operateResult.IsSuccess)
					{
						return operateResult;
					}

					list.AddRange(operateResult.Content);
					if (operateResult.Content[0] == endCode)
					{
						flag = true;
						break;
					}
				}
			}

			if (!flag)
			{
				return new OperateResult<byte[]>(ErrorCode.ReceiveDataTimeout.Desc());
			}
			return OperateResult.Ok(list.ToArray());
		}
		catch (Exception ex)
		{
			socket?.Close();
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 接收一行命令数据，需要自己指定这个结束符，默认超时时间为60秒，也即是60000，单位是毫秒
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="endCode1">结束符1信息</param>
	/// <param name="endCode2">结束符2信息</param>
	/// /// <param name="timeout">超时时间，默认无穷大，单位毫秒</param>
	/// <returns>带有结果对象的数据信息</returns>
	protected OperateResult<byte[]> ReceiveCommandLineFromSocket(Socket socket, byte endCode1, byte endCode2, int timeout = 60000)
	{
		var list = new List<byte>();
		try
		{
			DateTime now = DateTime.Now;
			bool flag = false;
			while ((DateTime.Now - now).TotalMilliseconds < (double)timeout)
			{
				if (socket.Poll(timeout, SelectMode.SelectRead))
				{
					OperateResult<byte[]> operateResult = Receive(socket, 1, timeout);
					if (!operateResult.IsSuccess)
					{
						return operateResult;
					}

					list.AddRange(operateResult.Content);
					if (operateResult.Content[0] == endCode2 && list.Count > 1 && list[list.Count - 2] == endCode1)
					{
						flag = true;
						break;
					}
				}
			}

			if (!flag)
			{
				return new OperateResult<byte[]>(ErrorCode.ReceiveDataTimeout.Desc());
			}
			return OperateResult.Ok(list.ToArray());
		}
		catch (Exception ex)
		{
			socket?.Close();
			return new OperateResult<byte[]>(ex.Message);
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
	/// 创建一个新的socket对象并连接到远程的地址，默认超时时间为10秒钟，需要指定ip地址以及端口号信息。
	/// </summary>
	/// <param name="ipAddress">Ip地址</param>
	/// <param name="port">端口号</param>
	/// <returns>返回套接字的封装结果对象</returns>
	protected OperateResult<Socket> CreateSocketAndConnect(string ipAddress, int port)
	{
		return CreateSocketAndConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), 10000);
	}

	/// <summary>
	/// 创建一个新的socket对象并连接到远程的地址，需要指定ip地址以及端口号信息，还有超时时间，单位是毫秒。
	/// </summary>
	/// <param name="ipAddress">Ip地址</param>
	/// <param name="port">端口号</param>
	/// <param name="timeOut">连接的超时时间</param>
	/// <returns>返回套接字的封装结果对象</returns>
	protected OperateResult<Socket> CreateSocketAndConnect(string ipAddress, int port, int timeOut)
	{
		return CreateSocketAndConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), timeOut);
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
			var hslTimeOut = OpsTimeOut.HandleTimeOutCheck(socket, timeOut);

			try
			{
				if (local != null)
				{
					socket.Bind(local);
				}
				socket.Connect(endPoint);
				connectErrorCount = 0;
				hslTimeOut.IsSuccessful = true;

				return OperateResult.Ok(socket);
			}
			catch (Exception ex)
			{
				socket?.Close();
				hslTimeOut.IsSuccessful = true;
				if (connectErrorCount < 1000000000)
				{
					connectErrorCount++;
				}

				if (hslTimeOut.GetConsumeTime() < TimeSpan.FromMilliseconds(500.0) && num < 2)
				{
					Thread.Sleep(100);
					continue;
				}

				if (hslTimeOut.IsTimeout)
				{
					return new OperateResult<Socket>(-connectErrorCount, $"ConnectTimeout, endPoint: { endPoint}, timeOut: {timeOut} ms");
				}

				return new OperateResult<Socket>(-connectErrorCount, $"Socket Connect {endPoint} Exception -> {ex.Message}");
			}
		}
	}

	/// <summary>
	/// 读取流中的数据到缓存区，读取的长度需要按照实际的情况来判断
	/// </summary>
	/// <param name="stream">数据流</param>
	/// <param name="buffer">缓冲区</param>
	/// <returns>带有成功标志的读取数据长度</returns>
	protected OperateResult<int> ReadStream(Stream stream, byte[] buffer)
	{
		var manualResetEvent = new ManualResetEvent(initialState: false);
		var fileStateObject = new FileStateObject
		{
			WaitDone = manualResetEvent,
			Stream = stream,
			DataLength = buffer.Length,
			Buffer = buffer
		};

		try
		{
			stream.BeginRead(buffer, 0, fileStateObject.DataLength, ReadStreamCallBack, fileStateObject);
		}
		catch (Exception ex)
		{
			fileStateObject = null;
			manualResetEvent.Close();
			return new OperateResult<int>("stream.BeginRead Exception -> " + ex.Message);
		}

		manualResetEvent.WaitOne();
		manualResetEvent.Close();
		return fileStateObject.IsError ? new OperateResult<int>(fileStateObject.ErrerMsg) : OperateResult.Ok(fileStateObject.AlreadyDealLength);
	}

	private void ReadStreamCallBack(IAsyncResult ar)
	{
		var fileStateObject = ar.AsyncState as FileStateObject;
		if (fileStateObject != null)
		{
			try
			{
				fileStateObject.AlreadyDealLength += fileStateObject.Stream.EndRead(ar);
				fileStateObject.WaitDone.Set();
			}
			catch (Exception ex)
			{
				fileStateObject.IsError = true;
				fileStateObject.ErrerMsg = ex.Message;
				fileStateObject.WaitDone.Set();
			}
		}
	}

	/// <summary>
	/// 将缓冲区的数据写入到流里面去
	/// </summary>
	/// <param name="stream">数据流</param>
	/// <param name="buffer">缓冲区</param>
	/// <returns>是否写入成功</returns>
	protected OperateResult WriteStream(Stream stream, byte[] buffer)
	{
		var manualResetEvent = new ManualResetEvent(initialState: false);
		var fileStateObject = new FileStateObject
		{
			WaitDone = manualResetEvent,
			Stream = stream
		};

		try
		{
			stream.BeginWrite(buffer, 0, buffer.Length, WriteStreamCallBack, fileStateObject);
		}
		catch (Exception ex)
		{
			fileStateObject = null;
			manualResetEvent.Close();
			return new OperateResult("stream.BeginWrite Exception -> " + ex.Message);
		}

		manualResetEvent.WaitOne();
		manualResetEvent.Close();
		if (fileStateObject.IsError)
		{
			return new OperateResult
			{
				Message = fileStateObject.ErrerMsg
			};
		}
		return OperateResult.Ok();
	}

	private void WriteStreamCallBack(IAsyncResult ar)
	{
		var fileStateObject = ar.AsyncState as FileStateObject;
		if (fileStateObject == null)
		{
			return;
		}

		try
		{
			fileStateObject.Stream.EndWrite(ar);
		}
		catch (Exception ex)
		{
			fileStateObject.IsError = true;
			fileStateObject.ErrerMsg = ex.Message;
		}
		finally
		{
			fileStateObject.WaitDone.Set();
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
	/// [自校验] 发送字节数据并确认对方接收完成数据，如果结果异常，则结束通讯。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="customer">用户指令</param>
	/// <param name="send">发送的数据</param>
	/// <returns>是否发送成功</returns>
	protected OperateResult SendBytesAndCheckReceive(Socket socket, int customer, byte[] send)
	{
		return SendBaseAndCheckReceive(socket, 1002, customer, send);
	}

	/// <summary>
	/// [自校验] 直接发送字符串数据并确认对方接收完成数据，如果结果异常，则结束通讯。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="customer">用户指令</param>
	/// <param name="send">发送的数据</param>
	/// <returns>是否发送成功</returns>
	protected OperateResult SendStringAndCheckReceive(Socket socket, int customer, string send)
	{
		byte[] send2 = string.IsNullOrEmpty(send) ? null : Encoding.Unicode.GetBytes(send);
		return SendBaseAndCheckReceive(socket, 1001, customer, send2);
	}

	/// <summary>
	/// [自校验] 直接发送字符串数组并确认对方接收完成数据，如果结果异常，则结束通讯。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="customer">用户指令</param>
	/// <param name="sends">发送的字符串数组</param>
	/// <returns>是否发送成功</returns>
	protected OperateResult SendStringAndCheckReceive(Socket socket, int customer, string[] sends)
	{
		return SendBaseAndCheckReceive(socket, 1005, customer, OpsProtocol.PackStringArrayToByte(sends));
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
	/// <exception cref="T:System.ArgumentNullException">result</exception>
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
	/// [自校验] 从网络中接收一个字符串数据，如果结果异常，则结束通讯
	/// </summary>
	/// <param name="socket">套接字</param>
	/// <param name="timeOut">接收数据的超时时间</param>
	/// <returns>包含是否成功的结果对象</returns>
	protected OperateResult<int, string> ReceiveStringContentFromSocket(Socket socket, int timeOut = 30000)
	{
		var operateResult = ReceiveAndCheckBytes(socket, timeOut);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<int, string>(operateResult);
		}

		if (BitConverter.ToInt32(operateResult.Content1, 0) != 1001)
		{
			socket?.Close();
			return new OperateResult<int, string>("CommandHeadCodeCheckFailed");
		}

		if (operateResult.Content2 == null)
		{
			operateResult.Content2 = new byte[0];
		}
		return OperateResult.Ok(BitConverter.ToInt32(operateResult.Content1, 4), Encoding.Unicode.GetString(operateResult.Content2));
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

		if (operateResult.Content2 == null)
		{
			operateResult.Content2 = new byte[4];
		}
		return OperateResult.Ok(BitConverter.ToInt32(operateResult.Content1, 4), OpsProtocol.UnPackStringArrayFromByte(operateResult.Content2));
	}

	/// <summary>
	/// [自校验] 从网络中接收一串字节数据，如果结果异常，则结束通讯。
	/// </summary>
	/// <param name="socket">套接字的网络</param>
	/// <param name="timeout">超时时间</param>
	/// <returns>包含是否成功的结果对象</returns>
	protected OperateResult<int, byte[]> ReceiveBytesContentFromSocket(Socket socket, int timeout = 30000)
	{
		var operateResult = ReceiveAndCheckBytes(socket, timeout);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<int, byte[]>(operateResult);
		}

		if (BitConverter.ToInt32(operateResult.Content1, 0) != 1002)
		{
			socket?.Close();
			return new OperateResult<int, byte[]>("CommandHeadCodeCheckFailed");
		}
		return OperateResult.Ok(BitConverter.ToInt32(operateResult.Content1, 4), operateResult.Content2);
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
	/// 发送一个流的所有数据到指定的网络套接字，需要指定发送的数据长度，支持按照百分比的进度报告。
	/// </summary>
	/// <param name="socket">套接字</param>
	/// <param name="stream">内存流</param>
	/// <param name="receive">发送的数据长度</param>
	/// <param name="report">进度报告的委托</param>
	/// <param name="reportByPercent">进度报告是否按照百分比报告</param>
	/// <returns>是否成功的结果对象</returns>
	protected OperateResult SendStreamToSocket(Socket socket, Stream stream, long receive, Action<long, long> report, bool reportByPercent)
	{
		byte[] array = new byte[fileCacheSize];
		long num = 0L;
		long num2 = 0L;
		stream.Position = 0L;
		while (num < receive)
		{
			var operateResult = ReadStream(stream, array);
			if (!operateResult.IsSuccess)
			{
				socket?.Close();
				return operateResult;
			}

			num += operateResult.Content;
			byte[] array2 = new byte[operateResult.Content];
			Array.Copy(array, 0, array2, 0, array2.Length);
			var operateResult2 = SendBytesAndCheckReceive(socket, operateResult.Content, array2);
			if (!operateResult2.IsSuccess)
			{
				socket?.Close();
				return operateResult2;
			}

			if (reportByPercent)
			{
				long num3 = num * 100 / receive;
				if (num2 != num3)
				{
					num2 = num3;
					report?.Invoke(num, receive);
				}
			}
			else
			{
				report?.Invoke(num, receive);
			}
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 从套接字中接收所有的数据然后写入到指定的流当中去，需要指定数据的长度，支持按照百分比进行进度报告。
	/// </summary>
	/// <param name="socket">套接字</param>
	/// <param name="stream">数据流</param>
	/// <param name="totalLength">所有数据的长度</param>
	/// <param name="report">进度报告</param>
	/// <param name="reportByPercent">进度报告是否按照百分比</param>
	/// <returns>是否成功的结果对象</returns>
	protected OperateResult WriteStreamFromSocket(Socket socket, Stream stream, long totalLength, Action<long, long> report, bool reportByPercent)
	{
		long num = 0L;
		long num2 = 0L;
		while (num < totalLength)
		{
			var operateResult = ReceiveBytesContentFromSocket(socket, 60000);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}

			num += operateResult.Content1;
			OperateResult operateResult2 = WriteStream(stream, operateResult.Content2);
			if (!operateResult2.IsSuccess)
			{
				socket?.Close();
				return operateResult2;
			}
			if (reportByPercent)
			{
				long num3 = num * 100 / totalLength;
				if (num2 != num3)
				{
					num2 = num3;
					report?.Invoke(num, totalLength);
				}
			}
			else
			{
				report?.Invoke(num, totalLength);
			}
		}
		return OperateResult.Ok();
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
				if (connectErrorCount < 1000000000)
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

	protected async Task<OperateResult<Socket>> CreateSocketAndConnectAsync(string ipAddress, int port)
	{
		return await CreateSocketAndConnectAsync(new IPEndPoint(IPAddress.Parse(ipAddress), port), 10000);
	}

	protected async Task<OperateResult<Socket>> CreateSocketAndConnectAsync(string ipAddress, int port, int timeOut)
	{
		return await CreateSocketAndConnectAsync(new IPEndPoint(IPAddress.Parse(ipAddress), port), timeOut);
	}

	protected async Task<OperateResult<byte[]>> ReceiveAsync(Socket socket, int length, int timeOut = 60000, Action<long, long> reportProgress = null)
	{
		if (length == 0)
		{
			return OperateResult.Ok(new byte[0]);
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
					int count2 = await Task.Factory.FromAsync(socket.BeginReceive(buffer, alreadyCount, currentReceiveLength, SocketFlags.None, null, socket), socket.EndReceive);
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
			int count = await Task.Factory.FromAsync(socket.BeginReceive(buffer2, 0, buffer2.Length, SocketFlags.None, null, socket), socket.EndReceive);
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

	protected async Task<OperateResult<byte[]>> ReceiveCommandLineFromSocketAsync(Socket socket, byte endCode, int timeout = int.MaxValue)
	{
		var bufferArray = new List<byte>();
		try
		{
			DateTime st = DateTime.Now;
			bool bOK = false;
			while ((DateTime.Now - st).TotalMilliseconds < (double)timeout)
			{
				if (socket.Poll(timeout, SelectMode.SelectRead))
				{
					OperateResult<byte[]> headResult = await ReceiveAsync(socket, 1, 5000);
					if (!headResult.IsSuccess)
					{
						return headResult;
					}

					bufferArray.AddRange(headResult.Content);
					if (headResult.Content[0] == endCode)
					{
						bOK = true;
						break;
					}
				}
			}

			if (!bOK)
			{
				return new OperateResult<byte[]>("ReceiveDataTimeout");
			}

			return OperateResult.Ok(bufferArray.ToArray());
		}
		catch (Exception ex2)
		{
			Exception ex = ex2;
			socket?.Close();
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	protected async Task<OperateResult<byte[]>> ReceiveCommandLineFromSocketAsync(Socket socket, byte endCode1, byte endCode2, int timeout = 60000)
	{
		var bufferArray = new List<byte>();
		try
		{
			DateTime st = DateTime.Now;
			bool bOK = false;
			while ((DateTime.Now - st).TotalMilliseconds < timeout)
			{
				if (socket.Poll(timeout, SelectMode.SelectRead))
				{
					OperateResult<byte[]> headResult = await ReceiveAsync(socket, 1, timeout);
					if (!headResult.IsSuccess)
					{
						return headResult;
					}

					bufferArray.AddRange(headResult.Content);
					if (headResult.Content[0] == endCode2 && bufferArray.Count > 1 && bufferArray[^2] == endCode1)
					{
						bOK = true;
						break;
					}
				}
			}

			if (!bOK)
			{
				return new OperateResult<byte[]>(ErrorCode.ReceiveDataTimeout.Desc());
			}
			return OperateResult.Ok(bufferArray.ToArray());
		}
		catch (Exception ex)
		{
			socket?.Close();
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	protected async Task<OperateResult> SendAsync(Socket socket, byte[] data)
	{
		if (data == null)
		{
			return OperateResult.Ok();
		}
		return await SendAsync(socket, data, 0, data.Length);
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
				int count = await Task.Factory.FromAsync(socket.BeginSend(data, offset, size - alreadyCount, SocketFlags.None, null, socket), socket.EndSend);
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
			return await ReceiveAsync(socket, -1, timeOut);
		}

		var headResult = await ReceiveAsync(socket, netMessage.ProtocolHeadBytesLength, timeOut);
		if (!headResult.IsSuccess)
		{
			return headResult;
		}

		netMessage.HeadBytes = headResult.Content;
		int contentLength = netMessage.GetContentLengthByHeadBytes();
		OperateResult<byte[]> contentResult = await ReceiveAsync(socket, contentLength, timeOut, reportProgress);
		if (!contentResult.IsSuccess)
		{
			return contentResult;
		}

		netMessage.ContentBytes = contentResult.Content;
		return OperateResult.Ok(SoftBasic.SpliceArray(headResult.Content, contentResult.Content));
	}

	protected async Task<OperateResult<int>> ReadStreamAsync(Stream stream, byte[] buffer)
	{
		try
		{
			return OperateResult.Ok(await stream.ReadAsync(buffer, 0, buffer.Length));
		}
		catch (Exception ex)
		{
			stream?.Close();
			return new OperateResult<int>(ex.Message);
		}
	}

	protected async Task<OperateResult> WriteStreamAsync(Stream stream, byte[] buffer)
	{
		int alreadyCount = 0;
		try
		{
			await stream.WriteAsync(buffer, alreadyCount, buffer.Length - alreadyCount);
			return OperateResult.Ok(alreadyCount);
		}
		catch (Exception ex)
		{
			stream?.Close();
			return new OperateResult<int>(ex.Message);
		}
	}

	private async Task<OperateResult<long>> ReceiveLongAsync(Socket socket)
	{
		var read = await ReceiveAsync(socket, 8, -1);
		if (read.IsSuccess)
		{
			return OperateResult.Ok(BitConverter.ToInt64(read.Content, 0));
		}
		return OperateResult.Error<long>(read);
	}

	private async Task<OperateResult> SendLongAsync(Socket socket, long value)
	{
		return await SendAsync(socket, BitConverter.GetBytes(value));
	}

	protected async Task<OperateResult> SendBaseAndCheckReceiveAsync(Socket socket, int headCode, int customer, byte[] send)
	{
		var send0 = OpsProtocol.CommandBytes(headCode, customer, Token, send);
		OperateResult sendResult = await SendAsync(socket, send0);
		if (!sendResult.IsSuccess)
		{
			return sendResult;
		}

		OperateResult<long> checkResult = await ReceiveLongAsync(socket);
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

	protected async Task<OperateResult> SendBytesAndCheckReceiveAsync(Socket socket, int customer, byte[] send)
	{
		return await SendBaseAndCheckReceiveAsync(socket, 1002, customer, send);
	}

	protected async Task<OperateResult> SendStringAndCheckReceiveAsync(Socket socket, int customer, string send)
	{
		byte[] data = string.IsNullOrEmpty(send) ? Array.Empty<byte>() : Encoding.Unicode.GetBytes(send);
		return await SendBaseAndCheckReceiveAsync(socket, 1001, customer, data);
	}

	protected async Task<OperateResult> SendStringAndCheckReceiveAsync(Socket socket, int customer, string[] sends)
	{
		return await SendBaseAndCheckReceiveAsync(socket, 1005, customer, OpsProtocol.PackStringArrayToByte(sends));
	}

	protected async Task<OperateResult> SendAccountAndCheckReceiveAsync(Socket socket, int customer, string name, string pwd)
	{
		return await SendBaseAndCheckReceiveAsync(socket, 5, customer, OpsProtocol.PackStringArrayToByte(new string[2] { name, pwd }));
	}

	protected async Task<OperateResult<byte[], byte[]>> ReceiveAndCheckBytesAsync(Socket socket, int timeout)
	{
		OperateResult<byte[]> headResult = await ReceiveAsync(socket, 32, timeout);
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
		OperateResult<byte[]> contentResult = await ReceiveAsync(socket, contentLength, timeout);
		if (!contentResult.IsSuccess)
		{
			return OperateResult.Error<byte[], byte[]>(contentResult);
		}

		OperateResult checkResult = await SendLongAsync(socket, 32 + contentLength);
		if (!checkResult.IsSuccess)
		{
			return OperateResult.Error<byte[], byte[]>(checkResult);
		}

		byte[] head = headResult.Content;
		byte[] content2 = contentResult.Content;
		content2 = OpsProtocol.CommandAnalysis(head, content2);
		return OperateResult.Ok(head, content2);
	}

	protected async Task<OperateResult<int, string>> ReceiveStringContentFromSocketAsync(Socket socket, int timeOut = 30000)
	{
		OperateResult<byte[], byte[]> receive = await ReceiveAndCheckBytesAsync(socket, timeOut);
		if (!receive.IsSuccess)
		{
			return OperateResult.Error<int, string>(receive);
		}

		if (BitConverter.ToInt32(receive.Content1, 0) != 1001)
		{
			socket?.Close();
			return new OperateResult<int, string>("CommandHeadCodeCheckFailed");
		}

		if (receive.Content2 == null)
		{
			receive.Content2 = new byte[0];
		}
		return OperateResult.Ok(BitConverter.ToInt32(receive.Content1, 4), Encoding.Unicode.GetString(receive.Content2));
	}

	protected async Task<OperateResult<int, string[]>> ReceiveStringArrayContentFromSocketAsync(Socket socket, int timeOut = 30000)
	{
		var receive = await ReceiveAndCheckBytesAsync(socket, timeOut);
		if (!receive.IsSuccess)
		{
			return OperateResult.Error<int, string[]>(receive);
		}

		if (BitConverter.ToInt32(receive.Content1, 0) != 1005)
		{
			socket?.Close();
			return new OperateResult<int, string[]>("CommandHeadCodeCheckFailed");
		}

		if (receive.Content2 == null)
		{
			receive.Content2 = new byte[4];
		}
		return OperateResult.Ok(BitConverter.ToInt32(receive.Content1, 4), OpsProtocol.UnPackStringArrayFromByte(receive.Content2));
	}

	protected async Task<OperateResult<int, byte[]>> ReceiveBytesContentFromSocketAsync(Socket socket, int timeout = 30000)
	{
		var receive = await ReceiveAndCheckBytesAsync(socket, timeout);
		if (!receive.IsSuccess)
		{
			return OperateResult.Error<int, byte[]>(receive);
		}

		if (BitConverter.ToInt32(receive.Content1, 0) != 1002)
		{
			socket?.Close();
			return new OperateResult<int, byte[]>("CommandHeadCodeCheckFailed");
		}

		return OperateResult.Ok(BitConverter.ToInt32(receive.Content1, 4), receive.Content2);
	}

	protected async Task<OperateResult> SendStreamToSocketAsync(Socket socket, Stream stream, long receive, Action<long, long> report, bool reportByPercent)
	{
		byte[] buffer = new byte[fileCacheSize];
		long SendTotal = 0L;
		long percent = 0L;
		stream.Position = 0L;
		while (SendTotal < receive)
		{
			OperateResult<int> read = await ReadStreamAsync(stream, buffer);
			if (!read.IsSuccess)
			{
				socket?.Close();
				return read;
			}

			SendTotal += read.Content;
			byte[] newBuffer = new byte[read.Content];
			Array.Copy(buffer, 0, newBuffer, 0, newBuffer.Length);
			OperateResult write = await SendBytesAndCheckReceiveAsync(socket, read.Content, newBuffer);
			if (!write.IsSuccess)
			{
				socket?.Close();
				return write;
			}

			if (reportByPercent)
			{
				long percentCurrent = SendTotal * 100 / receive;
				if (percent != percentCurrent)
				{
					percent = percentCurrent;
					report?.Invoke(SendTotal, receive);
				}
			}
			else
			{
				report?.Invoke(SendTotal, receive);
			}
		}
		return OperateResult.Ok();
	}

	protected async Task<OperateResult> WriteStreamFromSocketAsync(Socket socket, Stream stream, long totalLength, Action<long, long> report, bool reportByPercent)
	{
		long count_receive = 0L;
		long percent = 0L;
		while (count_receive < totalLength)
		{
			var read = await ReceiveBytesContentFromSocketAsync(socket, 60000);
			if (!read.IsSuccess)
			{
				return read;
			}

			count_receive += read.Content1;
			OperateResult write = await WriteStreamAsync(stream, read.Content2);
			if (!write.IsSuccess)
			{
				socket?.Close();
				return write;
			}

			if (reportByPercent)
			{
				long percentCurrent = count_receive * 100 / totalLength;
				if (percent != percentCurrent)
				{
					percent = percentCurrent;
					report?.Invoke(count_receive, totalLength);
				}
			}
			else
			{
				report?.Invoke(count_receive, totalLength);
			}
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 接收一条hsl协议的数据信息，自动解析，解压，解码操作，获取最后的实际的数据，接收结果依次为暗号，用户码，负载数据。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <returns>接收结果，依次为暗号，用户码，负载数据</returns>
	protected OperateResult<int, int, byte[]> ReceiveHslMessage(Socket socket)
	{
		var operateResult = Receive(socket, 32, 10000);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<int, int, byte[]>(operateResult);
		}

		int length = BitConverter.ToInt32(operateResult.Content, operateResult.Content.Length - 4);
		var operateResult2 = Receive(socket, length);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<int, int, byte[]>(operateResult2);
		}

		byte[] value = OpsProtocol.CommandAnalysis(operateResult.Content, operateResult2.Content);
		int value2 = BitConverter.ToInt32(operateResult.Content, 0);
		int value3 = BitConverter.ToInt32(operateResult.Content, 4);
		return OperateResult.Ok(value2, value3, value);
	}

	protected async Task<OperateResult<int, int, byte[]>> ReceiveHslMessageAsync(Socket socket)
	{
		var receiveHead = await ReceiveAsync(socket, 32, 10000);
		if (!receiveHead.IsSuccess)
		{
			return OperateResult.Error<int, int, byte[]>(receiveHead);
		}

		int receive_length = BitConverter.ToInt32(receiveHead.Content, receiveHead.Content.Length - 4);
		var receiveContent = await ReceiveAsync(socket, receive_length);
		if (!receiveContent.IsSuccess)
		{
			return OperateResult.Error<int, int, byte[]>(receiveContent);
		}

		byte[] Content = OpsProtocol.CommandAnalysis(receiveHead.Content, receiveContent.Content);
		int protocol = BitConverter.ToInt32(receiveHead.Content, 0);
		int customer = BitConverter.ToInt32(receiveHead.Content, 4);
		return OperateResult.Ok(protocol, customer, Content);
	}

	/// <summary>
	/// 删除文件的操作。
	/// </summary>
	/// <param name="filename">完整的真实的文件路径</param>
	/// <returns>是否删除成功</returns>
	protected bool DeleteFileByName(string filename)
	{
		try
		{
			if (!File.Exists(filename))
			{
				return true;
			}

			File.Delete(filename);
			return true;
		}
		catch (Exception ex)
		{
			Logger?.LogError(ex, $"delete file failed:{filename}");
			return false;
		}
	}

	/// <summary>
	/// 预处理文件夹的名称，除去文件夹名称最后一个'\'或'/'，如果有的话。
	/// </summary>
	/// <param name="folder">文件夹名称</param>
	/// <returns>返回处理之后的名称</returns>
	protected string PreprocessFolderName(string folder)
	{
		if (folder.EndsWith(Path.DirectorySeparatorChar))
		{
			return folder[0..^1];
		}
		return folder;
	}

	public override string ToString()
	{
		return "NetworkBase";
	}

	/// <summary>
	/// 通过主机名或是IP地址信息，获取到真实的IP地址信息。
	/// </summary>
	/// <param name="hostName">主机名或是IP地址</param>
	/// <returns>IP地址信息</returns>
	public static string GetIpAddressHostName(string hostName)
	{
		IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
		IPAddress iPAddress = hostEntry.AddressList[0];
		return iPAddress.ToString();
	}
}
