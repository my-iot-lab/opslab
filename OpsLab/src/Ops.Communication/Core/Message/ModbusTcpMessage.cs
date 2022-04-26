namespace Ops.Communication.Core.Message;

/// <summary>
/// Modbus-Tcp协议支持的消息解析类
/// </summary>
public class ModbusTcpMessage : INetMessage
{
	public int ProtocolHeadBytesLength => 8;

	public byte[] HeadBytes { get; set; }

	public byte[] ContentBytes { get; set; }

	public byte[] SendBytes { get; set; }

	/// <summary>
	/// 获取或设置是否进行检查返回的消息ID和发送的消息ID是否一致，默认为true，也就是检查
	/// </summary>
	public bool IsCheckMessageId { get; set; } = true;

	public int GetContentLengthByHeadBytes()
	{
		if (HeadBytes?.Length >= ProtocolHeadBytesLength)
		{
			int num = HeadBytes[4] * 256 + HeadBytes[5];
			if (num == 0)
			{
				byte[] array = new byte[ProtocolHeadBytesLength - 1];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = HeadBytes[i + 1];
				}
				HeadBytes = array;
				return HeadBytes[5] * 256 + HeadBytes[6] - 1;
			}
			return num - 2;
		}
		return 0;
	}

	public bool CheckHeadBytesLegal(byte[] token)
	{
		if (IsCheckMessageId)
		{
			if (HeadBytes == null)
			{
				return false;
			}
			if (SendBytes[0] != HeadBytes[0] || SendBytes[1] != HeadBytes[1])
			{
				return false;
			}
			return HeadBytes[2] == 0 && HeadBytes[3] == 0;
		}
		return true;
	}

	public int GetHeadBytesIdentity()
	{
		return HeadBytes[0] * 256 + HeadBytes[1];
	}
}
