namespace Ops.Communication.Core.Message;

/// <summary>
/// 西门子S7协议的消息解析规则
/// </summary>
public class S7Message : INetMessage
{
	public int ProtocolHeadBytesLength => 4;

	public byte[] HeadBytes { get; set; }

	public byte[] ContentBytes { get; set; }

	public byte[] SendBytes { get; set; }

	public bool CheckHeadBytesLegal(byte[] token)
	{
		if (HeadBytes == null)
		{
			return false;
		}
		if (HeadBytes[0] == 3 && HeadBytes[1] == 0)
		{
			return true;
		}
		return false;
	}

	public int GetContentLengthByHeadBytes()
	{
		byte[] headBytes = HeadBytes;
		if (headBytes != null && headBytes.Length >= 4)
		{
			return HeadBytes[2] * 256 + HeadBytes[3] - 4;
		}
		return 0;
	}

	public int GetHeadBytesIdentity()
	{
		return 0;
	}
}
