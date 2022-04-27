using Ops.Communication.Basic;
using Ops.Communication.Core;
using Ops.Communication.Serial;

namespace Ops.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的HostLink协议的实现，地址支持示例 DM区:D100; CIO区:C100; Work区:W100; Holding区:H100; Auxiliary区: A100。
/// </summary>
/// <remarks>
/// 感谢 深圳～拾忆 的测试，地址可以携带站号信息，例如 s=2;D100 
/// <br />
/// <note type="important">
/// 如果发现串口线和usb同时打开才能通信的情况，需要按照如下的操作：<br />
/// 串口线不是标准的串口线，电脑的串口线的235引脚分别接PLC的329引脚，45线短接，就可以通讯，感谢 深圳-小君(QQ932507362)提供的解决方案。
/// </note>
/// </remarks>
public class OmronHostLink : SerialDeviceBase
{
	public byte ICF { get; set; } = 0;

	public byte DA2 { get; set; } = 0;

	public byte SA2 { get; set; }

	public byte SID { get; set; } = 0;

	public byte ResponseWaitTime { get; set; } = 48;

	public byte UnitNumber { get; set; }

	public int ReadSplits { get; set; } = 260;

	public OmronHostLink()
	{
		WordLength = 1;
		ByteTransform = new ReverseWordTransform
		{
			DataFormat = DataFormat.CDAB,
			IsStringReverseByteWord = true,
		};
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		OperateResult<List<byte[]>> operateResult = OmronFinsNetHelper.BuildReadCommand(address, length, isBit: false, ReadSplits);
		if (!operateResult.IsSuccess)
		{
			return operateResult.ConvertError<byte[]>();
		}

		var list = new List<byte>();
		for (int i = 0; i < operateResult.Content.Count; i++)
		{
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackCommand(station, operateResult.Content[i]));
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult2);
			}

			OperateResult<byte[]> operateResult3 = OmronHostLinkOverTcp.ResponseValidAnalysis(operateResult2.Content, isRead: true);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult3);
			}
			list.AddRange(operateResult3.Content);
		}
		return OperateResult.Ok(list.ToArray());
	}

	public override OperateResult Write(string address, byte[] value)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		OperateResult<byte[]> operateResult = OmronFinsNetHelper.BuildWriteWordCommand(address, value, isBit: false);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackCommand(station, operateResult.Content));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult<byte[]> operateResult3 = OmronHostLinkOverTcp.ResponseValidAnalysis(operateResult2.Content, isRead: false);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		OperateResult<List<byte[]>> operateResult = OmronFinsNetHelper.BuildReadCommand(address, length, isBit: true);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		var list = new List<bool>();
		for (int i = 0; i < operateResult.Content.Count; i++)
		{
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackCommand(station, operateResult.Content[i]));
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult2);
			}

			OperateResult<byte[]> operateResult3 = OmronHostLinkOverTcp.ResponseValidAnalysis(operateResult2.Content, isRead: true);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult3);
			}
			list.AddRange(operateResult3.Content.Select((byte m) => (m != 0) ? true : false));
		}
		return OperateResult.Ok(list.ToArray());
	}

	public override OperateResult Write(string address, bool[] values)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var operateResult = OmronFinsNetHelper.BuildWriteWordCommand(address, values.Select((bool m) => (byte)(m ? 1 : 0)).ToArray(), isBit: true);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackCommand(station, operateResult.Content));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult<byte[]> operateResult3 = OmronHostLinkOverTcp.ResponseValidAnalysis(operateResult2.Content, isRead: false);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	public override string ToString()
	{
		return $"OmronHostLink[{base.PortName}:{base.BaudRate}]";
	}

	/// <summary>
	/// 将普通的指令打包成完整的指令
	/// </summary>
	/// <param name="station">PLC的站号信息</param>
	/// <param name="cmd">fins指令</param>
	/// <returns>完整的质量</returns>
	private byte[] PackCommand(byte station, byte[] cmd)
	{
		cmd = SoftBasic.BytesToAsciiBytes(cmd);
		byte[] array = new byte[18 + cmd.Length];
		array[0] = 64;
		array[1] = SoftBasic.BuildAsciiBytesFrom(station)[0];
		array[2] = SoftBasic.BuildAsciiBytesFrom(station)[1];
		array[3] = 70;
		array[4] = 65;
		array[5] = ResponseWaitTime;
		array[6] = SoftBasic.BuildAsciiBytesFrom(ICF)[0];
		array[7] = SoftBasic.BuildAsciiBytesFrom(ICF)[1];
		array[8] = SoftBasic.BuildAsciiBytesFrom(DA2)[0];
		array[9] = SoftBasic.BuildAsciiBytesFrom(DA2)[1];
		array[10] = SoftBasic.BuildAsciiBytesFrom(SA2)[0];
		array[11] = SoftBasic.BuildAsciiBytesFrom(SA2)[1];
		array[12] = SoftBasic.BuildAsciiBytesFrom(SID)[0];
		array[13] = SoftBasic.BuildAsciiBytesFrom(SID)[1];
		array[^2] = 42;
		array[^1] = 13;
		cmd.CopyTo(array, 14);
		int num = array[0];
		for (int i = 1; i < array.Length - 4; i++)
		{
			num ^= array[i];
		}
		array[^4] = SoftBasic.BuildAsciiBytesFrom((byte)num)[0];
		array[^3] = SoftBasic.BuildAsciiBytesFrom((byte)num)[1];
		return array;
	}
}
