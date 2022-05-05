using Ops.Communication.Extensions;
using Ops.Communication.Utils;

namespace Ops.Communication.Core;

/// <summary>
/// 按照字节错位的数据转换类。
/// </summary>
public class ReverseWordTransform : ByteTransformBase
{
	public ReverseWordTransform()
	{
		base.DataFormat = DataFormat.ABCD;
	}

	public ReverseWordTransform(DataFormat dataFormat)
		: base(dataFormat)
	{
	}

	/// <summary>
	/// 按照字节错位的方法
	/// </summary>
	/// <param name="buffer">实际的字节数据</param>
	/// <param name="index">起始字节位置</param>
	/// <param name="length">数据长度</param>
	/// <returns>处理过的数据信息</returns>
	private byte[] ReverseBytesByWord(byte[] buffer, int index, int length)
	{
		if (buffer == null)
		{
			return Array.Empty<byte>();
		}
		return SoftBasic.BytesReverseByWord(buffer.SelectMiddle(index, length));
	}

	public override short TransInt16(byte[] buffer, int index)
	{
		return base.TransInt16(ReverseBytesByWord(buffer, index, 2), 0);
	}

	public override ushort TransUInt16(byte[] buffer, int index)
	{
		return base.TransUInt16(ReverseBytesByWord(buffer, index, 2), 0);
	}

	public override byte[] TransByte(short[] values)
	{
		byte[] inBytes = base.TransByte(values);
		return SoftBasic.BytesReverseByWord(inBytes);
	}

	public override byte[] TransByte(ushort[] values)
	{
		byte[] inBytes = base.TransByte(values);
		return SoftBasic.BytesReverseByWord(inBytes);
	}

	public override IByteTransform CreateByDateFormat(DataFormat dataFormat)
	{
		return new ReverseWordTransform(dataFormat)
		{
			IsStringReverseByteWord = base.IsStringReverseByteWord,
		};
	}

	public override string ToString()
	{
		return $"ReverseWordTransform[{base.DataFormat}]";
	}
}
