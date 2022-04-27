using Ops.Communication.Core;
using Ops.Communication.Serial;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 三菱的串口通信的对象，适用于读取FX系列的串口数据，支持的类型参考文档说明。
/// </summary>
public class MelsecFxSerial : SerialDeviceBase
{
	public bool IsNewVersion { get; set; }

	/// <summary>
	/// 实例化一个默认的对象
	/// </summary>
	public MelsecFxSerial()
	{
		base.ByteTransform = new RegularByteTransform();
		base.WordLength = 1;
		IsNewVersion = true;
		base.ByteTransform.IsStringReverseByteWord = true;
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		return MelsecFxSerialOverTcp.ReadHelper(address, length, ReadFromCoreServer, IsNewVersion);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		return MelsecFxSerialOverTcp.WriteHelper(address, value, ReadFromCoreServer, IsNewVersion);
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		return MelsecFxSerialOverTcp.ReadBoolHelper(address, length, ReadFromCoreServer);
	}

	public override OperateResult Write(string address, bool value)
	{
		return MelsecFxSerialOverTcp.WriteHelper(address, value, ReadFromCoreServer);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool value)
	{
		return await Task.Run(() => Write(address, value));
	}

	public override string ToString()
	{
		return $"MelsecFxSerial[{base.PortName}:{base.BaudRate}]";
	}
}
