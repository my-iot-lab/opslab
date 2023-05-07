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
    /// 连接服务器成功后的套接字。
    /// </summary>
    public Socket CoreSocket { get; protected set; }

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
	/// <returns>包含了字节数据的结果类</returns>
	/// <exception cref="RemoteCloseException"></exception>
	private OperateResult<byte[]> Receive(Socket socket, int length, int timeOut = 60000)
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
				byte[] value = NetSupport.ReadBytesFromSocket(socket, length);
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
			return new OperateResult<byte[]>((int)ErrorCode.RemoteClosedConnection, "RemoteClosedConnection");
		}
		catch (Exception ex2)
		{
			socket?.Close();
			return new OperateResult<byte[]>((int)ErrorCode.SocketException, $"Socket Exception -> {ex2.Message}");
		}
	}

	/// <summary>
	/// 接收一条完整的 <seealso cref="INetMessage" /> 数据内容，需要指定超时时间，单位为毫秒。
	/// </summary>
	/// <param name="socket">网络的套接字</param>
	/// <param name="timeOut">超时时间，单位：毫秒</param>
	/// <param name="netMessage">消息的格式定义</param>
	/// <returns>带有是否成功的byte数组对象</returns>
	protected OperateResult<byte[]> ReceiveByMessage(Socket socket, int timeOut, INetMessage netMessage)
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
		var operateResult2 = Receive(socket, contentLengthByHeadBytes, timeOut);
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
			return new OperateResult<byte[]>((int)ErrorCode.SocketSendException, ex.Message);
		}
	}

	/// <summary>
	/// 创建一个新的socket对象并连接到远程的地址，需要指定远程终结点，超时时间（单位是毫秒），如果需要绑定本地的IP或是端口，传入 local对象。
	/// </summary>
	/// <param name="endPoint">连接的目标终结点</param>
	/// <param name="timeOut">连接的超时时间</param>
	/// <returns>返回套接字的封装结果对象</returns>
	protected OperateResult<Socket> CreateSocketAndConnect(IPEndPoint endPoint, int timeOut)
	{
        OperateResult<Socket> operateResult = NetSupport.CreateSocketAndConnect(endPoint, timeOut);
        return operateResult;
    }

    /// <summary>
    /// 创建 Socket，并连接远程主机。
    /// </summary>
    /// <param name="endPoint">终结点</param>
    /// <param name="timeOut">连接超时时长，单位ms</param>
    /// <returns></returns>
    protected async Task<OperateResult<Socket>> CreateSocketAndConnectAsync(IPEndPoint endPoint, int timeOut)
	{
        OperateResult<Socket> connect = await NetSupport.CreateSocketAndConnectAsync(endPoint, timeOut);
		return connect;
    }
	
	/// <summary>
	/// 发送数据，出现异常后会主动关闭 Socket。
	/// </summary>
	/// <param name="socket"></param>
	/// <param name="data">要发送的数据。</param>
	/// <returns></returns>
	protected async Task<OperateResult> SendAsync(Socket socket, byte[] data)
	{
		if (data == null)
		{
			return OperateResult.Ok();
		}

		try
		{
            await socket.SendAsync(data, SocketFlags.None).ConfigureAwait(false);
			return OperateResult.Ok();
		}
		catch (Exception ex)
		{
			socket.Close();
			return new OperateResult<byte[]>((int)ErrorCode.SocketSendException, ex.Message);
		}
	}

	/// <summary>
	/// 接收数据并按指定格式解析。
	/// </summary>
	/// <remarks>出现异常会主动关闭 Socket。</remarks>
	/// <param name="socket">套接字</param>
	/// <param name="timeOut">接收超时时长</param>
	/// <param name="netMessage">消息解析格式。</param>
	/// <returns></returns>
	protected async Task<OperateResult<byte[]>> ReceiveByMessageAsync(Socket socket, int timeOut, INetMessage netMessage)
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
		OperateResult<byte[]> contentResult = await ReceiveAsync(socket, contentLength, timeOut).ConfigureAwait(false);
		if (!contentResult.IsSuccess)
		{
			return contentResult;
		}

		netMessage.ContentBytes = contentResult.Content;
		return OperateResult.Ok(SoftBasic.SpliceArray(headResult.Content, contentResult.Content));
	}

    private static async Task<OperateResult<byte[]>> ReceiveAsync(Socket socket, int length, int timeOut = 60000)
    {
        if (length == 0)
        {
            return OperateResult.Ok(Array.Empty<byte>());
        }

        try
        {
            // 设置超时
            using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(timeOut));

			byte[] buffer = new byte[length > 0 ? length : 2048];
            int count = await socket.ReceiveAsync(buffer, SocketFlags.None, cts.Token).ConfigureAwait(false); // 读取数据，直至填满缓冲区。

            // 0 => Socket 被动关闭; -1 => Socket 主动关闭
            if (count == 0)
            {
                throw new RemoteCloseException();
            }

            return OperateResult.Ok(length > 0 ? buffer : SoftBasic.ArraySelectBegin(buffer, count));
        }
        catch (RemoteCloseException)
        {
            socket.Close();
            return new OperateResult<byte[]>((int)ErrorCode.RemoteClosedConnection, "RemoteClosedConnection");
        }
        catch (OperationCanceledException)
        {
            socket.Close();
            return new OperateResult<byte[]>((int)ErrorCode.ReceiveDataTimeout, $"ReceiveDataTimeout: {timeOut}ms");
        }
		catch (SocketException ex)
		{
            // 已断开连接 ex.NativeErrorCode.Equals(10035), 10035 == WSAEWOULDBLOCK
            // 详细参考：https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.connected?view=net-7.0#system-net-sockets-socket-connected
			//if (ex.SocketErrorCode != SocketError.WouldBlock)
			//{
			//}
			
            socket.Close();
            return new OperateResult<byte[]>((int)ErrorCode.SocketException, $"Socket Exception -> {ex.Message}");
        }
        catch (Exception ex)
        {
            socket.Close();
            return new OperateResult<byte[]>((int)ErrorCode.SocketException, $"Socket Exception -> {ex.Message}");
        }
    }

    public override string ToString()
	{
		return "NetworkBase";
	}
}
