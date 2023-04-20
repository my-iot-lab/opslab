using System.Net.Sockets;
using Ops.Communication.Utils;

namespace Ops.Communication.Extensions;

/// <summary>
/// 扩展的辅助类方法
/// </summary>
public static class OpsExtension
{
	public static string ToHexString(this byte[] InBytes)
	{
		return SoftBasic.ByteToHexString(InBytes);
	}

	public static string ToHexString(this byte[] InBytes, char segment)
	{
		return SoftBasic.ByteToHexString(InBytes, segment);
	}

	public static string ToHexString(this byte[] InBytes, char segment, int newLineCount)
	{
		return SoftBasic.ByteToHexString(InBytes, segment, newLineCount);
	}

	public static byte[] ToHexBytes(this string value)
	{
		return SoftBasic.HexStringToBytes(value);
	}

	public static byte[] ToByteArray(this bool[] array)
	{
		return SoftBasic.BoolArrayToByte(array);
	}

	public static bool[] ToBoolArray(this byte[] InBytes, int length)
	{
		return SoftBasic.ByteToBoolArray(InBytes, length);
	}

	public static bool[] ToBoolArray(this byte[] InBytes)
	{
		return SoftBasic.ByteToBoolArray(InBytes);
	}

	/// <summary>
	/// 获取Byte数组的第 bytIndex 个位置的，boolIndex偏移的bool值
	/// </summary>
	/// <param name="bytes">字节数组信息</param>
	/// <param name="bytIndex">字节的偏移位置</param>
	/// <param name="boolIndex">指定字节的位偏移</param>
	/// <returns>bool值</returns>
	public static bool GetBoolValue(this byte[] bytes, int bytIndex, int boolIndex)
	{
		return SoftBasic.BoolOnByteIndex(bytes[bytIndex], boolIndex);
	}

	/// <summary>
	/// 获取Byte数组的第 boolIndex 偏移的bool值，这个偏移值可以为 10，就是第 1 个字节的 第3位
	/// </summary>
	/// <param name="bytes">字节数组信息</param>
	/// <param name="boolIndex">指定字节的位偏移</param>
	/// <returns>bool值</returns>
	public static bool GetBoolByIndex(this byte[] bytes, int boolIndex)
	{
		return SoftBasic.BoolOnByteIndex(bytes[boolIndex / 8], boolIndex % 8);
	}

	/// <summary>
	/// 获取Byte的第 boolIndex 偏移的bool值，比如3，就是第4位
	/// </summary>
	/// <param name="byt">字节信息</param>
	/// <param name="boolIndex">指定字节的位偏移</param>
	/// <returns>bool值</returns>
	public static bool GetBoolByIndex(this byte byt, int boolIndex)
	{
		return SoftBasic.BoolOnByteIndex(byt, boolIndex % 8);
	}

	/// <summary>
	/// 设置Byte的第 boolIndex 位的bool值，可以强制为 true 或是 false, 不影响其他的位
	/// </summary>
	/// <param name="byt">字节信息</param>
	/// <param name="boolIndex">指定字节的位偏移</param>
	/// <param name="value">bool的值</param>
	/// <returns>修改之后的byte值</returns>
	public static byte SetBoolByIndex(this byte byt, int boolIndex, bool value)
	{
		return SoftBasic.SetBoolOnByteIndex(byt, boolIndex, value);
	}

	public static T[] RemoveDouble<T>(this T[] value, int leftLength, int rightLength)
	{
		return SoftBasic.ArrayRemoveDouble(value, leftLength, rightLength);
	}

	public static T[] RemoveBegin<T>(this T[] value, int length)
	{
		return SoftBasic.ArrayRemoveBegin(value, length);
	}

	public static T[] RemoveLast<T>(this T[] value, int length)
	{
		return SoftBasic.ArrayRemoveLast(value, length);
	}

	public static T[] SelectMiddle<T>(this T[] value, int index, int length)
	{
		return SoftBasic.ArraySelectMiddle(value, index, length);
	}

	public static T[] SelectBegin<T>(this T[] value, int length)
	{
		return SoftBasic.ArraySelectBegin(value, length);
	}

	public static T[] SelectLast<T>(this T[] value, int length)
	{
		return SoftBasic.ArraySelectLast(value, length);
	}

	public static T[] SpliceArray<T>(this T[] value, params T[][] arrays)
	{
        List<T[]> list = new(arrays.Length + 1) { value };
        list.AddRange(arrays);
		return SoftBasic.SpliceArray(list.ToArray());
	}

	/// <summary>
	/// 拷贝当前的实例数组，是基于引用层的浅拷贝，如果类型为值类型，那就是深度拷贝，如果类型为引用类型，就是浅拷贝
	/// </summary>
	/// <typeparam name="T">类型对象</typeparam>
	/// <param name="value">数组对象</param>
	/// <returns>拷贝的结果内容</returns>
	public static T[] CopyArray<T>(this T[] value)
	{
		if (value == null)
		{
			return Array.Empty<T>();
		}

		T[] array = new T[value.Length];
		Array.Copy(value, array, value.Length);
		return array;
	}

	public static string ToArrayString<T>(this T[] value)
	{
		return SoftBasic.ArrayFormat(value);
	}

	public static string ToArrayString<T>(this T[] value, string format)
	{
		return SoftBasic.ArrayFormat(value, format);
	}

	/// <summary>
	/// 根据英文小数点进行切割字符串，去除空白的字符
	/// </summary>
	/// <param name="str">字符串本身</param>
	/// <returns>切割好的字符串数组，例如输入 "100.5"，返回 "100", "5"</returns>
	public static string[] SplitDot(this string str)
	{
        return str.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
	}

	/// <summary>
	/// 设置客户端的Socket的心跳时间信息
	/// </summary>
	/// <param name="socket"></param>
	/// <param name="keepAliveTime"></param>
	/// <param name="keepAliveInterval"></param>
	/// <returns></returns>
	public static int SetKeepAlive(this Socket socket, int keepAliveTime, int keepAliveInterval)
	{
		byte[] array = new byte[12];
		BitConverter.GetBytes((keepAliveTime >= 0) ? 1 : 0).CopyTo(array, 0); // 启用 KeepAlive
		BitConverter.GetBytes(keepAliveTime).CopyTo(array, 4); // 此时间段内若没有数据交互，则发送探测包
		BitConverter.GetBytes(keepAliveInterval).CopyTo(array, 8); // 发送探测包的时间间隔
		try
		{
			return socket.IOControl(IOControlCode.KeepAliveValues, array, null);
		}
		catch
		{
			return 0;
		}
	}
}
