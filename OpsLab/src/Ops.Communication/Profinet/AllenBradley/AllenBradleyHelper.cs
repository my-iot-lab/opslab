using System.Text;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.AllenBradley;

/// <summary>
/// AB PLC的辅助类，用来辅助生成基本的指令信息
/// </summary>
public static class AllenBradleyHelper
{
	/// <summary>
	/// CIP命令中的读取数据的服务
	/// </summary>
	public const byte CIP_READ_DATA = 76;

	/// <summary>
	/// CIP命令中的写数据的服务
	/// </summary>
	public const int CIP_WRITE_DATA = 77;

	/// <summary>
	/// CIP命令中的读并写的数据服务
	/// </summary>
	public const int CIP_READ_WRITE_DATA = 78;

	/// <summary>
	/// CIP命令中的读片段的数据服务
	/// </summary>
	public const int CIP_READ_FRAGMENT = 82;

	/// <summary>
	/// CIP命令中的写片段的数据服务
	/// </summary>
	public const int CIP_WRITE_FRAGMENT = 83;

	/// <summary>
	/// CIP指令中读取数据的列表
	/// </summary>
	public const byte CIP_READ_LIST = 85;

	/// <summary>
	/// CIP命令中的对数据读取服务
	/// </summary>
	public const int CIP_MULTIREAD_DATA = 4096;

	/// <summary>
	/// bool型数据，一个字节长度
	/// </summary>
	public const ushort CIP_Type_Bool = 193;

	/// <summary>
	/// byte型数据，一个字节长度，SINT
	/// </summary>
	public const ushort CIP_Type_Byte = 194;

	/// <summary>
	/// 整型，两个字节长度，INT
	/// </summary>
	public const ushort CIP_Type_Word = 195;

	/// <summary>
	/// 长整型，四个字节长度，DINT
	/// </summary>
	public const ushort CIP_Type_DWord = 196;

	/// <summary>
	/// 特长整型，8个字节，LINT
	/// </summary>
	public const ushort CIP_Type_LInt = 197;

	/// <summary>
	/// • Unsigned 8-bit integer, USINT
	/// </summary>
	public const ushort CIP_Type_USInt = 198;

	/// <summary>
	/// Unsigned 16-bit integer, UINT
	/// </summary>
	public const ushort CIP_Type_UInt = 199;

	/// <summary>
	///  Unsigned 32-bit integer, UDINT,
	/// </summary>
	public const ushort CIP_Type_UDint = 200;

	/// <summary>
	///  Unsigned 64-bit integer, ULINT,
	/// </summary>
	public const ushort CIP_Type_ULint = 201;

	/// <summary>
	/// 实数数据，四个字节长度
	/// </summary>
	public const ushort CIP_Type_Real = 202;

	/// <summary>
	/// 实数数据，八个字节的长度
	/// </summary>
	public const ushort CIP_Type_Double = 203;

	/// <summary>
	/// 结构体数据，不定长度
	/// </summary>
	public const ushort CIP_Type_Struct = 204;

	/// <summary>
	/// 字符串数据内容
	/// </summary>
	public const ushort CIP_Type_String = 208;

	/// <summary>
	///  Bit string, 8 bits, BYTE,
	/// </summary>
	public const ushort CIP_Type_D1 = 209;

	/// <summary>
	/// Bit string, 16-bits, WORD
	/// </summary>
	public const ushort CIP_Type_D2 = 210;

	/// <summary>
	/// Bit string, 32 bits, DWORD
	/// </summary>
	public const ushort CIP_Type_D3 = 211;

	/// <summary>
	/// 二进制数据内容
	/// </summary>
	public const ushort CIP_Type_BitArray = 211;

