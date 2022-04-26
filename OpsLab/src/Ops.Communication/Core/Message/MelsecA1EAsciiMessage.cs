using System.Text;

namespace Ops.Communication.Core.Message;

/// <summary>
/// 三菱的A兼容1E帧ASCII协议解析规则
/// </summary>
public class MelsecA1EAsciiMessage : INetMessage
{
	public int ProtocolHeadBytesLength => 4;

	public byte[] HeadBytes { get; set; }

	public byte[] ContentBytes { get; set; }

	public byte[] SendBytes { get; set; }

	public int GetContentLengthByHeadBytes()
	{
		if (HeadBytes[2] == 53 && HeadBytes[3] == 66)
		{
			return 4;
		}

		if (HeadBytes[2] == 48 && HeadBytes[3] == 48)
		{
			int num = Convert.ToInt32(Encoding.ASCII.GetString(SendBytes, 20, 2), 16);
			if (num == 0)
			{
				num = 256;
			}

                return HeadBytes[1] switch
                {
                    48 => (num % 2 == 1) ? (num + 1) : num,
                    49 => num * 4,
                    50 or 51 => 0,
                    _ => 0,
                };
            }
		return 0;
	}

	public bool CheckHeadBytesLegal(byte[] token)
	{
		if (HeadBytes != null)
		{
			return HeadBytes[0] - SendBytes[0] == 8;
		}
		return false;
	}

	public int GetHeadBytesIdentity()
	{
		return 0;
	}
}
