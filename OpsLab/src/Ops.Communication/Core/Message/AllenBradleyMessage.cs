namespace Ops.Communication.Core.Message;

/// <summary>
/// 用于和 AllenBradley PLC 交互的消息协议类
/// </summary>
public class AllenBradleyMessage : INetMessage
{
	public int ProtocolHeadBytesLength => 24;

	public byte[] HeadBytes { get; set; }

	public byte[] ContentBytes { get; set; }

	public byte[] SendBytes { get; set; }

	public int GetContentLengthByHeadBytes()
	{
		return BitConverter.ToUInt16(HeadBytes, 2);
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
