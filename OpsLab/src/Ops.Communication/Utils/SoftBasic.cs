using System.Text;
using Ops.Communication.Extensions;

namespace Ops.Communication.Utils;

/// <summary>
/// 一个软件基础类，提供常用的一些静态方法，比如字符串转换，字节转换的方法
/// </summary>
public class SoftBasic
{
	/// <summary>
	/// 从一个时间差返回带单位的描述，主要是用于显示操作。
	/// </summary>
	/// <param name="ts">实际的时间差</param>
	/// <returns>最终的字符串值</returns>
	public static string GetTimeSpanDescription(TimeSpan ts)
	{
		if (ts.TotalSeconds <= 60.0)
		{
			return (int)ts.TotalSeconds + "s";
		}

		if (ts.TotalMinutes <= 60.0)
		{
			return ts.TotalMinutes.ToString("F1") + "min";
		}

		if (ts.TotalHours <= 24.0)
		{
			return ts.TotalHours.ToString("F2") + "h";
		}

		return ts.TotalDays.ToString("F2") + "d";
	}

	/// <summary>
	/// 将数组格式化为显示的字符串的信息，支持所有的类型对象
	/// </summary>
	/// <typeparam name="T">数组的类型</typeparam>
	/// <param name="array">数组信息</param>
	/// <returns>最终显示的信息</returns>
	public static string ArrayFormat<T>(T[] array)
	{
		return ArrayFormat(array, string.Empty);
	}

