namespace Ops.Communication.Core;

/// <summary>
/// 字节倒序的转换类
/// </summary>
public sealed class ReverseBytesTransform : ByteTransformBase
{
	public ReverseBytesTransform()
	{
	}

	public ReverseBytesTransform(DataFormat dataFormat)
		: base(dataFormat)
	{
	}

	public override short TransInt16(byte[] buffer, int index)
	{
		return BitConverter.ToInt16(new byte[2]
		{
			buffer[1 + index],
			buffer[index],
		}, 0);
	}

	public override ushort TransUInt16(byte[] buffer, int index)
	{
		return BitConverter.ToUInt16(new byte[2]
		{
			buffer[1 + index],
			buffer[index],
		}, 0);
	}

	public override int TransInt32(byte[] buffer, int index)
	{
		return BitConverter.ToInt32(ByteTransDataFormat4(new byte[4]
		{
			buffer[3 + index],
			buffer[2 + index],
			buffer[1 + index],
			buffer[index],
		}), 0);
	}

	public override uint TransUInt32(byte[] buffer, int index)
	{
		return BitConverter.ToUInt32(ByteTransDataFormat4(new byte[4]
		{
			buffer[3 + index],
			buffer[2 + index],
			buffer[1 + index],
			buffer[index],
		}), 0);
	}

	public override long TransInt64(byte[] buffer, int index)
	{
		return BitConverter.ToInt64(ByteTransDataFormat8(new byte[8]
		{
			buffer[7 + index],
			buffer[6 + index],
			buffer[5 + index],
			buffer[4 + index],
			buffer[3 + index],
			buffer[2 + index],
			buffer[1 + index],
			buffer[index],
		}), 0);
	}

	public override ulong TransUInt64(byte[] buffer, int index)
	{
		return BitConverter.ToUInt64(ByteTransDataFormat8(new byte[8]
		{
			buffer[7 + index],
			buffer[6 + index],
			buffer[5 + index],
			buffer[4 + index],
			buffer[3 + index],
			buffer[2 + index],
			buffer[1 + index],
			buffer[index],
		}), 0);
	}

	public override float TransSingle(byte[] buffer, int index)
	{
		return BitConverter.ToSingle(ByteTransDataFormat4(new byte[4]
		{
			buffer[3 + index],
			buffer[2 + index],
			buffer[1 + index],
			buffer[index],
		}), 0);
	}

	public override double TransDouble(byte[] buffer, int index)
	{
		return BitConverter.ToDouble(ByteTransDataFormat8(new byte[8]
		{
			buffer[7 + index],
			buffer[6 + index],
			buffer[5 + index],
			buffer[4 + index],
			buffer[3 + index],
			buffer[2 + index],
			buffer[1 + index],
			buffer[index],
		}), 0);
	}

	public override byte[] TransByte(short[] values)
	{
		if (values == null)
		{
			return Array.Empty<byte>();
		}

		byte[] array = new byte[values.Length * 2];
		for (int i = 0; i < values.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(values[i]);
			Array.Reverse((Array)bytes);
			bytes.CopyTo(array, 2 * i);
		}
		return array;
	}

	public override byte[] TransByte(ushort[] values)
	{
		if (values == null)
		{
			return Array.Empty<byte>();
		}

		byte[] array = new byte[values.Length * 2];
		for (int i = 0; i < values.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(values[i]);
			Array.Reverse((Array)bytes);
			bytes.CopyTo(array, 2 * i);
		}
		return array;
	}

	public override byte[] TransByte(int[] values)
	{
		if (values == null)
		{
			return Array.Empty<byte>();
		}

		byte[] array = new byte[values.Length * 4];
		for (int i = 0; i < values.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(values[i]);
			Array.Reverse((Array)bytes);
			ByteTransDataFormat4(bytes).CopyTo(array, 4 * i);
		}
		return array;
	}

	public override byte[] TransByte(uint[] values)
	{
		if (values == null)
		{
			return Array.Empty<byte>();
		}

		byte[] array = new byte[values.Length * 4];
		for (int i = 0; i < values.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(values[i]);
			Array.Reverse((Array)bytes);
			ByteTransDataFormat4(bytes).CopyTo(array, 4 * i);
		}
		return array;
	}

	public override byte[] TransByte(long[] values)
	{
		if (values == null)
		{
			return Array.Empty<byte>();
		}

		byte[] array = new byte[values.Length * 8];
		for (int i = 0; i < values.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(values[i]);
			Array.Reverse((Array)bytes);
			ByteTransDataFormat8(bytes).CopyTo(array, 8 * i);
		}
		return array;
	}

	public override byte[] TransByte(ulong[] values)
	{
		if (values == null)
		{
			return Array.Empty<byte>();
		}

		byte[] array = new byte[values.Length * 8];
		for (int i = 0; i < values.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(values[i]);
			Array.Reverse((Array)bytes);
			ByteTransDataFormat8(bytes).CopyTo(array, 8 * i);
		}
		return array;
	}

	public override byte[] TransByte(float[] values)
	{
		if (values == null)
		{
			return Array.Empty<byte>();
		}

		byte[] array = new byte[values.Length * 4];
		for (int i = 0; i < values.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(values[i]);
			Array.Reverse((Array)bytes);
			ByteTransDataFormat4(bytes).CopyTo(array, 4 * i);
		}
		return array;
	}

	public override byte[] TransByte(double[] values)
	{
		if (values == null)
		{
			return Array.Empty<byte>();
		}

		byte[] array = new byte[values.Length * 8];
		for (int i = 0; i < values.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(values[i]);
			Array.Reverse((Array)bytes);
			ByteTransDataFormat8(bytes).CopyTo(array, 8 * i);
		}
		return array;
	}

	public override IByteTransform CreateByDateFormat(DataFormat dataFormat)
	{
		return new ReverseBytesTransform(dataFormat)
		{
			IsStringReverseByteWord = IsStringReverseByteWord,
		};
	}

	public override string ToString()
	{
		return $"ReverseBytesTransform[{base.DataFormat}]";
	}
}
