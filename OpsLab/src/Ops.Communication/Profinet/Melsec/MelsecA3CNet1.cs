using Ops.Communication.Core;
using Ops.Communication.Serial;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 基于Qna 兼容3C帧的格式一的通讯，具体的地址需要参照三菱的基本地址。
/// </summary>
public class MelsecA3CNet1 : SerialDeviceBase
{
	public byte Station { get; set; } = 0;

	public MelsecA3CNet1()
	{
		base.ByteTransform = new RegularByteTransform();
		base.WordLength = 1;
	}

	private OperateResult<byte[]> ReadWithPackCommand(byte[] command, byte station)
	{
		return ReadFromCoreServer(MelsecA3CNet1OverTcp.PackCommand(command, station));
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		return MelsecA3CNet1OverTcp.ReadHelper(address, length, b, ReadWithPackCommand);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		return MelsecA3CNet1OverTcp.WriteHelper(address, value, b, ReadWithPackCommand);
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		return MelsecA3CNet1OverTcp.ReadBoolHelper(address, length, b, ReadWithPackCommand);
	}

	public override OperateResult Write(string address, bool[] value)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		return MelsecA3CNet1OverTcp.WriteHelper(address, value, b, ReadWithPackCommand);
	}

	public OperateResult RemoteRun()
	{
		return MelsecA3CNet1OverTcp.RemoteRunHelper(Station, ReadWithPackCommand);
	}

	public OperateResult RemoteStop()
	{
		return MelsecA3CNet1OverTcp.RemoteStopHelper(Station, ReadWithPackCommand);
	}

	public OperateResult<string> ReadPlcType()
	{
		return MelsecA3CNet1OverTcp.ReadPlcTypeHelper(Station, ReadWithPackCommand);
	}

	public override string ToString()
	{
		return $"MelsecA3CNet1[{base.PortName}:{base.BaudRate}]";
	}
}
