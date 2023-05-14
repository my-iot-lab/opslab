namespace Ops.Communication.Core;

/// <summary>
/// 用于读写的设备接口，相较于 <see cref="IReadWriteNet" />，增加了 <see cref="ReadFromCoreServer(byte[])" />相关的方法，可以用来和设备进行额外的交互。
/// </summary>
public interface IReadWriteDevice : IReadWriteNet
{
	/// <summary>
	/// 将当前的数据报文发送到设备去，具体使用什么通信方式取决于设备信息，然后从设备接收数据回来，并返回给调用者。
	/// </summary>
	/// <param name="send">发送的完整的报文信息</param>
	/// <returns>接收的完整的报文信息</returns>
	OperateResult<byte[]> ReadFromCoreServer(byte[] send);

    /// <summary>
    /// 将当前的数据报文发送到设备去，具体使用什么通信方式取决于设备信息，然后从设备接收数据回来，并返回给调用者。
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <returns>接收的完整的报文信息</returns>
    OperateResult<byte[]> ReadFromCoreServer(IEnumerable<byte[]> send);

    /// <summary>
    /// 将当前的数据报文发送到设备去，具体使用什么通信方式取决于设备信息，然后从设备接收数据回来，并返回给调用者。
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <returns>接收的完整的报文信息</returns>
    Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send);

    /// <summary>
    /// 将当前的数据报文发送到设备去，具体使用什么通信方式取决于设备信息，然后从设备接收数据回来，并返回给调用者。
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <returns>接收的完整的报文信息</returns>
    Task<OperateResult<byte[]>> ReadFromCoreServerAsync(IEnumerable<byte[]> send);
}
