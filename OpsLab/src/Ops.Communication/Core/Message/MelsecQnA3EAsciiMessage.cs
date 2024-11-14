using System.Text;

namespace Ops.Communication.Core.Message;

/// <summary>
/// 基于MC协议的Qna兼容3E帧协议的ASCII通讯消息机制
/// </summary>
public class MelsecQnA3EAsciiMessage : INetMessage
{
	public int ProtocolHeadBytesLength => 18;

	public byte[] HeadBytes { get; set; }

	public byte[] ContentBytes { get; set; }

	public byte[] SendBytes { get; set; }

	public int GetContentLengthByHeadBytes()
	{
		byte[] bytes =
        [
            HeadBytes[14],
			HeadBytes[15],
			HeadBytes[16],
			HeadBytes[17]
		];
		return Convert.ToInt32(Encoding.ASCII.GetString(bytes), 16);
	}

	public bool CheckHeadBytesLegal(byte[] token)
	{
		if (HeadBytes == null)
		{
			return false;
		}

		if (HeadBytes[0] == 68 && HeadBytes[1] == 48 && HeadBytes[2] == 48 && HeadBytes[3] == 48)
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
