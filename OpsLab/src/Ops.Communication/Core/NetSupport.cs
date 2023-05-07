using System.Net;
using System.Net.Sockets;

namespace Ops.Communication.Core;

/// <summary>
/// 静态的方法支持类，提供一些网络的静态支持，支持从套接字从同步接收指定长度的字节数据，并支持报告进度。
/// </summary>
/// <remarks>
/// 在接收指定数量的字节数据的时候，如果一直接收不到，就会发生假死的状态。接收的数据时保存在内存里的，不适合大数据块的接收。
/// </remarks>
internal static class NetSupport
{
	/// <summary>
	/// Socket传输中的缓冲池大小
	/// </summary>
	internal const int SocketBufferSize = 16384;

    internal static int GetSplitLengthFromTotal(int length)
    {
        return length switch
        {
            < 1024 => length,
            <= 8192 => 2048,
            <= 32768 => 8192,
            <= 262144 => 32768,
            <= 1048576 => 262144,
            <= 8388608 => 1048576,
            _ => 2097152,
        };
    }

    /// <summary>
    /// 从socket的网络中读取数据内容，需要指定数据长度和超时的时间，为了防止数据太大导致接收失败，所以此处接收到新的数据之后就更新时间。
    /// </summary>
    /// <param name="socket">网络套接字</param>
    /// <param name="receive">接收的长度</param>
    /// <returns>最终接收的指定长度的byte[]数据</returns>
    /// <exception cref="RemoteCloseException"></exception>
    internal static byte[] ReadBytesFromSocket(Socket socket, int receive)
	{
		byte[] array = new byte[receive];
        ReceiveBytesFromSocket(socket, array, 0, receive);
        return array;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <exception cref="RemoteCloseException"></exception>
    internal static void ReceiveBytesFromSocket(Socket socket, byte[] buffer, int offset, int length)
    {
        int num = 0;
        while (num < length)
        {
            int size = Math.Min(length - num, SocketBufferSize);
            int num2 = socket.Receive(buffer, num + offset, size, SocketFlags.None);
            num += num2;
            if (num2 == 0)
            {
                throw new RemoteCloseException();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="stream"></param>
    /// <param name="length"></param>
    /// <exception cref="RemoteCloseException"></exception>
    internal static void ReceiveBytesFromSocket(Socket socket, Stream stream, int length)
    {
        int num = 0;
        byte[] array = new byte[GetSplitLengthFromTotal(length)];
        while (num < length)
        {
            int num2 = socket.Receive(array, 0, array.Length, SocketFlags.None);
            stream.Write(array, 0, num2);
            num += num2;
            if (num2 == 0)
            {
                throw new RemoteCloseException();
            }
        }
    }

    /// <summary>
    /// 创建 Socket，并连接远程主机。
    /// </summary>
    /// <param name="endPoint">终结点</param>
    /// <param name="timeOut">连接超时时长，单位ms</param>
    /// <returns></returns>
    /// <remarks>注：同步操作时，在设定的超时时长内会阻塞线程。</remarks>
    internal static OperateResult<Socket> CreateSocketAndConnect(IPEndPoint endPoint, int timeOut)
    {
        Socket socket = null;
        try
        {
            socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            bool isConnectionSuccessful = false;
            using ManualResetEvent mre = new(false);

            // 可以采用 IAsynResult.AsyncWaitHandle.WaitOne(1000) 进行阻塞等待
            socket.BeginConnect(endPoint, new AsyncCallback(state =>
            {
                isConnectionSuccessful = true;
                mre.Set();
                Socket s = (Socket)state.AsyncState;
                s.EndConnect(state);
            }), socket);

            if (mre.WaitOne(timeOut, true))
            {
                if (!isConnectionSuccessful)
                {
                    throw new OperationCanceledException();
                }
            }

            return OperateResult.Ok(socket);
        }
        catch (OperationCanceledException)
        {
            socket.Close();
            return new OperateResult<Socket>((int)ErrorCode.ConnectTimeout, $"ConnectTimeout, EndPoint:{endPoint}, Timeout: {timeOut}ms");
        }
        catch (Exception ex)
        {
            socket.Close();
            return new OperateResult<Socket>((int)ErrorCode.SocketException, $"Socket Exception -> {ex.Message}");
        }
    }

    /// <summary>
    /// 创建 Socket，并连接远程主机。
    /// </summary>
    /// <param name="endPoint">终结点</param>
    /// <param name="timeOut">连接超时时长，单位ms</param>
    /// <returns></returns>
    internal static async Task<OperateResult<Socket>> CreateSocketAndConnectAsync(IPEndPoint endPoint, int timeOut)
    {
        Socket socket = default!;

        try
        {
            socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // 设置超时
            using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(timeOut));
            await socket.ConnectAsync(endPoint, cts.Token).ConfigureAwait(false);

            return OperateResult.Ok(socket);
        }
        catch (OperationCanceledException)
        {
            socket.Close();
            return new OperateResult<Socket>((int)ErrorCode.ConnectTimeout, $"ConnectTimeout, EndPoint:{endPoint}, Timeout: {timeOut}ms");
        }
        catch (Exception ex)
        {
            socket.Close();
            return new OperateResult<Socket>((int)ErrorCode.SocketException, $"Socket Exception -> {ex.Message}");
        }
    }
}
