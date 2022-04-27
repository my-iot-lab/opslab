using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Ops.Communication.Basic;

namespace Ops.Communication.Core;

/// <summary>
/// 一些静态辅助方法。
/// </summary>
public class OpsHelper
{
	/// <summary>
	/// 解析地址的附加参数方法，比如你的地址是s=100;D100，可以提取出"s"的值的同时，修改地址本身，如果"s"不存在的话，返回给定的默认值。
	/// </summary>
	/// <param name="address">复杂的地址格式，比如：s=100;D100</param>
	/// <param name="paraName">等待提取的参数名称</param>
	/// <param name="defaultValue">如果提取的参数信息不存在，返回的默认值信息</param>
	/// <returns>解析后的新的数据值或是默认的给定的数据值</returns>
	public static int ExtractParameter(ref string address, string paraName, int defaultValue)
	{
		OperateResult<int> operateResult = ExtractParameter(ref address, paraName);
		return operateResult.IsSuccess ? operateResult.Content : defaultValue;
	}

	/// <summary>
	/// 解析地址的附加参数方法，比如你的地址是s=100;D100，可以提取出"s"的值的同时，修改地址本身，如果"s"不存在的话，返回错误的消息内容。
	/// </summary>
	/// <param name="address">复杂的地址格式，比如：s=100;D100</param>
	/// <param name="paraName">等待提取的参数名称</param>
	/// <returns>解析后的参数结果内容</returns>
	public static OperateResult<int> ExtractParameter(ref string address, string paraName)
	{
		try
		{
			Match match = Regex.Match(address, $"{paraName}=[0-9A-Fa-fx]+;");
			if (!match.Success)
			{
				return new OperateResult<int>($"Address [{address}] can't find [{paraName}] Parameters. for example: {paraName}=1;100");
			}

			string text = match.Value.Substring(paraName.Length + 1, match.Value.Length - paraName.Length - 2);
			int value = text.StartsWith("0x") ? Convert.ToInt32(text[2..], 16) : (text.StartsWith("0") ? Convert.ToInt32(text, 8) : Convert.ToInt32(text));
			address = address.Replace(match.Value, "");
			return OperateResult.Ok(value);
		}
		catch (Exception ex)
		{
			return new OperateResult<int>($"Address [{address}] Get [{paraName}] Parameters failed: {ex.Message}");
		}
	}

	/// <summary>
	/// 解析地址的起始地址的方法，比如你的地址是 A[1] , 那么将会返回 1，地址修改为 A，如果不存在起始地址，那么就不修改地址，返回 -1。
	/// </summary>
	/// <param name="address">复杂的地址格式，比如：A[0] </param>
	/// <returns>如果存在，就起始位置，不存在就返回 -1</returns>
	public static int ExtractStartIndex(ref string address)
	{
		try
		{
			Match match = Regex.Match(address, "\\[[0-9]+\\]$");
			if (!match.Success)
			{
				return -1;
			}

			string value = match.Value[1..^1];
			int result = Convert.ToInt32(value);
			address = address.Remove(address.Length - match.Value.Length);
			return result;
		}
		catch
		{
			return -1;
		}
	}

	/// <summary>
	/// 解析地址的附加<see cref="DataFormat" />参数方法，比如你的地址是format=ABCD;D100，可以提取出"format"的值的同时，修改地址本身，
	/// 如果"format"不存在的话，返回默认的<see cref="IByteTransform" />对象。
	/// </summary>
	/// <param name="address">复杂的地址格式，比如：format=ABCD;D100</param>
	/// <param name="defaultTransform">默认的数据转换信息</param>
	/// <returns>解析后的参数结果内容</returns>
	public static IByteTransform ExtractTransformParameter(ref string address, IByteTransform defaultTransform)
	{
		try
		{
			string text = "format";
			Match match = Regex.Match(address, $"{text}=(ABCD|BADC|DCBA|CDAB);", RegexOptions.IgnoreCase);
			if (!match.Success)
			{
				return defaultTransform;
			}

			string text2 = match.Value.Substring(text.Length + 1, match.Value.Length - text.Length - 2);
			DataFormat dataFormat = defaultTransform.DataFormat;
			switch (text2.ToUpper())
			{
				case "ABCD":
					dataFormat = DataFormat.ABCD;
					break;
				case "BADC":
					dataFormat = DataFormat.BADC;
					break;
				case "DCBA":
					dataFormat = DataFormat.DCBA;
					break;
				case "CDAB":
					dataFormat = DataFormat.CDAB;
					break;
			}
			address = address.Replace(match.Value, "");
			if (dataFormat != defaultTransform.DataFormat)
			{
				return defaultTransform.CreateByDateFormat(dataFormat);
			}
			return defaultTransform;
		}
		catch
		{
			throw;
		}
	}

