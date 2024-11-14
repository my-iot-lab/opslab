using System.Runtime.InteropServices;

namespace Ops.Communication;

/// <summary>
/// 用于网络传递的信息头，使用上等同于 int
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct NetHandle
{
    /// <summary>
    /// 完整的暗号值
    /// </summary>
    [field: FieldOffset(0)]
    public int CodeValue { get; set; }

    /// <summary>
    /// 主暗号分类0-255
    /// </summary>
    [field: FieldOffset(3)]
    public byte CodeMajor { get; private set; }

    /// <summary>
    /// 次要的暗号分类0-255
    /// </summary>
    [field: FieldOffset(2)]
    public byte CodeMinor { get; private set; }

    /// <summary>
    /// 暗号的编号分类0-65535
    /// </summary>
    [field: FieldOffset(0)]
    public ushort CodeIdentifier { get; private set; }

    /// <summary>
    /// 赋值操作，可以直接赋值int数据
    /// </summary>
    /// <param name="value">int数值</param>
    /// <returns>等值的消息对象</returns>
    public static implicit operator NetHandle(int value)
	{
		return new NetHandle(value);
	}

	/// <summary>
	/// 也可以赋值给int数据
	/// </summary>
	/// <param name="netHandle">netHandle对象</param>
	/// <returns>等值的消息对象</returns>
	public static implicit operator int(NetHandle netHandle)
	{
		return netHandle.CodeValue;
	}

	/// <summary>
	/// 判断是否相等
	/// </summary>
	/// <param name="netHandle1">第一个数</param>
	/// <param name="netHandle2">第二个数</param>
	/// <returns>等于返回<c>True</c>，否则<c>False</c></returns>
	public static bool operator ==(NetHandle netHandle1, NetHandle netHandle2)
	{
		return netHandle1.CodeValue == netHandle2.CodeValue;
	}

	/// <summary>
	/// 判断是否不相等
	/// </summary>
	/// <param name="netHandle1">第一个对象</param>
	/// <param name="netHandle2">第二个对象</param>
	/// <returns>等于返回<c>False</c>，否则<c>True</c></returns>
	public static bool operator !=(NetHandle netHandle1, NetHandle netHandle2)
	{
		return netHandle1.CodeValue != netHandle2.CodeValue;
	}

	/// <summary>
	/// 两个数值相加
	/// </summary>
	/// <param name="netHandle1">第一个对象</param>
	/// <param name="netHandle2">第二个对象</param>
	/// <returns>返回两个指令的和</returns>
	public static NetHandle operator +(NetHandle netHandle1, NetHandle netHandle2)
	{
		return new NetHandle(netHandle1.CodeValue + netHandle2.CodeValue);
	}

	/// <summary>
	/// 两个数值相减
	/// </summary>
	/// <param name="netHandle1">第一个对象</param>
	/// <param name="netHandle2">第二个对象</param>
	/// <returns>返回两个指令的差</returns>
	public static NetHandle operator -(NetHandle netHandle1, NetHandle netHandle2)
	{
		return new NetHandle(netHandle1.CodeValue - netHandle2.CodeValue);
	}

	/// <summary>
	/// 判断是否小于另一个数值
	/// </summary>
	/// <param name="netHandle1">第一个对象</param>
	/// <param name="netHandle2">第二个对象</param>
	/// <returns>小于则返回<c>True</c>，否则返回<c>False</c></returns>
	public static bool operator <(NetHandle netHandle1, NetHandle netHandle2)
	{
		return netHandle1.CodeValue < netHandle2.CodeValue;
	}

	/// <summary>
	/// 判断是否大于另一个数值
	/// </summary>
	/// <param name="netHandle1">第一个对象</param>
	/// <param name="netHandle2">第二个对象</param>
	/// <returns>大于则返回<c>True</c>，否则返回<c>False</c></returns>
	public static bool operator >(NetHandle netHandle1, NetHandle netHandle2)
	{
		return netHandle1.CodeValue > netHandle2.CodeValue;
	}

	/// <summary>
	/// 初始化一个暗号对象
	/// </summary>
	/// <param name="value">使用一个默认的数值进行初始化</param>
	public NetHandle(int value)
	{
		CodeMajor = 0;
		CodeMinor = 0;
		CodeIdentifier = 0;
		CodeValue = value;
	}

	/// <summary>
	/// 根据三个值来初始化暗号对象
	/// </summary>
	/// <param name="major">主暗号</param>
	/// <param name="minor">次暗号</param>
	/// <param name="identifier">暗号编号</param>
	public NetHandle(byte major, byte minor, ushort identifier)
	{
		CodeValue = 0;
		CodeMajor = major;
		CodeMinor = minor;
		CodeIdentifier = identifier;
	}

	/// <summary>
	/// 获取完整的暗号数据
	/// </summary>
	/// <returns>返回暗号的字符串表示形式</returns>
	public override readonly string ToString()
	{
		return CodeValue.ToString();
	}

	/// <summary>
	/// 判断两个实例是否相同
	/// </summary>
	/// <param name="obj">对比的对象</param>
	/// <returns>相同返回<c>True</c>，否则返回<c>False</c></returns>
	public override bool Equals(object obj)
	{
		if (obj != null && obj is NetHandle obj1)
		{
			return CodeValue.Equals(obj1.CodeValue);
		}

		return false;
	}

	/// <summary>
	/// 获取哈希值
	/// </summary>
	/// <returns>返回当前对象的哈希值</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
