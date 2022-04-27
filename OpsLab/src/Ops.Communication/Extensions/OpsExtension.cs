using System.Linq.Expressions;
using System.Net.Sockets;
using Ops.Communication.Basic;

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

	/// <summary>
	/// 获取当前数组的倒序数组，这是一个新的实例，不改变原来的数组值
	/// </summary>
	/// <param name="value">输入的原始数组</param>
	/// <returns>反转之后的数组信息</returns>
	public static T[] ReverseNew<T>(this T[] value)
	{
		T[] array = value.CopyArray();
		Array.Reverse((Array)array);
		return array;
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
		List<T[]> list = new(arrays.Length + 1);
		list.Add(value);
		list.AddRange(arrays);
		return SoftBasic.SpliceArray(list.ToArray());
	}

	/// <summary>
	/// 将指定的数据添加到数组的每个元素上去，使用表达式树的形式实现，将会修改原数组。不适用byte类型
	/// </summary>
	/// <typeparam name="T">数组的类型</typeparam>
	/// <param name="array">原始数据</param>
	/// <param name="value">数据值</param>
	/// <returns>返回的结果信息</returns>
	public static T[] IncreaseBy<T>(this T[] array, T value)
	{
		if (typeof(T) == typeof(byte))
		{
			var parameterExpression = Expression.Parameter(typeof(int), "first");
			var parameterExpression2 = Expression.Parameter(typeof(int), "second");
			var body = Expression.Add(parameterExpression, parameterExpression2);
			var expression = Expression.Lambda<Func<int, int, int>>(body, new ParameterExpression[2] { parameterExpression, parameterExpression2 });
			Func<int, int, int> func = expression.Compile();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (T)(object)(byte)func(Convert.ToInt32(array[i]), Convert.ToInt32(value));
			}
		}
		else
		{
			var parameterExpression3 = Expression.Parameter(typeof(T), "first");
			var parameterExpression4 = Expression.Parameter(typeof(T), "second");
			var body2 = Expression.Add(parameterExpression3, parameterExpression4);
			var expression2 = Expression.Lambda<Func<T, T, T>>(body2, new ParameterExpression[2] { parameterExpression3, parameterExpression4 });
			Func<T, T, T> func2 = expression2.Compile();
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = func2(array[j], value);
			}
		}
		return array;
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
			return null;
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
	/// 将字符串数组转换为实际的数据数组。例如字符串格式[1,2,3,4,5]，可以转成实际的数组对象
	/// </summary>
	/// <typeparam name="T">类型对象</typeparam>
	/// <param name="value">字符串数据</param>
	/// <param name="selector">转换方法</param>
	/// <returns>实际的数组</returns>
	public static T[] ToStringArray<T>(this string value, Func<string, T> selector)
	{
		if (value.IndexOf('[') >= 0)
		{
			value = value.Replace("[", "");
		}
		if (value.IndexOf(']') >= 0)
		{
			value = value.Replace("]", "");
		}
		string[] source = value.Split(new char[2] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
		return source.Select(selector).ToArray();
	}

	/// <summary>
	/// 将字符串数组转换为实际的数据数组。支持byte,sbyte,bool,short,ushort,int,uint,long,ulong,float,double，使用默认的十进制，
	/// 例如字符串格式[1,2,3,4,5]，可以转成实际的数组对象
	/// </summary>
	/// <typeparam name="T">类型对象</typeparam>
	/// <param name="value">字符串数据</param>
	/// <returns>实际的数组</returns>
	public static T[] ToStringArray<T>(this string value)
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(byte))
		{
			return (T[])(object)value.ToStringArray(byte.Parse);
		}
		if (typeFromHandle == typeof(sbyte))
		{
			return (T[])(object)value.ToStringArray(sbyte.Parse);
		}
		if (typeFromHandle == typeof(bool))
		{
			return (T[])(object)value.ToStringArray(bool.Parse);
		}
		if (typeFromHandle == typeof(short))
		{
			return (T[])(object)value.ToStringArray(short.Parse);
		}
		if (typeFromHandle == typeof(ushort))
		{
			return (T[])(object)value.ToStringArray(ushort.Parse);
		}
		if (typeFromHandle == typeof(int))
		{
			return (T[])(object)value.ToStringArray(int.Parse);
		}
		if (typeFromHandle == typeof(uint))
		{
			return (T[])(object)value.ToStringArray(uint.Parse);
		}
		if (typeFromHandle == typeof(long))
		{
			return (T[])(object)value.ToStringArray(long.Parse);
		}
		if (typeFromHandle == typeof(ulong))
		{
			return (T[])(object)value.ToStringArray(ulong.Parse);
		}
		if (typeFromHandle == typeof(float))
		{
			return (T[])(object)value.ToStringArray(float.Parse);
		}
		if (typeFromHandle == typeof(double))
		{
			return (T[])(object)value.ToStringArray(double.Parse);
		}
		if (typeFromHandle == typeof(DateTime))
		{
			return (T[])(object)value.ToStringArray(DateTime.Parse);
		}
		if (typeFromHandle == typeof(string))
		{
			return (T[])(object)value.ToStringArray((string m) => m);
		}
		throw new Exception("use ToArray<T>(Func<string,T>) method instead");
	}

	/// <summary>
	/// 启动接收数据，需要传入回调方法，传递对象
	/// </summary>
	/// <param name="socket">socket对象</param>
	/// <param name="callback">回调方法</param>
	/// <param name="obj">数据对象</param>
	/// <returns>是否启动成功</returns>
	public static OperateResult BeginReceiveResult(this Socket socket, AsyncCallback callback, object obj)
	{
		try
		{
			socket.BeginReceive(new byte[0], 0, 0, SocketFlags.None, callback, obj);
			return OperateResult.Ok();
		}
		catch (Exception ex)
		{
			socket?.Close();
			return new OperateResult(ex.Message);
		}
	}

	/// <summary>
	/// 启动接收数据，需要传入回调方法，传递对象默认为socket本身
	/// </summary>
	/// <param name="socket">socket对象</param>
	/// <param name="callback">回调方法</param>
	/// <returns>是否启动成功</returns>
	public static OperateResult BeginReceiveResult(this Socket socket, AsyncCallback callback)
	{
		return socket.BeginReceiveResult(callback, socket);
	}

	/// <summary>
	/// 结束挂起的异步读取，返回读取的字节数，如果成功的情况。
	/// </summary>
	/// <param name="socket">socket对象</param>
	/// <param name="ar">回调方法</param>
	/// <returns>是否启动成功</returns>
	public static OperateResult<int> EndReceiveResult(this Socket socket, IAsyncResult ar)
	{
		try
		{
			return OperateResult.Ok(socket.EndReceive(ar));
		}
		catch (Exception ex)
		{
			socket?.Close();
			return new OperateResult<int>(ex.Message);
		}
	}

	/// <summary>
	/// 根据英文小数点进行切割字符串，去除空白的字符
	/// </summary>
	/// <param name="str">字符串本身</param>
	/// <returns>切割好的字符串数组，例如输入 "100.5"，返回 "100", "5"</returns>
	public static string[] SplitDot(this string str)
	{
		return str.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
	}
}
