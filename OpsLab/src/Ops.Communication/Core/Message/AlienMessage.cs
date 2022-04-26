namespace Ops.Communication.Core.Message;

/// <summary>
/// 异形消息对象，用于异形客户端的注册包接收以及验证使用
/// </summary>
public class AlienMessage : INetMessage
{
	public int ProtocolHeadBytesLength => 5;

	public byte[] HeadBytes { get; set; }

	public byte[] ContentBytes { get; set; }

	public byte[] SendBytes { get; set; }

	public bool CheckHeadBytesLegal(byte[] token)
	{
		if (HeadBytes == null)
		{
			return false;
		}
		if (HeadBytes[0] == 72 && HeadBytes[1] == 115 && HeadBytes[2] == 110)
		{
			return true;
		}
		return false;
	}

	public int GetContentLengthByHeadBytes()
	{
		return HeadBytes[4];
	}

	public int GetHeadBytesIdentity()
	{
		return 0;
	}
}
