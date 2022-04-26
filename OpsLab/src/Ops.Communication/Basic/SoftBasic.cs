using System.Security.Cryptography;
using System.Text;
using Ops.Communication.Extensions;

namespace Ops.Communication.Basic;

/// <summary>
/// 一个软件基础类，提供常用的一些静态方法，比如字符串转换，字节转换的方法
/// </summary>
public class SoftBasic
{
	/// <summary>
	/// 获取文件的md5码
	/// </summary>
	/// <param name="filePath">文件的路径，既可以是完整的路径，也可以是相对的路径</param>
	/// <returns>Md5字符串</returns>
	public static string CalculateFileMD5(string filePath)
	{
		string result;
		using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
		{
			result = CalculateStreamMD5(stream);
		}
		return result;
	}

	/// <summary>
	/// 获取数据流的md5码
	/// </summary>
	/// <param name="stream">数据流，可以是内存流，也可以是文件流</param>
	/// <returns>Md5字符串</returns>
	public static string CalculateStreamMD5(Stream stream)
	{
		byte[] array;
		using (MD5 mD = MD5.Create())
		{
			array = mD.ComputeHash(stream);
		}
		return BitConverter.ToString(array).Replace("-", "");
	}

	/// <summary>
	/// 获取文本字符串信息的Md5码，编码为UTF8
	/// </summary>
	/// <param name="data">文本数据信息</param>
	/// <returns>Md5字符串</returns>
	public static string CalculateStreamMD5(string data)
	{
		return CalculateStreamMD5(data, Encoding.UTF8);
	}

	/// <summary>
	/// 获取文本字符串信息的Md5码，使用指定的编码
	/// </summary>
	/// <param name="data">文本数据信息</param>
	/// <param name="encode">编码信息</param>
	/// <returns>Md5字符串</returns>
	public static string CalculateStreamMD5(string data, Encoding encode)
	{
		string result;
		using (MD5 mD = MD5.Create())
		{
			byte[] array = mD.ComputeHash(encode.GetBytes(data));
			result = BitConverter.ToString(array).Replace("-", "");
		}
		return result;
	}

	/// <summary>
	/// 从一个字节大小返回带单位的描述，主要是用于显示操作
	/// </summary>
	/// <param name="size">实际的大小值</param>
	/// <returns>最终的字符串值</returns>
	public static string GetSizeDescription(long size)
	{
		if (size < 1000)
		{
			return size + " B";
		}

		if (size < 1000000)
		{
			return (size / 1024f).ToString("F2") + " Kb";
		}

		if (size < 1000000000)
		{
			return (size / 1024f / 1024f).ToString("F2") + " Mb";
		}

		return (size / 1024f / 1024f / 1024f).ToString("F2") + " Gb";
	}

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
	public static string ArrayFormat<T>(T[] array, string format)
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
	/// 将数组格式化为显示的字符串的信息，支持所有的类型对象
	/// </summary>
	/// <typeparam name="T">数组的类型</typeparam>
	/// <param name="array">数组信息</param>
	/// <returns>最终显示的信息</returns>
	public static string ArrayFormat<T>(T array)
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
	public static string ArrayFormat<T>(T array, string format)
	{
		var stringBuilder = new StringBuilder("[");
		var array2 = array as Array;
		if (array2 != null)
		{
			foreach (object item in array2)
			{
				stringBuilder.Append(string.IsNullOrEmpty(format) ? item.ToString() : string.Format(format, item));
				stringBuilder.Append(',');
			}
			if (array2.Length > 0 && stringBuilder[^1] == ',')
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
		}
		else
		{
			stringBuilder.Append(string.IsNullOrEmpty(format) ? array.ToString() : string.Format(format, array));
		}
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}

