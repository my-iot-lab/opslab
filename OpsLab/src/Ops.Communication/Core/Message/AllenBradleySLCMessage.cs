namespace Ops.Communication.Core.Message;

/// <summary>
/// 用于和 AllenBradley PLC 交互的消息协议类
/// </summary>
public class AllenBradleySLCMessage : INetMessage
{
	public int ProtocolHeadBytesLength => 28;

	public byte[] HeadBytes { get; set; }

	public byte[] ContentBytes { get; set; }

	public byte[] SendBytes { get; set; }

	public int GetContentLengthByHeadBytes()
	{
		if (HeadBytes == null)
		{
			return 0;
		}
		return HeadBytes[2] * 256 + HeadBytes[3];
	}

	public bool CheckHeadBytesLegal(byte[] token)
	{
		return true;
	}

	public int GetHeadBytesIdentity()
	{
		return 0;
	}
}
