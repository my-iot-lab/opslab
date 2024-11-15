using Ops.Communication.Core;
using Ops.Communication.Core.Net;
using Ops.Communication.Utils;

namespace Ops.Communication.Modbus;

/// <summary>
/// Modbus-Udp协议的客户端通讯类，方便的和服务器进行数据交互，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式，详细见备注说明
/// </summary>
/// <remarks>
/// 本客户端支持的标准的modbus协议，Modbus-Tcp及Modbus-Udp内置的消息号会进行自增，地址支持富文本格式，具体参考示例代码。<br />
/// 读取线圈，输入线圈，寄存器，输入寄存器的方法中的读取长度对商业授权用户不限制，内部自动切割读取，结果合并。
/// </remarks>
public class ModbusUdpNet : NetworkUdpDeviceBase, IModbus, IReadWriteDevice, IReadWriteNet
{
	private readonly IncrementCount softIncrementCount;

	public bool AddressStartWithZero { get; set; } = true;

	public byte Station { get; set; } = 1;

	public DataFormat DataFormat
	{
		get
		{
			return base.ByteTransform.DataFormat;
		}
		set
		{
			base.ByteTransform.DataFormat = value;
		}
	}

	public bool IsStringReverse
	{
		get
		{
			return base.ByteTransform.IsStringReverseByteWord;
		}
		set
		{
			base.ByteTransform.IsStringReverseByteWord = value;
		}
	}

	public IncrementCount MessageId => softIncrementCount;

	/// <summary>
	/// 实例化一个MOdbus-Udp协议的客户端对象。
	/// </summary>
	public ModbusUdpNet()
	{
		base.ByteTransform = new ReverseWordTransform();
		softIncrementCount = new IncrementCount(65535L, 0L);
		base.WordLength = 1;
	}

	public ModbusUdpNet(string ipAddress, int port = 502, byte station = 1)
		: this()
	{
		IpAddress = ipAddress;
		Port = port;
		this.Station = station;
	}

	public virtual OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
	{
		return OperateResult.Ok(address);
	}

	protected override byte[] PackCommandWithHeader(byte[] command)
	{
		return ModbusInfo.PackCommandToTcp(command, (ushort)softIncrementCount.GetCurrentValue());
	}

	protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
	{
		return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(response));
	}

	public OperateResult<bool> ReadCoil(string address)
	{
		return ReadBool(address);
	}

	public OperateResult<bool[]> ReadCoil(string address, ushort length)
	{
		return ReadBool(address, length);
	}

	public OperateResult<bool> ReadDiscrete(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadDiscrete(address, 1));
	}

	public OperateResult<bool[]> ReadDiscrete(string address, ushort length)
	{
		return ModbusHelper.ReadBoolHelper(this, address, length, 2);
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		return ModbusHelper.Read(this, address, length);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		return ModbusHelper.Write(this, address, value);
	}

	public override OperateResult Write(string address, short value)
	{
		return ModbusHelper.Write(this, address, value);
	}

	public override OperateResult Write(string address, ushort value)
	{
		return ModbusHelper.Write(this, address, value);
	}

	public OperateResult WriteMask(string address, ushort andMask, ushort orMask)
	{
		return ModbusHelper.WriteMask(this, address, andMask, orMask);
	}

	public OperateResult WriteOneRegister(string address, short value)
	{
		return Write(address, value);
	}

	public OperateResult WriteOneRegister(string address, ushort value)
	{
		return Write(address, value);
	}

	public async Task<OperateResult<bool>> ReadCoilAsync(string address)
	{
		return await Task.Run(() => ReadCoil(address)).ConfigureAwait(false);
	}

	public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length)
	{
		return await Task.Run(() => ReadCoil(address, length)).ConfigureAwait(false);
	}

	public async Task<OperateResult<bool>> ReadDiscreteAsync(string address)
	{
		return await Task.Run(() => ReadDiscrete(address)).ConfigureAwait(false);
	}

	public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length)
	{
		return await Task.Run(() => ReadDiscrete(address, length)).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, short value)
	{
		return await Task.Run(() => Write(address, value)).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, ushort value)
	{
		return await Task.Run(() => Write(address, value)).ConfigureAwait(false);
	}

	public async Task<OperateResult> WriteOneRegisterAsync(string address, short value)
	{
		return await Task.Run(() => WriteOneRegister(address, value)).ConfigureAwait(false);
	}

	public async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value)
	{
		return await Task.Run(() => WriteOneRegister(address, value)).ConfigureAwait(false);
	}

	public async Task<OperateResult> WriteMaskAsync(string address, ushort andMask, ushort orMask)
	{
		return await Task.Run(() => WriteMask(address, andMask, orMask)).ConfigureAwait(false);
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		return ModbusHelper.ReadBoolHelper(this, address, length, 1);
	}

	public override OperateResult Write(string address, bool[] values)
	{
		return ModbusHelper.Write(this, address, values);
	}

	public override OperateResult Write(string address, bool value)
	{
		return ModbusHelper.Write(this, address, value);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool value)
	{
		return await Task.Run(() => Write(address, value)).ConfigureAwait(false);
	}

	public override OperateResult<int[]> ReadInt32(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransInt32(m, 0, length));
	}

	public override OperateResult<uint[]> ReadUInt32(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransUInt32(m, 0, length));
	}

	public override OperateResult<float[]> ReadFloat(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransSingle(m, 0, length));
	}

	public override OperateResult<long[]> ReadInt64(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransInt64(m, 0, length));
	}

	public override OperateResult<ulong[]> ReadUInt64(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransUInt64(m, 0, length));
	}

	public override OperateResult<double[]> ReadDouble(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransDouble(m, 0, length));
	}

	public override OperateResult Write(string address, int[] values)
	{
		IByteTransform byteTransform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override OperateResult Write(string address, uint[] values)
	{
		IByteTransform byteTransform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override OperateResult Write(string address, float[] values)
	{
		IByteTransform byteTransform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override OperateResult Write(string address, long[] values)
	{
		IByteTransform byteTransform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override OperateResult Write(string address, ulong[] values)
	{
		IByteTransform byteTransform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override OperateResult Write(string address, double[] values)
	{
		IByteTransform byteTransform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 2)).ConfigureAwait(false), (byte[] m) => transform.TransInt32(m, 0, length));
	}

	public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 2)).ConfigureAwait(false), (byte[] m) => transform.TransUInt32(m, 0, length));
	}

	public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 2)).ConfigureAwait(false), (byte[] m) => transform.TransSingle(m, 0, length));
	}

	public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 4)).ConfigureAwait(false), (byte[] m) => transform.TransInt64(m, 0, length));
	}

	public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 4)).ConfigureAwait(false), (byte[] m) => transform.TransUInt64(m, 0, length));
	}

	public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
	{
		IByteTransform transform = ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 4)).ConfigureAwait(false), (byte[] m) => transform.TransDouble(m, 0, length));
	}

	public override async Task<OperateResult> WriteAsync(string address, int[] values)
	{
		return await WriteAsync(value: ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, uint[] values)
	{
		return await WriteAsync(value: ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, float[] values)
	{
		return await WriteAsync(value: ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, long[] values)
	{
		return await WriteAsync(value: ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, ulong[] values)
	{
		return await WriteAsync(value: ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, double[] values)
	{
		return await WriteAsync(value: ConnHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override string ToString()
	{
		return $"ModbusUdpNet[{IpAddress}:{Port}]";
	}
}