	private static byte[] BuildRequestPathCommand(string address, bool isConnectedAddress = false)
	{
		using var memoryStream = new MemoryStream();
		string[] array = address.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string text = string.Empty;
			int num = array[i].IndexOf('[');
			int num2 = array[i].IndexOf(']');
			if (num > 0 && num2 > 0 && num2 > num)
			{
				text = array[i].Substring(num + 1, num2 - num - 1);
				array[i] = array[i][..num];
			}

			memoryStream.WriteByte(145);
			byte[] bytes = Encoding.UTF8.GetBytes(array[i]);
			memoryStream.WriteByte((byte)bytes.Length);
			memoryStream.Write(bytes, 0, bytes.Length);
			if (bytes.Length % 2 == 1)
			{
				memoryStream.WriteByte(0);
			}

			if (string.IsNullOrEmpty(text))
			{
				continue;
			}

			string[] array2 = text.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			for (int j = 0; j < array2.Length; j++)
			{
				int num3 = Convert.ToInt32(array2[j]);
				if (num3 < 256 && !isConnectedAddress)
				{
					memoryStream.WriteByte(40);
					memoryStream.WriteByte((byte)num3);
					continue;
				}

				memoryStream.WriteByte(41);
				memoryStream.WriteByte(0);
				memoryStream.WriteByte(BitConverter.GetBytes(num3)[0]);
				memoryStream.WriteByte(BitConverter.GetBytes(num3)[1]);
			}
		}
		return memoryStream.ToArray();
	}

	/// <summary>
	/// 从生成的报文里面反解出实际的数据地址，不支持结构体嵌套，仅支持数据，一维数组，不支持多维数据
	/// </summary>
	/// <param name="pathCommand">地址路径报文</param>
	/// <returns>实际的地址</returns>
	public static string ParseRequestPathCommand(byte[] pathCommand)
	{
		var stringBuilder = new StringBuilder();
		for (int i = 0; i < pathCommand.Length; i++)
		{
			if (pathCommand[i] != 145)
			{
				continue;
			}

			string text = Encoding.UTF8.GetString(pathCommand, i + 2, pathCommand[i + 1]).TrimEnd(new char[1]);
			stringBuilder.Append(text);
			int num = 2 + text.Length;
			if (text.Length % 2 == 1)
			{
				num++;
			}
			if (pathCommand.Length > num + i)
			{
				if (pathCommand[i + num] == 40)
				{
					stringBuilder.Append($"[{pathCommand[i + num + 1]}]");
				}
				else if (pathCommand[i + num] == 41)
				{
					stringBuilder.Append($"[{BitConverter.ToUInt16(pathCommand, i + num + 2)}]");
				}
			}
			stringBuilder.Append('.');
		}

		if (stringBuilder[^1] == '.')
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
		}
		return stringBuilder.ToString();
	}

	/// <summary>
	/// 获取枚举PLC数据信息的指令
	/// </summary>
	/// <param name="startInstance">实例的起始地址</param>
	/// <returns>结果数据</returns>
	public static byte[] GetEnumeratorCommand(ushort startInstance)
	{
		return new byte[14]
		{
			85,
			3,
			32,
			107,
			37,
			0,
			BitConverter.GetBytes(startInstance)[0],
			BitConverter.GetBytes(startInstance)[1],
			2,
			0,
			1,
			0,
			2,
			0
		};
	}

	/// <summary>
	/// 获取获得结构体句柄的命令
	/// </summary>
	/// <param name="symbolType">包含地址的信息</param>
	/// <returns>命令数据</returns>
	public static byte[] GetStructHandleCommand(ushort symbolType)
	{
		byte[] array = new byte[18];
		byte[] bytes = BitConverter.GetBytes(symbolType);
		bytes[1] = (byte)(bytes[1] & 0xFu);
		array[0] = 3;
		array[1] = 3;
		array[2] = 32;
		array[3] = 108;
		array[4] = 37;
		array[5] = 0;
		array[6] = bytes[0];
		array[7] = bytes[1];
		array[8] = 4;
		array[9] = 0;
		array[10] = 4;
		array[11] = 0;
		array[12] = 5;
		array[13] = 0;
		array[14] = 2;
		array[15] = 0;
		array[16] = 1;
		array[17] = 0;
		return array;
	}

	/// <summary>
	/// 获取结构体内部数据结构的方法
	/// </summary>
	/// <param name="symbolType">地址</param>
	/// <param name="structHandle">句柄</param>
	/// <returns>指令</returns>
	public static byte[] GetStructItemNameType(ushort symbolType, AbStructHandle structHandle)
	{
		byte[] array = new byte[14];
		ushort value = (ushort)(structHandle.TemplateObjectDefinitionSize * 4 - 21);
		byte[] bytes = BitConverter.GetBytes(symbolType);
		bytes[1] = (byte)(bytes[1] & 0xFu);
		byte[] bytes2 = BitConverter.GetBytes(0);
		byte[] bytes3 = BitConverter.GetBytes(value);
		array[0] = 76;
		array[1] = 3;
		array[2] = 32;
		array[3] = 108;
		array[4] = 37;
		array[5] = 0;
		array[6] = bytes[0];
		array[7] = bytes[1];
		array[8] = bytes2[0];
		array[9] = bytes2[1];
		array[10] = bytes2[2];
		array[11] = bytes2[3];
		array[12] = bytes3[0];
		array[13] = bytes3[1];
		return array;
	}

	/// <summary>
	/// 将CommandSpecificData的命令，打包成可发送的数据指令
	/// </summary>
	/// <param name="command">实际的命令暗号</param>
	/// <param name="session">当前会话的id</param>
	/// <param name="commandSpecificData">CommandSpecificData命令</param>
	/// <returns>最终可发送的数据命令</returns>
	public static byte[] PackRequestHeader(ushort command, uint session, byte[] commandSpecificData)
	{
		byte[] array = new byte[commandSpecificData.Length + 24];
		Array.Copy(commandSpecificData, 0, array, 24, commandSpecificData.Length);
		BitConverter.GetBytes(command).CopyTo(array, 0);
		BitConverter.GetBytes(session).CopyTo(array, 4);
		BitConverter.GetBytes((ushort)commandSpecificData.Length).CopyTo(array, 2);
		return array;
	}

	/// <summary>
	/// 打包生成一个请求读取数据的节点信息，CIP指令信息
	/// </summary>
	/// <param name="address">地址</param>
	/// <param name="length">指代数组的长度</param>
	/// <param name="isConnectedAddress">是否是连接模式下的地址，默认为false</param>
	/// <returns>CIP的指令信息</returns>
	public static byte[] PackRequsetRead(string address, int length, bool isConnectedAddress = false)
	{
		byte[] array = new byte[1024];
		int num = 0;
		array[num++] = 76;
		num++;
		byte[] array2 = BuildRequestPathCommand(address, isConnectedAddress);
		array2.CopyTo(array, num);
		num += array2.Length;
		array[1] = (byte)((num - 2) / 2);
		array[num++] = BitConverter.GetBytes(length)[0];
		array[num++] = BitConverter.GetBytes(length)[1];
		byte[] array3 = new byte[num];
		Array.Copy(array, 0, array3, 0, num);
		return array3;
	}

	/// <summary>
	/// 打包生成一个请求读取数据片段的节点信息，CIP指令信息
	/// </summary>
	/// <param name="address">节点的名称 -&gt; Tag Name</param>
	/// <param name="startIndex">起始的索引位置，以字节为单位 -&gt; The initial index position, in bytes</param>
	/// <param name="length">读取的数据长度，一次通讯总计490个字节 -&gt; Length of read data, a total of 490 bytes of communication</param>
	/// <returns>CIP的指令信息</returns>
	public static byte[] PackRequestReadSegment(string address, int startIndex, int length)
	{
		byte[] array = new byte[1024];
		int num = 0;
		array[num++] = 82;
		num++;
		byte[] array2 = BuildRequestPathCommand(address);
		array2.CopyTo(array, num);
		num += array2.Length;
		array[1] = (byte)((num - 2) / 2);
		array[num++] = BitConverter.GetBytes(length)[0];
		array[num++] = BitConverter.GetBytes(length)[1];
		array[num++] = BitConverter.GetBytes(startIndex)[0];
		array[num++] = BitConverter.GetBytes(startIndex)[1];
		array[num++] = BitConverter.GetBytes(startIndex)[2];
		array[num++] = BitConverter.GetBytes(startIndex)[3];
		byte[] array3 = new byte[num];
		Array.Copy(array, 0, array3, 0, num);
		return array3;
	}

	/// <summary>
	/// 根据指定的数据和类型，生成对应的数据
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="typeCode">数据类型</param>
	/// <param name="value">字节值</param>
	/// <param name="length">如果节点为数组，就是数组长度</param>
	/// <param name="isConnectedAddress">是否为连接模式的地址</param>
	/// <returns>CIP的指令信息</returns>
	public static byte[] PackRequestWrite(string address, ushort typeCode, byte[] value, int length = 1, bool isConnectedAddress = false)
	{
		byte[] array = new byte[1024];
		int num = 0;
		array[num++] = 77;
		num++;
		byte[] array2 = BuildRequestPathCommand(address, isConnectedAddress);
		array2.CopyTo(array, num);
		num += array2.Length;
		array[1] = (byte)((num - 2) / 2);
		array[num++] = BitConverter.GetBytes(typeCode)[0];
		array[num++] = BitConverter.GetBytes(typeCode)[1];
		array[num++] = BitConverter.GetBytes(length)[0];
		array[num++] = BitConverter.GetBytes(length)[1];
		value.CopyTo(array, num);
		num += value.Length;
		byte[] array3 = new byte[num];
		Array.Copy(array, 0, array3, 0, num);
		return array3;
	}

	/// <summary>
	/// 分析地址数据信息里的位索引的信息，例如a[10]  返回 a 和 10 索引，如果没有指定索引，就默认为0
	/// </summary>
	/// <param name="address">数据地址</param>
	/// <param name="arrayIndex">位索引</param>
	/// <returns>地址信息</returns>
	public static string AnalysisArrayIndex(string address, out int arrayIndex)
	{
		arrayIndex = 0;
		if (!address.EndsWith("]"))
		{
			return address;
		}

		int num = address.LastIndexOf('[');
		if (num < 0)
		{
			return address;
		}

		address = address.Remove(address.Length - 1);
		arrayIndex = int.Parse(address[(num + 1)..]);
		address = address[..num];
		return address;
	}

	/// <summary>
	/// 写入Bool数据的基本指令信息
	/// </summary>
	/// <param name="address">地址</param>
	/// <param name="value">值</param>
	/// <returns>报文信息</returns>
	public static byte[] PackRequestWrite(string address, bool value)
	{
		address = AnalysisArrayIndex(address, out var arrayIndex);
		address = address + "[" + arrayIndex / 32 + "]";
		int value2 = 0;
		int value3 = -1;
		if (value)
		{
			value2 = 1 << arrayIndex;
		}
		else
		{
			value3 = ~(1 << arrayIndex);
		}

		byte[] array = new byte[1024];
		int num = 0;
		array[num++] = 78;
		num++;
		byte[] array2 = BuildRequestPathCommand(address);
		array2.CopyTo(array, num);
		num += array2.Length;
		array[1] = (byte)((num - 2) / 2);
		array[num++] = 4;
		array[num++] = 0;
		BitConverter.GetBytes(value2).CopyTo(array, num);
		num += 4;
		BitConverter.GetBytes(value3).CopyTo(array, num);
		num += 4;
		byte[] array3 = new byte[num];
		Array.Copy(array, 0, array3, 0, num);
		return array3;
	}

	/// <summary>
	/// 将所有的cip指定进行打包操作。
	/// </summary>
	/// <param name="portSlot">PLC所在的面板槽号</param>
	/// <param name="cips">所有的cip打包指令信息</param>
	/// <returns>包含服务的信息</returns>
	public static byte[] PackCommandService(byte[] portSlot, params byte[][] cips)
	{
		using var memoryStream = new MemoryStream();
		memoryStream.WriteByte(178);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(82);
		memoryStream.WriteByte(2);
		memoryStream.WriteByte(32);
		memoryStream.WriteByte(6);
		memoryStream.WriteByte(36);
		memoryStream.WriteByte(1);
		memoryStream.WriteByte(10);
		memoryStream.WriteByte(240);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);

		int num = 0;
		if (cips.Length == 1)
		{
			memoryStream.Write(cips[0], 0, cips[0].Length);
			num += cips[0].Length;
		}
		else
		{
			memoryStream.WriteByte(10);
			memoryStream.WriteByte(2);
			memoryStream.WriteByte(32);
			memoryStream.WriteByte(2);
			memoryStream.WriteByte(36);
			memoryStream.WriteByte(1);
			num += 8;
			memoryStream.Write(BitConverter.GetBytes((ushort)cips.Length), 0, 2);
			ushort num2 = (ushort)(2 + 2 * cips.Length);
			num += 2 * cips.Length;
			for (int i = 0; i < cips.Length; i++)
			{
				memoryStream.Write(BitConverter.GetBytes(num2), 0, 2);
				num2 = (ushort)(num2 + cips[i].Length);
			}
			for (int j = 0; j < cips.Length; j++)
			{
				memoryStream.Write(cips[j], 0, cips[j].Length);
				num += cips[j].Length;
			}
		}

		memoryStream.WriteByte((byte)((portSlot.Length + 1) / 2));
		memoryStream.WriteByte(0);
		memoryStream.Write(portSlot, 0, portSlot.Length);

		if (portSlot.Length % 2 == 1)
		{
			memoryStream.WriteByte(0);
		}

		byte[] array = memoryStream.ToArray();
		BitConverter.GetBytes((short)num).CopyTo(array, 12);
		BitConverter.GetBytes((short)(array.Length - 4)).CopyTo(array, 2);
		return array;
	}

	/// <summary>
	/// 将所有的cip指定进行打包操作。
	/// </summary>
	/// <param name="portSlot">PLC所在的面板槽号</param>
	/// <param name="cips">所有的cip打包指令信息</param>
	/// <returns>包含服务的信息</returns>
	public static byte[] PackCleanCommandService(byte[] portSlot, params byte[][] cips)
	{
		using var memoryStream = new MemoryStream();
		memoryStream.WriteByte(178);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);

		if (cips.Length == 1)
		{
			memoryStream.Write(cips[0], 0, cips[0].Length);
		}
		else
		{
			memoryStream.WriteByte(10);
			memoryStream.WriteByte(2);
			memoryStream.WriteByte(32);
			memoryStream.WriteByte(2);
			memoryStream.WriteByte(36);
			memoryStream.WriteByte(1);
			memoryStream.Write(BitConverter.GetBytes((ushort)cips.Length), 0, 2);
			ushort num = (ushort)(2 + 2 * cips.Length);
			for (int i = 0; i < cips.Length; i++)
			{
				memoryStream.Write(BitConverter.GetBytes(num), 0, 2);
				num = (ushort)(num + cips[i].Length);
			}
			for (int j = 0; j < cips.Length; j++)
			{
				memoryStream.Write(cips[j], 0, cips[j].Length);
			}
		}

		memoryStream.WriteByte((byte)((portSlot.Length + 1) / 2));
		memoryStream.WriteByte(0);
		memoryStream.Write(portSlot, 0, portSlot.Length);
		if (portSlot.Length % 2 == 1)
		{
			memoryStream.WriteByte(0);
		}

		byte[] array = memoryStream.ToArray();
		BitConverter.GetBytes((short)(array.Length - 4)).CopyTo(array, 2);
		return array;
	}

	/// <summary>
	/// 打包一个读取所有特性数据的报文信息，需要传入slot
	/// </summary>
	/// <param name="portSlot">站号信息</param>
	/// <param name="sessionHandle">会话的ID信息</param>
	/// <returns>最终发送的报文数据</returns>
	public static byte[] PackCommandGetAttributesAll(byte[] portSlot, uint sessionHandle)
	{
		byte[] commandSpecificData = PackCommandSpecificData(new byte[4], PackCommandService(portSlot, new byte[6] { 1, 2, 32, 1, 36, 1 }));
		return PackRequestHeader(111, sessionHandle, commandSpecificData);
	}

	/// <summary>
	/// 根据数据创建反馈的数据信息
	/// </summary>
	/// <param name="data">反馈的数据信息</param>
	/// <param name="isRead">是否是读取</param>
	/// <returns>数据</returns>
	public static byte[] PackCommandResponse(byte[] data, bool isRead)
	{
		if (data == null)
		{
			return new byte[6] { 0, 0, 4, 0, 0, 0 };
		}
		return SoftBasic.SpliceArray(new byte[6]
		{
			(byte)(isRead ? 204u : 205u),
			0,
			0,
			0,
			0,
			0
		}, data);
	}

	/// <summary>
	/// 生成读取直接节点数据信息的内容
	/// </summary>
	/// <param name="service">cip指令内容</param>
	/// <returns>最终的指令值</returns>
	public static byte[] PackCommandSpecificData(params byte[][] service)
	{
		using var memoryStream = new MemoryStream();
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(1);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(BitConverter.GetBytes(service.Length)[0]);
		memoryStream.WriteByte(BitConverter.GetBytes(service.Length)[1]);

		for (int i = 0; i < service.Length; i++)
		{
			memoryStream.Write(service[i], 0, service[i].Length);
		}

		byte[] result = memoryStream.ToArray();
		return result;
	}

	/// <summary>
	/// 将所有的cip指定进行打包操作。
	/// </summary>
	/// <param name="command">指令信息</param>
	/// <returns>包含服务的信息</returns>
	public static byte[] PackCommandSingleService(byte[] command)
	{
		command ??= Array.Empty<byte>();

		byte[] array = new byte[4 + command.Length];
		array[0] = 178;
		array[1] = 0;
		array[2] = BitConverter.GetBytes(command.Length)[0];
		array[3] = BitConverter.GetBytes(command.Length)[1];
		command.CopyTo(array, 4);
		return array;
	}

	/// <summary>
	/// 向PLC注册会话ID的报文。
	/// </summary>
	/// <returns>报文信息</returns>
	public static byte[] RegisterSessionHandle()
	{
		byte[] commandSpecificData = new byte[4] { 1, 0, 0, 0 };
		return PackRequestHeader(101, 0u, commandSpecificData);
	}

	/// <summary>
	/// 获取卸载一个已注册的会话的报文。
	/// </summary>
	/// <param name="sessionHandle">当前会话的ID信息</param>
	/// <returns>字节报文信息</returns>
	public static byte[] UnRegisterSessionHandle(uint sessionHandle)
	{
		return PackRequestHeader(102, sessionHandle, Array.Empty<byte>());
	}

	/// <summary>
	/// 初步检查返回的CIP协议的报文是否正确。
	/// </summary>
	/// <param name="response">CIP的报文信息</param>
	/// <returns>是否正确的结果信息</returns>
	public static OperateResult CheckResponse(byte[] response)
	{
		try
		{
			int num = BitConverter.ToInt32(response, 8);
			if (num == 0)
			{
				return OperateResult.Ok();
			}

			string empty = string.Empty;
			return new OperateResult(num, num switch
			{
				1 => OpsErrorCode.AllenBradleySessionStatus01.Desc(),
				2 => OpsErrorCode.AllenBradleySessionStatus02.Desc(),
				3 => OpsErrorCode.AllenBradleySessionStatus03.Desc(),
				100 => OpsErrorCode.AllenBradleySessionStatus64.Desc(),
				101 => OpsErrorCode.AllenBradleySessionStatus65.Desc(),
				105 => OpsErrorCode.AllenBradleySessionStatus69.Desc(),
				_ => OpsErrorCode.UnknownError.Desc(),
			});
		}
		catch (Exception ex)
		{
			return new OperateResult(ex.Message);
		}
	}

	/// <summary>
	/// 从PLC反馈的数据解析
	/// </summary>
	/// <param name="response">PLC的反馈数据</param>
	/// <param name="isRead">是否是返回的操作</param>
	/// <returns>带有结果标识的最终数据</returns>
	public static OperateResult<byte[], ushort, bool> ExtractActualData(byte[] response, bool isRead)
	{
		var list = new List<byte>();
		int num = 38;
		bool value = false;
		ushort value2 = 0;
		ushort num2 = BitConverter.ToUInt16(response, 38);
		if (BitConverter.ToInt32(response, 40) == 138)
		{
			num = 44;
			int num3 = BitConverter.ToUInt16(response, num);
			for (int i = 0; i < num3; i++)
			{
				int num4 = BitConverter.ToUInt16(response, num + 2 + i * 2) + num;
				int num5 = ((i == num3 - 1) ? response.Length : (BitConverter.ToUInt16(response, num + 4 + i * 2) + num));
				ushort num6 = BitConverter.ToUInt16(response, num4 + 2);
				switch (num6)
				{
					case 4:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = OpsErrorCode.AllenBradley04.Desc(),
						};
					case 5:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = OpsErrorCode.AllenBradley05.Desc(),
						};
					case 6:
						if (response[num + 2] == 210 || response[num + 2] == 204)
						{
							return new OperateResult<byte[], ushort, bool>
							{
								ErrorCode = num6,
								Message = OpsErrorCode.AllenBradley06.Desc(),
							};
						}
						break;
					case 10:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = OpsErrorCode.AllenBradley0A.Desc(),
						};
					case 19:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = OpsErrorCode.AllenBradley13.Desc(),
						};
					case 28:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = OpsErrorCode.AllenBradley1C.Desc(),
						};
					case 30:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = OpsErrorCode.AllenBradley1E.Desc(),
						};
					case 38:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = OpsErrorCode.AllenBradley26.Desc(),
						};
					default:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = OpsErrorCode.UnknownError.Desc(),
						};
					case 0:
						break;
				}

				if (isRead)
				{
					for (int j = num4 + 6; j < num5; j++)
					{
						list.Add(response[j]);
					}
				}
			}
		}
		else
		{
			byte b = response[num + 4];
			switch (b)
			{
				case 4:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = OpsErrorCode.AllenBradley04.Desc(),
					};
				case 5:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = OpsErrorCode.AllenBradley05.Desc(),
					};
				case 6:
					value = true;
					break;
				case 10:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = OpsErrorCode.AllenBradley0A.Desc(),
					};
				case 19:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = OpsErrorCode.AllenBradley13.Desc(),
					};
				case 28:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = OpsErrorCode.AllenBradley1C.Desc(),
					};
				case 30:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = OpsErrorCode.AllenBradley1E.Desc(),
					};
				case 32:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = OpsErrorCode.AllenBradley20.Desc(),
					};
				case 38:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = OpsErrorCode.AllenBradley26.Desc(),
					};
				default:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = OpsErrorCode.UnknownError.Desc(),
					};
				case 0:
					break;
			}

			if (response[num + 2] == 205 || response[num + 2] == 211)
			{
				return OperateResult.Ok(list.ToArray(), value2, value);
			}

			if (response[num + 2] == 204 || response[num + 2] == 210)
			{
				for (int k = num + 8; k < num + 2 + num2; k++)
				{
					list.Add(response[k]);
				}
				value2 = BitConverter.ToUInt16(response, num + 6);
			}
			else if (response[num + 2] == 213)
			{
				for (int l = num + 6; l < num + 2 + num2; l++)
				{
					list.Add(response[l]);
				}
			}
		}

		return OperateResult.Ok(list.ToArray(), value2, value);
	}
}
