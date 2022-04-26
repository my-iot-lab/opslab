using System.Net.Sockets;

namespace Ops.Communication.Core.Net;

/// <summary>
/// 网络中的异步对象
/// </summary>
internal class StateObject : StateOneBase
{
	/// <summary>
	/// 唯一的一串信息
	/// </summary>
	public string UniqueId { get; set; }

	/// <summary>
	/// 网络套接字
	/// </summary>
	public Socket WorkSocket { get; set; }

	/// <summary>
	/// 是否关闭了通道
	/// </summary>
	public bool IsClose { get; set; }

	/// <summary>
	/// 实例化一个对象
	/// </summary>
	public StateObject()
	{
	}

	/// <summary>
	/// 实例化一个对象，指定接收或是发送的数据长度
	/// </summary>
	/// <param name="length">数据长度</param>
	public StateObject(int length)
	{
		base.DataLength = length;
		base.Buffer = new byte[length];
	}

	/// <summary>
	/// 清空旧的数据
	/// </summary>
	public void Clear()
	{
		base.IsError = false;
		IsClose = false;
		base.AlreadyDealLength = 0;
		base.Buffer = null;
	}
}
