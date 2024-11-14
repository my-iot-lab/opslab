using System.Text;
using Ops.Communication.Core;
using Ops.Communication.Serial;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 三菱计算机链接协议，适用FX3U系列，FX3G，FX3S等等系列，通常在PLC侧连接的是485的接线口。
/// </summary>
/// <remarks>
/// 关于在PLC侧的配置信息，协议：专用协议  传送控制步骤：格式一  站号设置：0
/// </remarks>
public sealed class MelsecFxLinks : SerialDeviceBase
{
	private byte _watiingTime = 0;

	public byte Station { get; set; } = 0;

	public byte WaittingTime
	{
		get => _watiingTime;
		set
		{
			_watiingTime = _watiingTime > 15 ? (byte)15 : value;
        }
	}

	public bool SumCheck { get; set; } = true;

	public MelsecFxLinks()
	{
		base.ByteTransform = new RegularByteTransform();
		base.WordLength = 1;
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildReadCommand(b, address, length, isBool: false, SumCheck, _watiingTime);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		if (operateResult2.Content[0] != 2)
		{
			return new OperateResult<byte[]>((int)ConnErrorCode.MelsecReadFailed, "Read Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
		}

		byte[] array = new byte[length * 2];
		for (int i = 0; i < array.Length / 2; i++)
		{
			ushort value = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult2.Content, i * 4 + 5, 4), 16);
			BitConverter.GetBytes(value).CopyTo(array, i * 2);
		}
		return OperateResult.Ok(array);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildWriteByteCommand(b, address, value, SumCheck, _watiingTime);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		if (operateResult2.Content[0] != 6)
		{
			return new OperateResult((int)ConnErrorCode.MelsecWriteFailed, "Write Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
		}
		return OperateResult.Ok();
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildReadCommand(b, address, length, isBool: true, SumCheck, _watiingTime);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult2);
		}

		if (operateResult2.Content[0] != 2)
		{
			return new OperateResult<bool[]>((int)ConnErrorCode.MelsecReadFailed, "Read Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
		}

		byte[] array = new byte[length];
		Array.Copy(operateResult2.Content, 5, array, 0, length);
		return OperateResult.Ok(array.Select((byte m) => m == 49).ToArray());
	}

	public override OperateResult Write(string address, bool[] value)
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildWriteBoolCommand(b, address, value, SumCheck, _watiingTime);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		if (operateResult2.Content[0] != 6)
		{
			return new OperateResult((int)ConnErrorCode.MelsecWriteFailed, "Write Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
		}
		return OperateResult.Ok();
	}

	public OperateResult StartPLC(string parameter = "")
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref parameter, "s", Station);
		OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildStart(b, SumCheck, _watiingTime);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		if (operateResult2.Content[0] != 6)
		{
			return new OperateResult((int)ConnErrorCode.MelsecStartPLCFailed, "Start Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
		}
		return OperateResult.Ok();
	}

	public OperateResult StopPLC(string parameter = "")
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref parameter, "s", Station);
		OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildStop(b, SumCheck, _watiingTime);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		if (operateResult2.Content[0] != 6)
		{
			return new OperateResult((int)ConnErrorCode.MelsecStopPLCFailed, "Stop Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
		}
		return OperateResult.Ok();
	}

	public OperateResult<string> ReadPlcType(string parameter = "")
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref parameter, "s", Station);
		OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildReadPlcType(b, SumCheck, _watiingTime);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult);
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult2);
		}

		if (operateResult2.Content[0] != 6)
		{
			return new OperateResult<string>((int)ConnErrorCode.MelsecReadPlcTypeError, "ReadPlcType Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
		}
		return MelsecFxLinksOverTcp.GetPlcTypeFromCode(Encoding.ASCII.GetString(operateResult2.Content, 5, 2));
	}

	public override string ToString()
	{
		return $"MelsecFxLinks[{base.PortName}:{base.BaudRate}]";
	}
}
