namespace Ops.Communication.Core;

/// <summary>
/// 常规的字节转换类。
/// </summary>
public class RegularByteTransform : ByteTransformBase
{
	public RegularByteTransform()
	{
	}

	public RegularByteTransform(DataFormat dataFormat)
		: base(dataFormat)
	{
	}

	public override IByteTransform CreateByDateFormat(DataFormat dataFormat)
	{
		return new RegularByteTransform(dataFormat)
		{
			IsStringReverseByteWord = base.IsStringReverseByteWord
		};
	}

	public override string ToString()
	{
		return $"RegularByteTransform[{base.DataFormat}]";
	}
}
