using System.Text;
using Ops.Communication.Basic;
using Ops.Communication.Core;
using Ops.Communication.Profinet.AllenBradley;

namespace Ops.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙PLC的CIP协议的类，支持NJ,NX,NY系列PLC，支持tag名的方式读写数据，假设你读取的是局部变量，那么使用 Program:MainProgram.变量名。
/// </summary>
public class OmronCipNet : AllenBradleyNet
{
	/// <summary>
	/// Instantiate a communication object for a OmronCipNet PLC protocol
	/// </summary>
	public OmronCipNet()
	{
	}

	/// <summary>
	/// Specify the IP address and port to instantiate a communication object for a OmronCipNet PLC protocol
	/// </summary>
	/// <param name="ipAddress">PLC IpAddress</param>
	/// <param name="port">PLC Port</param>
	public OmronCipNet(string ipAddress, int port = 44818)
		: base(ipAddress, port)
	{
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		if (length > 1)
		{
			return Read(new string[1] { address }, new int[1] { 1 });
		}
		return Read(new string[1] { address }, new int[1] { length });
	}

	public override OperateResult<short[]> ReadInt16(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransInt16(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransInt16(m, (startIndex >= 0) ? (startIndex * 2) : 0, length));
	}

	public override OperateResult<ushort[]> ReadUInt16(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransUInt16(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransUInt16(m, (startIndex >= 0) ? (startIndex * 2) : 0, length));
	}

	public override OperateResult<int[]> ReadInt32(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransInt32(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransInt32(m, (startIndex >= 0) ? (startIndex * 4) : 0, length));
	}

	public override OperateResult<uint[]> ReadUInt32(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransUInt32(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransUInt32(m, (startIndex >= 0) ? (startIndex * 4) : 0, length));
	}

	public override OperateResult<float[]> ReadFloat(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransSingle(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransSingle(m, (startIndex >= 0) ? (startIndex * 4) : 0, length));
	}

	public override OperateResult<long[]> ReadInt64(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransInt64(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransInt64(m, (startIndex >= 0) ? (startIndex * 8) : 0, length));
	}

	public override OperateResult<ulong[]> ReadUInt64(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransUInt64(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransUInt64(m, (startIndex >= 0) ? (startIndex * 8) : 0, length));
	}

	public override OperateResult<double[]> ReadDouble(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransDouble(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(Read(address, 1), m => ByteTransform.TransDouble(m, (startIndex >= 0) ? (startIndex * 8) : 0, length));
	}

	public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
	{
		OperateResult<byte[]> operateResult = Read(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult);
		}

		int count = base.ByteTransform.TransUInt16(operateResult.Content, 0);
		return OperateResult.Ok(encoding.GetString(operateResult.Content, 2, count));
	}

	public override OperateResult Write(string address, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			value = string.Empty;
		}

		byte[] array = SoftBasic.SpliceArray(new byte[2], SoftBasic.ArrayExpandToLengthEven(Encoding.ASCII.GetBytes(value)));
		array[0] = BitConverter.GetBytes(array.Length - 2)[0];
		array[1] = BitConverter.GetBytes(array.Length - 2)[1];
		return base.WriteTag(address, 208, array);
	}

	public override OperateResult Write(string address, byte value)
	{
		return WriteTag(address, 209, new byte[2] { value, 0 });
	}

	public override OperateResult WriteTag(string address, ushort typeCode, byte[] value, int length = 1)
	{
		return base.WriteTag(address, typeCode, value);
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		if (length > 1)
		{
			return await ReadAsync(new string[1] { address }, new int[1] { 1 });
		}
		return await ReadAsync(new string[1] { address }, new int[1] { length });
	}

	public override async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransInt16(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransInt16(m, (startIndex >= 0) ? (startIndex * 2) : 0, length));
	}

	public override async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransUInt16(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransUInt16(m, (startIndex >= 0) ? (startIndex * 2) : 0, length));
	}

	public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransInt32(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransInt32(m, (startIndex >= 0) ? (startIndex * 4) : 0, length));
	}

	public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransUInt32(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransUInt32(m, (startIndex >= 0) ? (startIndex * 4) : 0, length));
	}

	public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransSingle(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransSingle(m, (startIndex >= 0) ? (startIndex * 4) : 0, length));
	}

	public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransInt64(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransInt64(m, (startIndex >= 0) ? (startIndex * 8) : 0, length));
	}

	public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransUInt64(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransUInt64(m, (startIndex >= 0) ? (startIndex * 8) : 0, length));
	}

	public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
	{
		if (length == 1)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransDouble(m, 0, length));
		}

		int startIndex = OpsHelper.ExtractStartIndex(ref address);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1), m => ByteTransform.TransDouble(m, (startIndex >= 0) ? (startIndex * 8) : 0, length));
	}

	public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
	{
		OperateResult<byte[]> read = await ReadAsync(address, length);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<string>(read);
		}

		return OperateResult.Ok(encoding.GetString(count: ByteTransform.TransUInt16(read.Content, 0), bytes: read.Content, index: 2));
	}

	public override async Task<OperateResult> WriteAsync(string address, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			value = string.Empty;
		}

		byte[] data = SoftBasic.SpliceArray(new byte[2], SoftBasic.ArrayExpandToLengthEven(Encoding.ASCII.GetBytes(value)));
		data[0] = BitConverter.GetBytes(data.Length - 2)[0];
		data[1] = BitConverter.GetBytes(data.Length - 2)[1];
		return await WriteTagAsync(address, 208, data);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte value)
	{
		return await WriteTagAsync(address, 209, new byte[2] { value, 0 });
	}

	public override async Task<OperateResult> WriteTagAsync(string address, ushort typeCode, byte[] value, int length = 1)
	{
		return await base.WriteTagAsync(address, typeCode, value);
	}

	public override string ToString()
	{
		return $"OmronCipNet[{IpAddress}:{Port}]";
	}
}