	/// <summary>
	/// 将数组格式化为显示的字符串的信息，支持所有的类型对象
	/// </summary>
	/// <typeparam name="T">数组的类型</typeparam>
	/// <param name="array">数组信息</param>
	/// <param name="format">格式化的信息</param>
	/// <returns>最终显示的信息</returns>
	public static string ArrayFormat<T>(T[]? array, string format)
	{
		if (array == null)
		{
			return "NULL";
		}

		var stringBuilder = new StringBuilder("[");
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(string.IsNullOrEmpty(format) ? array[i].ToString() : string.Format(format, array[i]));
			if (i != array.Length - 1)
			{
				stringBuilder.Append(',');
			}
		}
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}

	/// <summary>
	/// 将一个数组进行扩充到指定长度，或是缩短到指定长度
	/// </summary>
	/// <typeparam name="T">数组的类型</typeparam>
	/// <param name="data">原先数据的数据</param>
	/// <param name="length">新数组的长度</param>
	/// <returns>新数组长度信息</returns>
	public static T[] ArrayExpandToLength<T>(T[] data, int length)
	{
		if (data.Length == length)
		{
			return data;
		}

		T[] array = new T[length];
		Array.Copy(data, array, Math.Min(data.Length, array.Length));
		return array;
	}

	/// <summary>
	/// 将一个数组进行扩充到偶数长度
	/// </summary>
	/// <typeparam name="T">数组的类型</typeparam>
	/// <param name="data">原先数据的数据</param>
	/// <returns>新数组长度信息</returns>
	public static T[] ArrayExpandToLengthEven<T>(T[] data)
	{
		if (data.Length % 2 == 1)
		{
			return ArrayExpandToLength(data, data.Length + 1);
		}
		return data;
	}

	/// <summary>
	/// 将指定的数据按照指定长度进行分割，例如int[10]，指定长度4，就分割成int[4],int[4],int[2]，然后拼接list
	/// </summary>
	/// <typeparam name="T">数组的类型</typeparam>
	/// <param name="array">等待分割的数组</param>
	/// <param name="length">指定的长度信息</param>
	/// <returns>分割后结果内容</returns>
	public static List<T[]> ArraySplitByLength<T>(T[] array, int length)
	{
		var list = new List<T[]>();
		int num = 0;
		while (num < array.Length)
		{
			if (num + length < array.Length)
			{
				T[] array2 = new T[length];
				Array.Copy(array, num, array2, 0, length);
				num += length;
				list.Add(array2);
			}
			else
			{
				T[] array3 = new T[array.Length - num];
				Array.Copy(array, num, array3, 0, array3.Length);
				num += length;
				list.Add(array3);
			}
		}
		return list;
	}

	/// <summary>
	/// 将整数进行有效的拆分成数组，指定每个元素的最大值
	/// </summary>
	/// <param name="integer">整数信息</param>
	/// <param name="everyLength">单个的数组长度</param>
	/// <returns>拆分后的数组长度</returns>
	public static int[] SplitIntegerToArray(int integer, int everyLength)
	{
		int[] array = new int[integer / everyLength + ((integer % everyLength != 0) ? 1 : 0)];
		for (int i = 0; i < array.Length; i++)
		{
			if (i == array.Length - 1)
			{
				array[i] = ((integer % everyLength == 0) ? everyLength : (integer % everyLength));
			}
			else
			{
				array[i] = everyLength;
			}
		}
		return array;
	}

	/// <summary>
	/// 判断两个字节的指定部分是否相同
	/// </summary>
	/// <param name="b1">第一个字节</param>
	/// <param name="start1">第一个字节的起始位置</param>
	/// <param name="b2">第二个字节</param>
	/// <param name="start2">第二个字节的起始位置</param>
	/// <param name="length">校验的长度</param>
	/// <returns>返回是否相等</returns>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public static bool IsTwoBytesEqual(byte[] b1, int start1, byte[] b2, int start2, int length)
	{
		if (b1 == null || b2 == null)
		{
			return false;
		}

		for (int i = 0; i < length; i++)
		{
			if (b1[i + start1] != b2[i + start2])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 判断两个字节的指定部分是否相同
	/// </summary>
	/// <param name="b1">第一个字节</param>
	/// <param name="b2">第二个字节</param>
	/// <returns>返回是否相等</returns>
	public static bool IsTwoBytesEqual(byte[] b1, byte[] b2)
	{
		if (b1 == null || b2 == null)
		{
			return false;
		}
		if (b1.Length != b2.Length)
		{
			return false;
		}
		return IsTwoBytesEqual(b1, 0, b2, 0, b1.Length);
	}

	/// <summary>
	/// 判断两个数据的令牌是否相等
	/// </summary>
	/// <param name="head">字节数据</param>
	/// <param name="token">GUID数据</param>
	/// <returns>返回是否相等</returns>
	public static bool IsByteTokenEqual(byte[] head, Guid token)
	{
		return IsTwoBytesEqual(head, 12, token.ToByteArray(), 0, 16);
	}

	/// <summary>
	/// 字节数据转化成16进制表示的字符串
	/// </summary>
	/// <param name="InBytes">字节数组</param>
	/// <returns>返回的字符串</returns>
	public static string ByteToHexString(byte[] InBytes)
	{
		return ByteToHexString(InBytes, '\0');
	}

	/// <summary>
	/// 字节数据转化成16进制表示的字符串
	/// </summary>
	/// <param name="InBytes">字节数组</param>
	/// <param name="segment">分割符</param>
	/// <returns>返回的字符串</returns>
	public static string ByteToHexString(byte[] InBytes, char segment)
	{
		return ByteToHexString(InBytes, segment, 0);
	}

	/// <summary>
	/// 字节数据转化成16进制表示的字符串
	/// </summary>
	/// <param name="InBytes">字节数组</param>
	/// <param name="segment">分割符</param>
	/// <param name="newLineCount">每隔指定数量的时候进行换行</param>
	/// <returns>返回的字符串</returns>
	public static string ByteToHexString(byte[] InBytes, char segment, int newLineCount)
	{
		var stringBuilder = new StringBuilder();
		long num = 0L;
		foreach (byte b in InBytes)
		{
			if (segment == '\0')
			{
				stringBuilder.Append($"{b:X2}");
			}
			else
			{
				stringBuilder.Append($"{b:X2}{segment}");
			}
			num++;
			if (newLineCount > 0 && num >= newLineCount)
			{
				stringBuilder.Append(Environment.NewLine);
				num = 0L;
			}
		}

		if (segment != 0 && stringBuilder.Length > 1 && stringBuilder[stringBuilder.Length - 1] == segment)
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
		}
		return stringBuilder.ToString();
	}

	private static int GetHexCharIndex(char ch)
	{
            return ch switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                'A' or 'a' => 10,
                'B' or 'b' => 11,
                'C' or 'c' => 12,
                'D' or 'd' => 13,
                'E' or 'e' => 14,
                'F' or 'f' => 15,
                _ => -1,
            };
        }

	/// <summary>
	/// 将16进制的字符串转化成Byte数据，将检测每2个字符转化，也就是说，中间可以是任意字符
	/// </summary>
	/// <param name="hex">十六进制的字符串，中间可以是任意的分隔符</param>
	/// <returns>转换后的字节数组</returns>
	/// <remarks>参数举例：AA 01 34 A8</remarks>
	public static byte[] HexStringToBytes(string hex)
	{
		using var memoryStream = new MemoryStream();
		for (int i = 0; i < hex.Length; i++)
		{
			if (i + 1 < hex.Length && GetHexCharIndex(hex[i]) >= 0 && GetHexCharIndex(hex[i + 1]) >= 0)
			{
				memoryStream.WriteByte((byte)(GetHexCharIndex(hex[i]) * 16 + GetHexCharIndex(hex[i + 1])));
				i++;
			}
		}

		byte[] result = memoryStream.ToArray();
		return result;
	}

	/// <summary>
	/// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐
	/// </summary>
	/// <param name="inBytes">输入的字节信息</param>
	/// <returns>反转后的数据</returns>
	public static byte[] BytesReverseByWord(byte[] inBytes)
	{
		if (inBytes.Length == 0)
		{
			return Array.Empty<byte>();
		}

		byte[] array = ArrayExpandToLengthEven(inBytes.CopyArray());
		for (int i = 0; i < array.Length / 2; i++)
		{
            (array[i * 2 + 1], array[i * 2]) = (array[i * 2], array[i * 2 + 1]);
        }
        return array;
	}

	/// <summary>
	/// 将原始的byte数组转换成ascii格式的byte数组
	/// </summary>
	/// <param name="inBytes">等待转换的byte数组</param>
	/// <returns>转换后的数组</returns>
	public static byte[] BytesToAsciiBytes(byte[] inBytes)
	{
		return Encoding.ASCII.GetBytes(ByteToHexString(inBytes));
	}

	/// <summary>
	/// 将ascii格式的byte数组转换成原始的byte数组
	/// </summary>
	/// <param name="inBytes">等待转换的byte数组</param>
	/// <returns>转换后的数组</returns>
	public static byte[] AsciiBytesToBytes(byte[] inBytes)
	{
		return HexStringToBytes(Encoding.ASCII.GetString(inBytes));
	}

	/// <summary>
	/// 从字节构建一个ASCII格式的数据内容
	/// </summary>
	/// <param name="value">数据</param>
	/// <returns>ASCII格式的字节数组</returns>
	public static byte[] BuildAsciiBytesFrom(byte value)
	{
		return Encoding.ASCII.GetBytes(value.ToString("X2"));
	}

	/// <summary>
	/// 从short构建一个ASCII格式的数据内容
	/// </summary>
	/// <param name="value">数据</param>
	/// <returns>ASCII格式的字节数组</returns>
	public static byte[] BuildAsciiBytesFrom(short value)
	{
		return Encoding.ASCII.GetBytes(value.ToString("X4"));
	}

	/// <summary>
	/// 从ushort构建一个ASCII格式的数据内容
	/// </summary>
	/// <param name="value">数据</param>
	/// <returns>ASCII格式的字节数组</returns>
	public static byte[] BuildAsciiBytesFrom(ushort value)
	{
		return Encoding.ASCII.GetBytes(value.ToString("X4"));
	}

	/// <summary>
	/// 从uint构建一个ASCII格式的数据内容
	/// </summary>
	/// <param name="value">数据</param>
	/// <returns>ASCII格式的字节数组</returns>
	public static byte[] BuildAsciiBytesFrom(uint value)
	{
		return Encoding.ASCII.GetBytes(value.ToString("X8"));
	}

	/// <summary>
	/// 从字节数组构建一个ASCII格式的数据内容
	/// </summary>
	/// <param name="value">字节信息</param>
	/// <returns>ASCII格式的地址</returns>
	public static byte[] BuildAsciiBytesFrom(byte[] value)
	{
		byte[] array = new byte[value.Length * 2];
		for (int i = 0; i < value.Length; i++)
		{
			BuildAsciiBytesFrom(value[i]).CopyTo(array, 2 * i);
		}
		return array;
	}

	private static byte GetDataByBitIndex(int offset)
	{
		return offset switch
		{
			0 => 1, 
			1 => 2, 
			2 => 4, 
			3 => 8, 
			4 => 16, 
			5 => 32, 
			6 => 64, 
			7 => 128, 
			_ => 0, 
		};
	}

	/// <summary>
	/// 获取byte数据类型的第offset位，是否为True
	/// </summary>
	/// <param name="value">byte数值</param>
	/// <param name="offset">索引位置</param>
	/// <returns>结果</returns>
	public static bool BoolOnByteIndex(byte value, int offset)
	{
		byte dataByBitIndex = GetDataByBitIndex(offset);
		return (value & dataByBitIndex) == dataByBitIndex;
	}

	/// <summary>
	/// 设置取byte数据类型的第offset位，是否为True
	/// </summary>
	/// <param name="byt">byte数值</param>
	/// <param name="offset">索引位置</param>
	/// <param name="value">写入的结果值</param>
	/// <returns>结果</returns>
	public static byte SetBoolOnByteIndex(byte byt, int offset, bool value)
	{
		byte dataByBitIndex = GetDataByBitIndex(offset);
		if (value)
		{
			return (byte)(byt | dataByBitIndex);
		}
		return (byte)(byt & ~dataByBitIndex);
	}

	/// <summary>
	/// 将bool数组转换到byte数组
	/// </summary>
	/// <param name="array">bool数组</param>
	/// <returns>转换后的字节数组</returns>
	public static byte[] BoolArrayToByte(bool[] array)
	{
		int num = ((array.Length % 8 == 0) ? (array.Length / 8) : (array.Length / 8 + 1));
		byte[] array2 = new byte[num];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i])
			{
				array2[i / 8] += GetDataByBitIndex(i % 8);
			}
		}
		return array2;
	}

	/// <summary>
	/// 从Byte数组中提取位数组，length代表位数
	/// </summary>
	/// <param name="InBytes">原先的字节数组</param>
	/// <param name="length">想要转换的长度，如果超出自动会缩小到数组最大长度</param>
	/// <returns>转换后的bool数组</returns>
	public static bool[] ByteToBoolArray(byte[] InBytes, int length)
	{
		if (length > InBytes.Length * 8)
		{
			length = InBytes.Length * 8;
		}
		bool[] array = new bool[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = BoolOnByteIndex(InBytes[i / 8], i % 8);
		}
		return array;
	}

	/// <summary>
	/// 从Byte数组中提取所有的位数组
	/// </summary>
	/// <param name="InBytes">原先的字节数组</param>
	/// <returns>转换后的bool数组</returns>
	public static bool[] ByteToBoolArray(byte[] InBytes)
	{
		return InBytes == null ? Array.Empty<bool>() : ByteToBoolArray(InBytes, InBytes.Length * 8);
	}

	/// <summary>
	/// 将一个数组的前后移除指定位数，返回新的一个数组
	/// </summary>
	/// <param name="value">数组</param>
	/// <param name="leftLength">前面的位数</param>
	/// <param name="rightLength">后面的位数</param>
	/// <returns>新的数组</returns>
	/// <exception cref="RankException"></exception>
	public static T[] ArrayRemoveDouble<T>(T[] value, int leftLength, int rightLength)
	{
		if (value.Length <= leftLength + rightLength)
		{
			return Array.Empty<T>();
		}

		T[] array = new T[value.Length - leftLength - rightLength];
		Array.Copy(value, leftLength, array, 0, array.Length);
		return array;
	}

	/// <summary>
	/// 将一个数组的前面指定位数移除，返回新的一个数组
	/// </summary>
	/// <param name="value">数组</param>
	/// <param name="length">等待移除的长度</param>
	/// <returns>新的数组</returns>
	/// <exception cref="RankException"></exception>
	public static T[] ArrayRemoveBegin<T>(T[] value, int length)
	{
		return ArrayRemoveDouble(value, length, 0);
	}

	/// <summary>
	/// 将一个数组的后面指定位数移除，返回新的一个数组
	/// </summary>
	/// <param name="value">数组</param>
	/// <param name="length">等待移除的长度</param>
	/// <returns>新的数组</returns>
	/// <exception cref="RankException"></exception>
	public static T[] ArrayRemoveLast<T>(T[] value, int length)
	{
		return ArrayRemoveDouble(value, 0, length);
	}

	/// <summary>
	/// 获取到数组里面的中间指定长度的数组
	/// </summary>
	/// <param name="value">数组</param>
	/// <param name="index">起始索引</param>
	/// <param name="length">数据的长度</param>
	/// <returns>新的数组值</returns>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public static T[] ArraySelectMiddle<T>(T[] value, int index, int length)
	{
		T[] array = new T[Math.Min(value.Length, length)];
		Array.Copy(value, index, array, 0, array.Length);
		return array;
	}

	/// <summary>
	/// 选择一个数组的前面的几个数据信息
	/// </summary>
	/// <param name="value">数组</param>
	/// <param name="length">数据的长度</param>
	/// <returns>新的数组</returns>
	public static T[] ArraySelectBegin<T>(T[] value, int length)
	{
		T[] array = new T[Math.Min(value.Length, length)];
		if (array.Length != 0)
		{
			Array.Copy(value, 0, array, 0, array.Length);
		}
		return array;
	}

	/// <summary>
	/// 选择一个数组的后面的几个数据信息
	/// </summary>
	/// <param name="value">数组</param>
	/// <param name="length">数据的长度</param>
	/// <returns>新的数组信息</returns>
	public static T[] ArraySelectLast<T>(T[] value, int length)
	{
		T[] array = new T[Math.Min(value.Length, length)];
		Array.Copy(value, value.Length - length, array, 0, array.Length);
		return array;
	}

	/// <summary>
	/// 拼接任意个泛型数组为一个总的泛型数组对象，采用深度拷贝实现。
	/// </summary>
	/// <typeparam name="T">数组的类型信息</typeparam>
	/// <param name="arrays">任意个长度的数组</param>
	/// <returns>拼接之后的最终的结果对象</returns>
	public static T[] SpliceArray<T>(params T[][] arrays)
	{
		int num = 0;
		for (int i = 0; i < arrays.Length; i++)
		{
			T[] obj = arrays[i];
			if (obj != null && obj.Length != 0)
			{
				num += arrays[i].Length;
			}
		}

		int num2 = 0;
		T[] array = new T[num];
		for (int j = 0; j < arrays.Length; j++)
		{
			T[] obj2 = arrays[j];
			if (obj2 != null && obj2.Length != 0)
			{
				arrays[j].CopyTo(array, num2);
				num2 += arrays[j].Length;
			}
		}
		return array;
	}

	/// <summary>
	/// 获取一串唯一的随机字符串，长度为20，由Guid码和4位数的随机数组成，保证字符串的唯一性
	/// </summary>
	/// <returns>随机字符串数据</returns>
	public static string GetUniqueStringByGuidAndRandom()
	{
		var random = new Random();
		return Guid.NewGuid().ToString("N") + random.Next(1000, 10000);
	}
}