	/// <summary>
	/// 切割当前的地址数据信息，根据读取的长度来分割成多次不同的读取内容，需要指定地址，总的读取长度，切割读取长度。
	/// </summary>
	/// <param name="address">整数的地址信息</param>
	/// <param name="length">读取长度信息</param>
	/// <param name="segment">切割长度信息</param>
	/// <returns>切割结果</returns>
	public static OperateResult<int[], int[]> SplitReadLength(int address, ushort length, ushort segment)
	{
		int[] array = SoftBasic.SplitIntegerToArray(length, segment);
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			if (i == 0)
			{
				array2[i] = address;
			}
			else
			{
				array2[i] = array2[i - 1] + array[i - 1];
			}
		}
		return OperateResult.Ok(array2, array);
	}

	/// <summary>
	/// 从当前的字符串信息获取IP地址数据，如果是ip地址直接返回，如果是域名，会自动解析IP地址，否则抛出异常。
	/// </summary>
	/// <param name="value">输入的字符串信息</param>
	/// <returns>真实的IP地址信息</returns>
	public static string GetIpAddressFromInput(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			if (Regex.IsMatch(value, "^[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+$"))
			{
				if (!IPAddress.TryParse(value, out var _))
				{
					throw new Exception("IpAddressError");
				}
				return value;
			}

			IPHostEntry hostEntry = Dns.GetHostEntry(value);
			IPAddress[] addressList = hostEntry.AddressList;
			if (addressList.Length != 0)
			{
				return addressList[0].ToString();
			}
		}
		return "127.0.0.1";
	}

	/// <summary>
	/// <b>[商业授权]</b> 将原始的字节数组，转换成实际的结构体对象，需要事先定义好结构体内容，否则会转换失败。
	/// </summary>
	/// <typeparam name="T">自定义的结构体</typeparam>
	/// <param name="content">原始的字节内容</param>
	/// <returns>是否成功的结果对象</returns>
	public static OperateResult<T> ByteArrayToStruct<T>(byte[] content) where T : struct
	{
		int num = Marshal.SizeOf(typeof(T));
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		try
		{
			Marshal.Copy(content, 0, intPtr, num);
			T value = Marshal.PtrToStructure<T>(intPtr);
			Marshal.FreeHGlobal(intPtr);
			return OperateResult.Ok(value);
		}
		catch (Exception ex)
		{
			Marshal.FreeHGlobal(intPtr);
			return new OperateResult<T>(ex.Message);
		}
	}

	/// <summary>
	/// 根据当前的位偏移地址及读取位长度信息，计算出实际的字节索引，字节数，字节位偏移
	/// </summary>
	/// <param name="addressStart">起始地址</param>
	/// <param name="length">读取的长度</param>
	/// <param name="newStart">返回的新的字节的索引，仍然按照位单位</param>
	/// <param name="byteLength">字节长度</param>
	/// <param name="offset">当前偏移的信息</param>
	public static void CalculateStartBitIndexAndLength(int addressStart, ushort length, out int newStart, out ushort byteLength, out int offset)
	{
		byteLength = (ushort)((addressStart + length - 1) / 8 - addressStart / 8 + 1);
		offset = addressStart % 8;
		newStart = addressStart - offset;
	}

	/// <summary>
	/// 根据字符串内容，获取当前的位索引地址，例如输入 6,返回6，输入15，返回15，输入B，返回11
	/// </summary>
	/// <param name="bit">位字符串</param>
	/// <returns>结束数据</returns>
	public static int CalculateBitStartIndex(string bit)
	{
		Span<char> arr = stackalloc char[] { 'A', 'B', 'C', 'D', 'E', 'F' };
		if (bit.AsSpan().IndexOfAny(arr) != -1)
		{
			return Convert.ToInt32(bit, 16);
		}

		return Convert.ToInt32(bit);
	}

	/// <summary>
	/// 将一个一维数组中的所有数据按照行列信息拷贝到二维数组里，返回当前的二维数组
	/// </summary>
	/// <typeparam name="T">数组的类型对象</typeparam>
	/// <param name="array">一维数组信息</param>
	/// <param name="row">行</param>
	/// <param name="col">列</param>
	public static T[,] CreateTwoArrayFromOneArray<T>(T[] array, int row, int col)
	{
		T[,] array2 = new T[row, col];
		int num = 0;
		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < col; j++)
			{
				array2[i, j] = array[num];
				num++;
			}
		}
		return array2;
	}
}