	/// <summary>
	/// 一个通用的数组新增个数方法，会自动判断越界情况，越界的情况下，会自动的截断或是填充
	/// </summary>
	/// <typeparam name="T">数据类型</typeparam>
	/// <param name="array">原数据</param>
	/// <param name="data">等待新增的数据</param>
	/// <param name="max">原数据的最大值</param>
	public static void AddArrayData<T>(ref T[] array, T[] data, int max)
	{
		if (data == null || data.Length == 0)
		{
			return;
		}

		if (array.Length == max)
		{
			Array.Copy(array, data.Length, array, 0, array.Length - data.Length);
			Array.Copy(data, 0, array, array.Length - data.Length, data.Length);
		}
		else if (array.Length + data.Length > max)
		{
			T[] array2 = new T[max];
			for (int i = 0; i < max - data.Length; i++)
			{
				array2[i] = array[i + (array.Length - max + data.Length)];
			}
			for (int j = 0; j < data.Length; j++)
			{
				array2[array2.Length - data.Length + j] = data[j];
			}
			array = array2;
		}
		else
		{
			T[] array3 = new T[array.Length + data.Length];
			for (int k = 0; k < array.Length; k++)
			{
				array3[k] = array[k];
			}
			for (int l = 0; l < data.Length; l++)
			{
				array3[array3.Length - data.Length + l] = data[l];
			}
			array = array3;
		}
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
		if (data == null)
		{
			return new T[length];
		}

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
		if (data == null)
		{
			return new T[0];
		}

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
		if (array == null)
		{
			return new List<T[]>();
		}

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
	/// 判断两个数据的令牌是否相等<br />
	/// Determines whether the tokens of two data are equal
	/// </summary>
	/// <param name="token1">第一个令牌</param>
	/// <param name="token2">第二个令牌</param>
	/// <returns>返回是否相等</returns>
	public static bool IsTwoTokenEqual(Guid token1, Guid token2)
	{
		return IsTwoBytesEqual(token1.ToByteArray(), 0, token2.ToByteArray(), 0, 16);
	}

	/// <summary>
	/// 获取一个枚举类型的所有枚举值，可直接应用于组合框数据
	/// </summary>
	/// <typeparam name="TEnum">枚举的类型值</typeparam>
	/// <returns>枚举值数组</returns>
	public static TEnum[] GetEnumValues<TEnum>() where TEnum : struct
	{
		return (TEnum[])Enum.GetValues(typeof(TEnum));
	}

	/// <summary>
	/// 从字符串的枚举值数据转换成真实的枚举值数据
	/// </summary>
	/// <typeparam name="TEnum">枚举的类型值</typeparam>
	/// <param name="value">枚举的字符串的数据值</param>
	/// <returns>真实的枚举值</returns>
	public static TEnum GetEnumFromString<TEnum>(string value) where TEnum : struct
	{
		return (TEnum)Enum.Parse(typeof(TEnum), value);
	}

	/// <summary>
	/// 获取一个异常的完整错误信息<br />
	/// </summary>
	/// <param name="ex">异常对象</param>
	/// <returns>完整的字符串数据</returns>
	/// <remarks>获取异常的完整信息</remarks>
	/// <exception cref="NullReferenceException">ex不能为空</exception>
	public static string GetExceptionMessage(Exception ex)
	{
		return $"ExceptionMessage:{ex.Message}{Environment.NewLine}StackTrace:{ex.StackTrace}{Environment.NewLine}TargetSite:{ex.TargetSite}";
	}

	/// <summary>
	/// 获取一个异常的完整错误信息，和额外的字符串描述信息
	/// </summary>
	/// <param name="extraMsg">额外的信息</param>
	/// <param name="ex">异常对象</param>
	/// <returns>完整的字符串数据</returns>
	/// <exception cref="NullReferenceException"></exception>
	public static string GetExceptionMessage(string extraMsg, Exception ex)
	{
		if (string.IsNullOrEmpty(extraMsg))
		{
			return GetExceptionMessage(ex);
		}
		return extraMsg + Environment.NewLine + GetExceptionMessage(ex);
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
		if (InBytes == null)
		{
			return string.Empty;
		}

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

	/// <summary>
	/// 字符串数据转化成16进制表示的字符串
	/// </summary>
	/// <param name="InString">输入的字符串数据</param>
	/// <returns>返回的字符串</returns>
	/// <exception cref="NullReferenceException"></exception>
	public static string ByteToHexString(string InString)
	{
		return ByteToHexString(Encoding.Unicode.GetBytes(InString));
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
		if (inBytes == null)
		{
			return null;
		}
		if (inBytes.Length == 0)
		{
			return new byte[0];
		}

		byte[] array = ArrayExpandToLengthEven(inBytes.CopyArray());
		for (int i = 0; i < array.Length / 2; i++)
		{
			byte b = array[i * 2];
			array[i * 2] = array[i * 2 + 1];
			array[i * 2 + 1] = b;
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
		if (array == null)
		{
			return null;
		}

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
		if (InBytes == null)
		{
			return null;
		}

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
		if (value == null)
		{
			return Array.Empty<T>();
		}
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
		if (value == null)
		{
			return Array.Empty<T>();
		}

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
	/// 将一个<see cref="String" />的数组和多个 <see cref="String" /> 类型的对象整合成一个数组
	/// </summary>
	/// <param name="first">第一个数组对象</param>
	/// <param name="array">字符串数组信息</param>
	/// <returns>总的数组对象</returns>
	public static string[] SpliceStringArray(string first, string[] array)
	{
		var list = new List<string>(array.Length + 1);
		list.Add(first);
		list.AddRange(array);
		return list.ToArray();
	}

	/// <summary>
	/// 将两个<see cref="T:System.String" />的数组和多个<see cref="T:System.String" /> 类型的对象整合成一个数组。
	/// </summary>
	/// <param name="first">第一个数据对象</param>
	/// <param name="second">第二个数据对象</param>
	/// <param name="array">字符串数组信息</param>
	/// <returns>总的数组对象</returns>
	public static string[] SpliceStringArray(string first, string second, string[] array)
	{
		var list = new List<string>(array.Length + 2);
		list.Add(first);
		list.Add(second);
		list.AddRange(array);
		return list.ToArray();
	}

	/// <summary>
	/// 将两个<see cref="T:System.String" />的数组和多个<see cref="T:System.String" /> 类型的对象整合成一个数组。
	/// </summary>
	/// <param name="first">第一个数据对象</param>
	/// <param name="second">第二个数据对象</param>
	/// <param name="third">第三个数据对象</param>
	/// <param name="array">字符串数组信息</param>
	/// <returns>总的数组对象</returns>
	public static string[] SpliceStringArray(string first, string second, string third, string[] array)
	{
		var list = new List<string>(array.Length + 3);
		list.Add(first);
		list.Add(second);
		list.Add(third);
		list.AddRange(array);
		return list.ToArray();
	}

	/// <summary>
	/// 使用序列化反序列化深度克隆一个对象，该对象需要支持序列化特性
	/// </summary>
	/// <param name="oringinal">源对象，支持序列化</param>
	/// <returns>新的一个实例化的对象</returns>
	/// <exception cref="NotSupportedException"></exception>
	/// <exception cref="System.Text.Json.JsonException"></exception>
	/// <remarks>
	/// <note type="warning">
	/// <paramref name="oringinal" /> 参数必须实现序列化的特性
	/// </note>
	/// </remarks>
	public static object DeepClone(object oringinal)
	{
		// 因 BinaryFormatter 的安全漏洞，建议使用其他代替方案，
		// 参考 https://docs.microsoft.com/zh-cn/dotnet/standard/serialization/binaryformatter-security-guide
		var json = System.Text.Json.JsonSerializer.Serialize(oringinal);
		return System.Text.Json.JsonSerializer.Deserialize(json, oringinal.GetType());
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
