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

	/// <summary>
	/// 从socket的网络中读取数据内容，需要指定数据长度和超时的时间，为了防止数据太大导致接收失败，所以此处接收到新的数据之后就更新时间。
	/// </summary>
	/// <param name="socket">网络套接字</param>
	/// <param name="receive">接收的长度</param>
	/// <param name="reportProgress">当前接收数据的进度报告，有些协议支持传输非常大的数据内容，可以给与进度提示的功能</param>
	/// <returns>最终接收的指定长度的byte[]数据</returns>
	internal static byte[] ReadBytesFromSocket(Socket socket, int receive, Action<long, long>? reportProgress = null)
	{
		byte[] array = new byte[receive];
		int num = 0;
		while (num < receive)
		{
			int size = Math.Min(receive - num, SocketBufferSize);
			int num2 = socket.Receive(array, num, size, SocketFlags.None);
			if (num2 == 0)
			{
				throw new RemoteCloseException();
			}

			num += num2;
			reportProgress?.Invoke(num, receive);
		}
		return array;
	}
}
