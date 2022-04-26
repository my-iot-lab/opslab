namespace Ops.Communication.Core.Message;

/// <summary>
/// 三菱的Qna兼容3E帧协议解析规则
/// </summary>
public class MelsecQnA3EBinaryMessage : INetMessage
{
	public int ProtocolHeadBytesLength => 9;

	public byte[] HeadBytes { get; set; }

	public byte[] ContentBytes { get; set; }

	public byte[] SendBytes { get; set; }

	public int GetContentLengthByHeadBytes()
	{
		return BitConverter.ToUInt16(HeadBytes, 7);
	}

	public bool CheckHeadBytesLegal(byte[] token)
	{
		if (HeadBytes == null)
		{
			return false;
		}

		if (HeadBytes[0] == 208 && HeadBytes[1] == 0)
		{
			return true;
		}
		return false;
	}

	public int GetHeadBytesIdentity()
	{
		return 0;
	}
}
