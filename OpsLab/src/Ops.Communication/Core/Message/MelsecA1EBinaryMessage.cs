namespace Ops.Communication.Core.Message;

/// <summary>
/// 三菱的A兼容1E帧协议解析规则
/// </summary>
public class MelsecA1EBinaryMessage : INetMessage
{
	public int ProtocolHeadBytesLength => 2;

	public byte[] HeadBytes { get; set; }

	public byte[] ContentBytes { get; set; }

	public byte[] SendBytes { get; set; }

	public int GetContentLengthByHeadBytes()
	{
		if (HeadBytes[1] == 91)
		{
			return 2;
		}

		if (HeadBytes[1] == 0)
		{
			return HeadBytes[0] switch
			{
				128 => (SendBytes[10] != 0) ? ((SendBytes[10] + 1) / 2) : 128,
				129 => SendBytes[10] * 2,
				130 or 131 => 0,
				_ => 0,
			};
		}
		return 0;
	}

	public bool CheckHeadBytesLegal(byte[] token)
	{
		if (HeadBytes != null)
		{
			return HeadBytes[0] - SendBytes[0] == 128;
		}
		return false;
	}

	public int GetHeadBytesIdentity()
	{
		return 0;
	}
}
