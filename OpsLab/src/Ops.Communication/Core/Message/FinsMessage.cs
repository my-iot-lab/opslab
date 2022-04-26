namespace Ops.Communication.Core.Message;

/// <summary>
/// 用于欧姆龙通信的Fins协议的消息解析规则
/// </summary>
public class FinsMessage : INetMessage
{
	public int ProtocolHeadBytesLength => 8;

	public byte[] HeadBytes { get; set; }

	public byte[] ContentBytes { get; set; }

	public byte[] SendBytes { get; set; }

	public int GetContentLengthByHeadBytes()
	{
		return BitConverter.ToInt32(new byte[4]
		{
			HeadBytes[7],
			HeadBytes[6],
			HeadBytes[5],
			HeadBytes[4]
		}, 0);
	}

	public bool CheckHeadBytesLegal(byte[] token)
	{
		if (HeadBytes == null)
		{
			return false;
		}

		if (HeadBytes[0] == 70 && HeadBytes[1] == 73 && HeadBytes[2] == 78 && HeadBytes[3] == 83)
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
