using Ops.Communication.Core;
using Ops.Communication.Core.Net;

namespace Ops.Communication.Modbus;

public class ModbusRtuOverTcp : NetworkDeviceBase, IModbus, IReadWriteDevice, IReadWriteNet
{
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

	public ModbusRtuOverTcp()
	{
		base.ByteTransform = new ReverseWordTransform();
		base.WordLength = 1;
		base.SleepTime = 20;
	}

	public ModbusRtuOverTcp(string ipAddress, int port = 502, byte station = 1)
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
		return ModbusInfo.PackCommandToRtu(command);
	}

	protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
	{
		return ModbusHelper.ExtraRtuResponseContent(send, response);
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
		return await ReadBoolAsync(address).ConfigureAwait(false);
	}

	public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length)
	{
		return await ReadBoolAsync(address, length).ConfigureAwait(false);
	}

	public async Task<OperateResult<bool>> ReadDiscreteAsync(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadDiscreteAsync(address, 1).ConfigureAwait(false));
	}

	public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length)
	{
		return await ReadBoolHelperAsync(address, length, 2).ConfigureAwait(false);
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		return await ModbusHelper.ReadAsync(this, address, length).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, short value)
	{
		return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, ushort value)
	{
		return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
	}

	public async Task<OperateResult> WriteOneRegisterAsync(string address, short value)
	{
		return await WriteAsync(address, value).ConfigureAwait(false);
	}

	public async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value)
	{
		return await WriteAsync(address, value).ConfigureAwait(false);
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

	private async Task<OperateResult<bool[]>> ReadBoolHelperAsync(string address, ushort length, byte function)
	{
		return await ModbusHelper.ReadBoolHelperAsync(this, address, length, function).ConfigureAwait(false);
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		return await ReadBoolHelperAsync(address, length, 1).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool[] values)
	{
		return await ModbusHelper.WriteAsync(this, address, values).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool value)
	{
		return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
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
		return $"ModbusRtuOverTcp[{IpAddress}:{Port}]";
	}
}
