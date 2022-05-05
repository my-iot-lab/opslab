using System.Text;
using Ops.Communication.Reflection;
using Ops.Communication.Utils;

namespace Ops.Communication.Core.Net;

/// <summary>
/// Udp报文协议的内容传送
/// </summary>
public class NetworkUdpDeviceBase : NetworkUdpBase, IReadWriteDevice, IReadWriteNet
{
	public IByteTransform ByteTransform { get; set; }

	protected ushort WordLength { get; set; } = 1;

	/// <summary>
	/// 实例化一个默认的对象
	/// </summary>
	public NetworkUdpDeviceBase()
	{
	}

	public override string ToString()
	{
		return $"NetworkUdpDeviceBase<{ByteTransform.GetType()}>[{IpAddress}:{Port}]";
	}

	public virtual OperateResult<byte[]> Read(string address, ushort length)
	{
		return new OperateResult<byte[]>(ErrorCode.NotSupportedFunction.Desc());
	}

	public virtual OperateResult Write(string address, byte[] value)
	{
		return new OperateResult(ErrorCode.NotSupportedFunction.Desc());
	}

	public virtual OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		return new OperateResult<bool[]>(ErrorCode.NotSupportedFunction.Desc());
	}

	public virtual OperateResult<bool> ReadBool(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadBool(address, 1));
	}

	public virtual OperateResult Write(string address, bool[] value)
	{
		return new OperateResult(ErrorCode.NotSupportedFunction.Desc());
	}

	public virtual OperateResult Write(string address, bool value)
	{
		return Write(address, new bool[1] { value });
	}

	public OperateResult<T> ReadCustomer<T>(string address) where T : IDataTransfer, new()
	{
		var operateResult = new OperateResult<T>();
		T content = new();
		OperateResult<byte[]> operateResult2 = Read(address, content.ReadCount);
		if (operateResult2.IsSuccess)
		{
			content.ParseSource(operateResult2.Content);
			operateResult.Content = content;
			operateResult.IsSuccess = true;
		}
		else
		{
			operateResult.ErrorCode = operateResult2.ErrorCode;
			operateResult.Message = operateResult2.Message;
		}
		return operateResult;
	}

	public OperateResult WriteCustomer<T>(string address, T data) where T : IDataTransfer, new()
	{
		return Write(address, data.ToSource());
	}

	public virtual OperateResult<T> Read<T>() where T : class, new()
	{
		return ReflectionHelper.Read<T>(this);
	}

	public virtual OperateResult Write<T>(T data) where T : class, new()
	{
		return ReflectionHelper.Write(data, this);
	}

	public OperateResult<short> ReadInt16(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadInt16(address, 1));
	}

	public virtual OperateResult<short[]> ReadInt16(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength)), (byte[] m) => ByteTransform.TransInt16(m, 0, length));
	}

	public OperateResult<ushort> ReadUInt16(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadUInt16(address, 1));
	}

	public virtual OperateResult<ushort[]> ReadUInt16(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength)), (byte[] m) => ByteTransform.TransUInt16(m, 0, length));
	}

	public OperateResult<int> ReadInt32(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadInt32(address, 1));
	}

	public virtual OperateResult<int[]> ReadInt32(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransInt32(m, 0, length));
	}

	public OperateResult<uint> ReadUInt32(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadUInt32(address, 1));
	}

	public virtual OperateResult<uint[]> ReadUInt32(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransUInt32(m, 0, length));
	}

	public OperateResult<float> ReadFloat(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadFloat(address, 1));
	}

	public virtual OperateResult<float[]> ReadFloat(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransSingle(m, 0, length));
	}

	public OperateResult<long> ReadInt64(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadInt64(address, 1));
	}

	public virtual OperateResult<long[]> ReadInt64(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransInt64(m, 0, length));
	}

	public OperateResult<ulong> ReadUInt64(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadUInt64(address, 1));
	}

	public virtual OperateResult<ulong[]> ReadUInt64(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransUInt64(m, 0, length));
	}

	public OperateResult<double> ReadDouble(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadDouble(address, 1));
	}

	public virtual OperateResult<double[]> ReadDouble(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransDouble(m, 0, length));
	}

	public virtual OperateResult<string> ReadString(string address, ushort length)
	{
		return ReadString(address, length, Encoding.ASCII);
	}

	public virtual OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), (byte[] m) => ByteTransform.TransString(m, 0, m.Length, encoding));
	}

	public virtual OperateResult Write(string address, short[] values)
	{
		return Write(address, ByteTransform.TransByte(values));
	}

	public virtual OperateResult Write(string address, short value)
	{
		return Write(address, new short[1] { value });
	}

	public virtual OperateResult Write(string address, ushort[] values)
	{
		return Write(address, ByteTransform.TransByte(values));
	}

	public virtual OperateResult Write(string address, ushort value)
	{
		return Write(address, new ushort[1] { value });
	}

	public virtual OperateResult Write(string address, int[] values)
	{
		return Write(address, ByteTransform.TransByte(values));
	}

	public OperateResult Write(string address, int value)
	{
		return Write(address, new int[1] { value });
	}

	public virtual OperateResult Write(string address, uint[] values)
	{
		return Write(address, ByteTransform.TransByte(values));
	}

	public OperateResult Write(string address, uint value)
	{
		return Write(address, new uint[1] { value });
	}

	public virtual OperateResult Write(string address, float[] values)
	{
		return Write(address, ByteTransform.TransByte(values));
	}

	public OperateResult Write(string address, float value)
	{
		return Write(address, new float[1] { value });
	}

	public virtual OperateResult Write(string address, long[] values)
	{
		return Write(address, ByteTransform.TransByte(values));
	}

	public OperateResult Write(string address, long value)
	{
		return Write(address, new long[1] { value });
	}

	public virtual OperateResult Write(string address, ulong[] values)
	{
		return Write(address, ByteTransform.TransByte(values));
	}

	public OperateResult Write(string address, ulong value)
	{
		return Write(address, new ulong[1] { value });
	}

	public virtual OperateResult Write(string address, double[] values)
	{
		return Write(address, ByteTransform.TransByte(values));
	}

	public OperateResult Write(string address, double value)
	{
		return Write(address, new double[1] { value });
	}

	public virtual OperateResult Write(string address, string value)
	{
		return Write(address, value, Encoding.ASCII);
	}

	public virtual OperateResult Write(string address, string value, int length)
	{
		return Write(address, value, length, Encoding.ASCII);
	}

	public virtual OperateResult Write(string address, string value, Encoding encoding)
	{
		byte[] array = ByteTransform.TransByte(value, encoding);
		if (WordLength == 1)
		{
			array = SoftBasic.ArrayExpandToLengthEven(array);
		}
		return Write(address, array);
	}

	public virtual OperateResult Write(string address, string value, int length, Encoding encoding)
	{
		byte[] data = ByteTransform.TransByte(value, encoding);
		if (WordLength == 1)
		{
			data = SoftBasic.ArrayExpandToLengthEven(data);
		}
		data = SoftBasic.ArrayExpandToLength(data, length);
		return Write(address, data);
	}

	public virtual async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		return await Task.Run(() => Read(address, length));
	}

	public virtual async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		return await Task.Run(() => Write(address, value));
	}

	public virtual async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		return await Task.Run(() => ReadBool(address, length));
	}

	public virtual async Task<OperateResult<bool>> ReadBoolAsync(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadBoolAsync(address, 1));
	}

	public virtual async Task<OperateResult> WriteAsync(string address, bool[] value)
	{
		return await Task.Run(() => Write(address, value));
	}

	public virtual async Task<OperateResult> WriteAsync(string address, bool value)
	{
		return await WriteAsync(address, new bool[1] { value });
	}

	public async Task<OperateResult<T>> ReadCustomerAsync<T>(string address) where T : IDataTransfer, new()
	{
		OperateResult<T> result = new OperateResult<T>();
		T Content = new();
		OperateResult<byte[]> read = await ReadAsync(address, Content.ReadCount);
		if (read.IsSuccess)
		{
			Content.ParseSource(read.Content);
			result.Content = Content;
			result.IsSuccess = true;
		}
		else
		{
			result.ErrorCode = read.ErrorCode;
			result.Message = read.Message;
		}
		return result;
	}

	public async Task<OperateResult> WriteCustomerAsync<T>(string address, T data) where T : IDataTransfer, new()
	{
		return await WriteAsync(address, data.ToSource());
	}

	public virtual async Task<OperateResult<T>> ReadAsync<T>() where T : class, new()
	{
		return await ReflectionHelper.ReadAsync<T>(this);
	}

	public virtual async Task<OperateResult> WriteAsync<T>(T data) where T : class, new()
	{
		return await ReflectionHelper.WriteAsync(data, this);
	}

	public async Task<OperateResult<short>> ReadInt16Async(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadInt16Async(address, 1));
	}

	public virtual async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength)), (byte[] m) => ByteTransform.TransInt16(m, 0, length));
	}

	public async Task<OperateResult<ushort>> ReadUInt16Async(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadUInt16Async(address, 1));
	}

	public virtual async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength)), (byte[] m) => ByteTransform.TransUInt16(m, 0, length));
	}

	public async Task<OperateResult<int>> ReadInt32Async(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadInt32Async(address, 1));
	}

	public virtual async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransInt32(m, 0, length));
	}

	public async Task<OperateResult<uint>> ReadUInt32Async(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadUInt32Async(address, 1));
	}

	public virtual async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransUInt32(m, 0, length));
	}

	public async Task<OperateResult<float>> ReadFloatAsync(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadFloatAsync(address, 1));
	}

	public virtual async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransSingle(m, 0, length));
	}

	public async Task<OperateResult<long>> ReadInt64Async(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadInt64Async(address, 1));
	}

	public virtual async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransInt64(m, 0, length));
	}

	public async Task<OperateResult<ulong>> ReadUInt64Async(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadUInt64Async(address, 1));
	}

	public virtual async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransUInt64(m, 0, length));
	}

	public async Task<OperateResult<double>> ReadDoubleAsync(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadDoubleAsync(address, 1));
	}

	public virtual async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransDouble(m, 0, length));
	}

	public async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
	{
		return await ReadStringAsync(address, length, Encoding.ASCII);
	}

	public virtual async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), (byte[] m) => ByteTransform.TransString(m, 0, m.Length, encoding));
	}

	public virtual async Task<OperateResult> WriteAsync(string address, short[] values)
	{
		return await WriteAsync(address, ByteTransform.TransByte(values));
	}

	public virtual async Task<OperateResult> WriteAsync(string address, short value)
	{
		return await WriteAsync(address, new short[1] { value });
	}

	public virtual async Task<OperateResult> WriteAsync(string address, ushort[] values)
	{
		return await WriteAsync(address, ByteTransform.TransByte(values));
	}

	public virtual async Task<OperateResult> WriteAsync(string address, ushort value)
	{
		return await WriteAsync(address, new ushort[1] { value });
	}

	public virtual async Task<OperateResult> WriteAsync(string address, int[] values)
	{
		return await WriteAsync(address, ByteTransform.TransByte(values));
	}

	public async Task<OperateResult> WriteAsync(string address, int value)
	{
		return await WriteAsync(address, new int[1] { value });
	}

	public virtual async Task<OperateResult> WriteAsync(string address, uint[] values)
	{
		return await WriteAsync(address, ByteTransform.TransByte(values));
	}

	public async Task<OperateResult> WriteAsync(string address, uint value)
	{
		return await WriteAsync(address, new uint[1] { value });
	}

	public virtual async Task<OperateResult> WriteAsync(string address, float[] values)
	{
		return await WriteAsync(address, ByteTransform.TransByte(values));
	}

	public async Task<OperateResult> WriteAsync(string address, float value)
	{
		return await WriteAsync(address, new float[1] { value });
	}

	public virtual async Task<OperateResult> WriteAsync(string address, long[] values)
	{
		return await WriteAsync(address, ByteTransform.TransByte(values));
	}

	public async Task<OperateResult> WriteAsync(string address, long value)
	{
		return await WriteAsync(address, new long[1] { value });
	}

	public virtual async Task<OperateResult> WriteAsync(string address, ulong[] values)
	{
		return await WriteAsync(address, ByteTransform.TransByte(values));
	}

	public async Task<OperateResult> WriteAsync(string address, ulong value)
	{
		return await WriteAsync(address, new ulong[1] { value });
	}

	public virtual async Task<OperateResult> WriteAsync(string address, double[] values)
	{
		return await WriteAsync(address, ByteTransform.TransByte(values));
	}

	public async Task<OperateResult> WriteAsync(string address, double value)
	{
		return await WriteAsync(address, new double[1] { value });
	}

	public virtual async Task<OperateResult> WriteAsync(string address, string value)
	{
		return await WriteAsync(address, value, Encoding.ASCII);
	}

	public virtual async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
	{
		byte[] temp = ByteTransform.TransByte(value, encoding);
		if (WordLength == 1)
		{
			temp = SoftBasic.ArrayExpandToLengthEven(temp);
		}
		return await WriteAsync(address, temp);
	}

	public virtual async Task<OperateResult> WriteAsync(string address, string value, int length)
	{
		return await WriteAsync(address, value, length, Encoding.ASCII);
	}

	public virtual async Task<OperateResult> WriteAsync(string address, string value, int length, Encoding encoding)
	{
		byte[] temp2 = ByteTransform.TransByte(value, encoding);
		if (WordLength == 1)
		{
			temp2 = SoftBasic.ArrayExpandToLengthEven(temp2);
		}
		temp2 = SoftBasic.ArrayExpandToLength(temp2, length);
		return await WriteAsync(address, temp2);
	}
}
