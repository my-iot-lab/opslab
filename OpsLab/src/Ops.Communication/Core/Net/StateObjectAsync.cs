namespace Ops.Communication.Core.Net;

/// <summary>
/// 携带TaskCompletionSource属性的异步对象
/// </summary>
/// <typeparam name="T">类型</typeparam>
internal class StateObjectAsync<T> : StateObject
{
	public TaskCompletionSource<T> Tcs { get; set; }

	/// <summary>
	/// 实例化一个对象
	/// </summary>
	public StateObjectAsync()
	{
	}

	/// <summary>
	/// 实例化一个对象，指定接收或是发送的数据长度
	/// </summary>
	/// <param name="length">数据长度</param>
	public StateObjectAsync(int length)
		: base(length)
	{
	}
}
