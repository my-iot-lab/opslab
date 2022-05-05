using System.Text;
using Ops.Communication.Address;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 所有三菱通讯类的通用辅助工具类，包含了一些通用的静态方法，可以使用本类来获取一些原始的报文信息。详细的操作参见例子。
/// </summary>
public class MelsecHelper
{
	/// <summary>
	/// 解析A1E协议数据地址。
	/// </summary>
	/// <param name="address">数据地址</param>
	/// <returns>结果对象</returns>
	public static OperateResult<MelsecA1EDataType, int> McA1EAnalysisAddress(string address)
	{
		var operateResult = new OperateResult<MelsecA1EDataType, int>();
		try
		{
			switch (address[0])
			{
				case 'T' or 't':
					if (address[1] == 'S' || address[1] == 's')
					{
						operateResult.Content1 = MelsecA1EDataType.TS;
						operateResult.Content2 = Convert.ToInt32(address[2..], MelsecA1EDataType.TS.FromBase);
						break;
					}
					if (address[1] == 'C' || address[1] == 'c')
					{
						operateResult.Content1 = MelsecA1EDataType.TC;
						operateResult.Content2 = Convert.ToInt32(address[2..], MelsecA1EDataType.TC.FromBase);
						break;
					}
					if (address[1] == 'N' || address[1] == 'n')
					{
						operateResult.Content1 = MelsecA1EDataType.TN;
						operateResult.Content2 = Convert.ToInt32(address[2..], MelsecA1EDataType.TN.FromBase);
						break;
					}
					throw new Exception(ErrorCode.NotSupportedDataType.Desc());
				case 'C' or 'c':
					if (address[1] == 'S' || address[1] == 's')
					{
						operateResult.Content1 = MelsecA1EDataType.CS;
						operateResult.Content2 = Convert.ToInt32(address[2..], MelsecA1EDataType.CS.FromBase);
						break;
					}
					if (address[1] == 'C' || address[1] == 'c')
					{
						operateResult.Content1 = MelsecA1EDataType.CC;
						operateResult.Content2 = Convert.ToInt32(address[2..], MelsecA1EDataType.CC.FromBase);
						break;
					}
					if (address[1] == 'N' || address[1] == 'n')
					{
						operateResult.Content1 = MelsecA1EDataType.CN;
						operateResult.Content2 = Convert.ToInt32(address[2..], MelsecA1EDataType.CN.FromBase);
						break;
					}
					throw new Exception(ErrorCode.NotSupportedDataType.Desc());
				case 'X' or 'x':
					operateResult.Content1 = MelsecA1EDataType.X;
					address = address[1..];
					if (address.StartsWith("0"))
					{
						operateResult.Content2 = Convert.ToInt32(address, 8);
					}
					else
					{
						operateResult.Content2 = Convert.ToInt32(address, MelsecA1EDataType.X.FromBase);
					}
					break;
				case 'Y' or 'y':
					operateResult.Content1 = MelsecA1EDataType.Y;
					address = address[1..];
					if (address.StartsWith("0"))
					{
						operateResult.Content2 = Convert.ToInt32(address, 8);
					}
					else
					{
						operateResult.Content2 = Convert.ToInt32(address, MelsecA1EDataType.Y.FromBase);
					}
					break;
				case 'M' or 'm':
					operateResult.Content1 = MelsecA1EDataType.M;
					operateResult.Content2 = Convert.ToInt32(address[1..], MelsecA1EDataType.M.FromBase);
					break;
				case 'S' or 's':
					operateResult.Content1 = MelsecA1EDataType.S;
					operateResult.Content2 = Convert.ToInt32(address[1..], MelsecA1EDataType.S.FromBase);
					break;
				case 'F' or 'f':
					operateResult.Content1 = MelsecA1EDataType.F;
					operateResult.Content2 = Convert.ToInt32(address[1..], MelsecA1EDataType.F.FromBase);
					break;
				case 'B' or 'b':
					operateResult.Content1 = MelsecA1EDataType.B;
					operateResult.Content2 = Convert.ToInt32(address[1..], MelsecA1EDataType.B.FromBase);
					break;
				case 'D' or 'd':
					operateResult.Content1 = MelsecA1EDataType.D;
					operateResult.Content2 = Convert.ToInt32(address[1..], MelsecA1EDataType.D.FromBase);
					break;
				case 'R' or 'r':
					operateResult.Content1 = MelsecA1EDataType.R;
					operateResult.Content2 = Convert.ToInt32(address[1..], MelsecA1EDataType.R.FromBase);
					break;
				case 'W' or 'w':
					operateResult.Content1 = MelsecA1EDataType.W;
					operateResult.Content2 = Convert.ToInt32(address[1..], MelsecA1EDataType.W.FromBase);
					break;
				default:
					throw new Exception(ErrorCode.NotSupportedDataType.Desc());
			}
		}
		catch (Exception ex)
		{
			operateResult.Message = ex.Message;
			return operateResult;
		}

		operateResult.IsSuccess = true;
		return operateResult;
	}

	/// <summary>
	/// 从三菱地址，是否位读取进行创建读取的MC的核心报文<br />
	/// From the Mitsubishi address, whether to read the core message of the MC for creating and reading
	/// </summary>
	/// <param name="isBit">是否进行了位读取操作</param>
	/// <param name="addressData">三菱Mc协议的数据地址</param>
	/// <returns>带有成功标识的报文对象</returns>
	public static byte[] BuildReadMcCoreCommand(McAddressData addressData, bool isBit)
	{
		return new byte[10]
		{
			1,
			4,
			(byte)(isBit ? 1 : 0),
			0,
			BitConverter.GetBytes(addressData.AddressStart)[0],
			BitConverter.GetBytes(addressData.AddressStart)[1],
			BitConverter.GetBytes(addressData.AddressStart)[2],
			addressData.McDataType.DataCode,
			(byte)((int)addressData.Length % 256),
			(byte)((int)addressData.Length / 256)
		};
	}

	/// <summary>
	/// 从三菱地址，是否位读取进行创建读取Ascii格式的MC的核心报文
	/// </summary>
	/// <param name="addressData">三菱Mc协议的数据地址</param>
	/// <param name="isBit">是否进行了位读取操作</param>
	/// <returns>带有成功标识的报文对象</returns>
	public static byte[] BuildAsciiReadMcCoreCommand(McAddressData addressData, bool isBit)
	{
		return new byte[20]
		{
			48,
			52,
			48,
			49,
			48,
			48,
			48,
			(byte)(isBit ? 49 : 48),
			Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0],
			Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1],
			BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0],
			BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1],
			BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2],
			BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3],
			BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4],
			BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5],
			SoftBasic.BuildAsciiBytesFrom(addressData.Length)[0],
			SoftBasic.BuildAsciiBytesFrom(addressData.Length)[1],
			SoftBasic.BuildAsciiBytesFrom(addressData.Length)[2],
			SoftBasic.BuildAsciiBytesFrom(addressData.Length)[3]
		};
	}

	/// <summary>
	/// 以字为单位，创建数据写入的核心报文
	/// </summary>
	/// <param name="addressData">三菱Mc协议的数据地址</param>
	/// <param name="value">实际的原始数据信息</param>
	/// <returns>带有成功标识的报文对象</returns>
	public static byte[] BuildWriteWordCoreCommand(McAddressData addressData, byte[] value)
	{
		if (value == null)
		{
			value = new byte[0];
		}
		byte[] array = new byte[10 + value.Length];
		array[0] = 1;
		array[1] = 20;
		array[2] = 0;
		array[3] = 0;
		array[4] = BitConverter.GetBytes(addressData.AddressStart)[0];
		array[5] = BitConverter.GetBytes(addressData.AddressStart)[1];
		array[6] = BitConverter.GetBytes(addressData.AddressStart)[2];
		array[7] = addressData.McDataType.DataCode;
		array[8] = (byte)(value.Length / 2 % 256);
		array[9] = (byte)(value.Length / 2 / 256);
		value.CopyTo(array, 10);
		return array;
	}

	/// <summary>
	/// 以字为单位，创建ASCII数据写入的核心报文
	/// </summary>
	/// <param name="addressData">三菱Mc协议的数据地址</param>
	/// <param name="value">实际的原始数据信息</param>
	/// <returns>带有成功标识的报文对象</returns>
	public static byte[] BuildAsciiWriteWordCoreCommand(McAddressData addressData, byte[] value)
	{
		value = TransByteArrayToAsciiByteArray(value);
		byte[] array = new byte[20 + value.Length];
		array[0] = 49;
		array[1] = 52;
		array[2] = 48;
		array[3] = 49;
		array[4] = 48;
		array[5] = 48;
		array[6] = 48;
		array[7] = 48;
		array[8] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0];
		array[9] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1];
		array[10] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0];
		array[11] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1];
		array[12] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2];
		array[13] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3];
		array[14] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4];
		array[15] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5];
		array[16] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[0];
		array[17] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[1];
		array[18] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[2];
		array[19] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[3];
		value.CopyTo(array, 20);
		return array;
	}

	/// <summary>
	/// 以位为单位，创建数据写入的核心报文
	/// </summary>
	/// <param name="addressData">三菱Mc协议的数据地址</param>
	/// <param name="value">原始的bool数组数据</param>
	/// <returns>带有成功标识的报文对象</returns>
	public static byte[] BuildWriteBitCoreCommand(McAddressData addressData, bool[] value)
	{
		if (value == null)
		{
			value = new bool[0];
		}
		byte[] array = TransBoolArrayToByteData(value);
		byte[] array2 = new byte[10 + array.Length];
		array2[0] = 1;
		array2[1] = 20;
		array2[2] = 1;
		array2[3] = 0;
		array2[4] = BitConverter.GetBytes(addressData.AddressStart)[0];
		array2[5] = BitConverter.GetBytes(addressData.AddressStart)[1];
		array2[6] = BitConverter.GetBytes(addressData.AddressStart)[2];
		array2[7] = addressData.McDataType.DataCode;
		array2[8] = (byte)(value.Length % 256);
		array2[9] = (byte)(value.Length / 256);
		array.CopyTo(array2, 10);
		return array2;
	}

	/// <summary>
	/// 以位为单位，创建ASCII数据写入的核心报文
	/// </summary>
	/// <param name="addressData">三菱Mc协议的数据地址</param>
	/// <param name="value">原始的bool数组数据</param>
	/// <returns>带有成功标识的报文对象</returns>
	public static byte[] BuildAsciiWriteBitCoreCommand(McAddressData addressData, bool[] value)
	{
		if (value == null)
		{
			value = new bool[0];
		}
		byte[] array = value.Select((bool m) => (byte)(m ? 49 : 48)).ToArray();
		byte[] array2 = new byte[20 + array.Length];
		array2[0] = 49;
		array2[1] = 52;
		array2[2] = 48;
		array2[3] = 49;
		array2[4] = 48;
		array2[5] = 48;
		array2[6] = 48;
		array2[7] = 49;
		array2[8] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0];
		array2[9] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1];
		array2[10] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0];
		array2[11] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1];
		array2[12] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2];
		array2[13] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3];
		array2[14] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4];
		array2[15] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5];
		array2[16] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[0];
		array2[17] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[1];
		array2[18] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[2];
		array2[19] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[3];
		array.CopyTo(array2, 20);
		return array2;
	}

	/// <summary>
	/// 从三菱扩展地址，是否位读取进行创建读取的MC的核心报文
	/// </summary>
	/// <param name="isBit">是否进行了位读取操作</param>
	/// <param name="extend">扩展指定</param>
	/// <param name="addressData">三菱Mc协议的数据地址</param>
	/// <returns>带有成功标识的报文对象</returns>
	public static byte[] BuildReadMcCoreExtendCommand(McAddressData addressData, ushort extend, bool isBit)
	{
		return new byte[17]
		{
			1,
			4,
			(byte)(isBit ? 129 : 128),
			0,
			0,
			0,
			BitConverter.GetBytes(addressData.AddressStart)[0],
			BitConverter.GetBytes(addressData.AddressStart)[1],
			BitConverter.GetBytes(addressData.AddressStart)[2],
			addressData.McDataType.DataCode,
			0,
			0,
			BitConverter.GetBytes(extend)[0],
			BitConverter.GetBytes(extend)[1],
			249,
			(byte)((int)addressData.Length % 256),
			(byte)((int)addressData.Length / 256)
		};
	}

	/// <summary>
	/// 按字为单位随机读取的指令创建
	/// </summary>
	/// <param name="address">地址数组</param>
	/// <returns>指令</returns>
	public static byte[] BuildReadRandomWordCommand(McAddressData[] address)
	{
		byte[] array = new byte[6 + address.Length * 4];
		array[0] = 3;
		array[1] = 4;
		array[2] = 0;
		array[3] = 0;
		array[4] = (byte)address.Length;
		array[5] = 0;
		for (int i = 0; i < address.Length; i++)
		{
			array[i * 4 + 6] = BitConverter.GetBytes(address[i].AddressStart)[0];
			array[i * 4 + 7] = BitConverter.GetBytes(address[i].AddressStart)[1];
			array[i * 4 + 8] = BitConverter.GetBytes(address[i].AddressStart)[2];
			array[i * 4 + 9] = address[i].McDataType.DataCode;
		}
		return array;
	}

	/// <summary>
	/// 随机读取的指令创建
	/// </summary>
	/// <param name="address">地址数组</param>
	/// <returns>指令</returns>
	public static byte[] BuildReadRandomCommand(McAddressData[] address)
	{
		byte[] array = new byte[6 + address.Length * 6];
		array[0] = 6;
		array[1] = 4;
		array[2] = 0;
		array[3] = 0;
		array[4] = (byte)address.Length;
		array[5] = 0;
		for (int i = 0; i < address.Length; i++)
		{
			array[i * 6 + 6] = BitConverter.GetBytes(address[i].AddressStart)[0];
			array[i * 6 + 7] = BitConverter.GetBytes(address[i].AddressStart)[1];
			array[i * 6 + 8] = BitConverter.GetBytes(address[i].AddressStart)[2];
			array[i * 6 + 9] = address[i].McDataType.DataCode;
			array[i * 6 + 10] = (byte)((int)address[i].Length % 256);
			array[i * 6 + 11] = (byte)((int)address[i].Length / 256);
		}
		return array;
	}

	/// <summary>
	/// 按字为单位随机读取的指令创建
	/// </summary>
	/// <param name="address">地址数组</param>
	/// <returns>指令</returns>
	public static byte[] BuildAsciiReadRandomWordCommand(McAddressData[] address)
	{
		byte[] array = new byte[12 + address.Length * 8];
		array[0] = 48;
		array[1] = 52;
		array[2] = 48;
		array[3] = 51;
		array[4] = 48;
		array[5] = 48;
		array[6] = 48;
		array[7] = 48;
		array[8] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[0];
		array[9] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[1];
		array[10] = 48;
		array[11] = 48;
		for (int i = 0; i < address.Length; i++)
		{
			array[i * 8 + 12] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[0];
			array[i * 8 + 13] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[1];
			array[i * 8 + 14] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[0];
			array[i * 8 + 15] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[1];
			array[i * 8 + 16] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[2];
			array[i * 8 + 17] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[3];
			array[i * 8 + 18] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[4];
			array[i * 8 + 19] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[5];
		}
		return array;
	}

	/// <summary>
	/// 随机读取的指令创建
	/// </summary>
	/// <param name="address">地址数组</param>
	/// <returns>指令</returns>
	public static byte[] BuildAsciiReadRandomCommand(McAddressData[] address)
	{
		byte[] array = new byte[12 + address.Length * 12];
		array[0] = 48;
		array[1] = 52;
		array[2] = 48;
		array[3] = 54;
		array[4] = 48;
		array[5] = 48;
		array[6] = 48;
		array[7] = 48;
		array[8] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[0];
		array[9] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[1];
		array[10] = 48;
		array[11] = 48;
		for (int i = 0; i < address.Length; i++)
		{
			array[i * 12 + 12] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[0];
			array[i * 12 + 13] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[1];
			array[i * 12 + 14] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[0];
			array[i * 12 + 15] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[1];
			array[i * 12 + 16] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[2];
			array[i * 12 + 17] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[3];
			array[i * 12 + 18] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[4];
			array[i * 12 + 19] = BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[5];
			array[i * 12 + 20] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[0];
			array[i * 12 + 21] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[1];
			array[i * 12 + 22] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[2];
			array[i * 12 + 23] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[3];
		}
		return array;
	}

	/// <summary>
	/// 创建批量读取标签的报文数据信息
	/// </summary>
	/// <param name="tags">标签名</param>
	/// <param name="lengths">长度信息</param>
	/// <returns>报文名称</returns>
	public static byte[] BuildReadTag(string[] tags, ushort[] lengths)
	{
		if (tags.Length != lengths.Length)
		{
			throw new Exception(ErrorCode.TwoParametersLengthIsNotSame.Desc());
		}

		using var memoryStream = new MemoryStream();
		memoryStream.WriteByte(26);
		memoryStream.WriteByte(4);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(BitConverter.GetBytes(tags.Length)[0]);
		memoryStream.WriteByte(BitConverter.GetBytes(tags.Length)[1]);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		for (int i = 0; i < tags.Length; i++)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(tags[i]);
			memoryStream.WriteByte(BitConverter.GetBytes(bytes.Length / 2)[0]);
			memoryStream.WriteByte(BitConverter.GetBytes(bytes.Length / 2)[1]);
			memoryStream.Write(bytes, 0, bytes.Length);
			memoryStream.WriteByte(1);
			memoryStream.WriteByte(0);
			memoryStream.WriteByte(BitConverter.GetBytes(lengths[i] * 2)[0]);
			memoryStream.WriteByte(BitConverter.GetBytes(lengths[i] * 2)[1]);
		}

		byte[] result = memoryStream.ToArray();
		return result;
	}

	/// <summary>
	/// 解析出标签读取的数据内容
	/// </summary>
	/// <param name="content">返回的数据信息</param>
	/// <returns>解析结果</returns>
	public static OperateResult<byte[]> ExtraTagData(byte[] content)
	{
		try
		{
			int num = BitConverter.ToUInt16(content, 0);
			int num2 = 2;
			var list = new List<byte>(20);
			for (int i = 0; i < num; i++)
			{
				int num3 = BitConverter.ToUInt16(content, num2 + 2);
				list.AddRange(SoftBasic.ArraySelectMiddle(content, num2 + 4, num3));
				num2 += 4 + num3;
			}
			return OperateResult.Ok(list.ToArray());
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>($"{ex.Message} Source: {SoftBasic.ByteToHexString(content, ' ')}");
		}
	}

	/// <summary>
	/// 读取本站缓冲寄存器的数据信息，需要指定寄存器的地址，和读取的长度
	/// </summary>
	/// <param name="address">寄存器的地址</param>
	/// <param name="length">数据长度</param>
	/// <returns>结果内容</returns>
	public static OperateResult<byte[]> BuildReadMemoryCommand(string address, ushort length)
	{
		try
		{
			uint value = uint.Parse(address);
			return OperateResult.Ok(new byte[10]
			{
				19,
				6,
				0,
				0,
				BitConverter.GetBytes(value)[0],
				BitConverter.GetBytes(value)[1],
				BitConverter.GetBytes(value)[2],
				BitConverter.GetBytes(value)[3],
				(byte)((int)length % 256),
				(byte)((int)length / 256)
			});
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	public static OperateResult<byte[]> BuildAsciiReadMemoryCommand(string address, ushort length)
	{
		try
		{
			uint value = uint.Parse(address);
			byte[] array = new byte[20]
			{
				48, 54, 49, 51, 48, 48, 48, 48, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			};
			SoftBasic.BuildAsciiBytesFrom(value).CopyTo(array, 8);
			SoftBasic.BuildAsciiBytesFrom(length).CopyTo(array, 16);
			return OperateResult.Ok(array);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 构建读取智能模块的命令，需要指定模块编号，起始地址，读取的长度，注意，该长度以字节为单位。
	/// </summary>
	/// <param name="module">模块编号</param>
	/// <param name="address">智能模块的起始地址</param>
	/// <param name="length">读取的字长度</param>
	/// <returns>报文的结果内容</returns>
	public static OperateResult<byte[]> BuildReadSmartModule(ushort module, string address, ushort length)
	{
		try
		{
			uint value = uint.Parse(address);
			return OperateResult.Ok(new byte[12]
			{
				1,
				6,
				0,
				0,
				BitConverter.GetBytes(value)[0],
				BitConverter.GetBytes(value)[1],
				BitConverter.GetBytes(value)[2],
				BitConverter.GetBytes(value)[3],
				(byte)((int)length % 256),
				(byte)((int)length / 256),
				BitConverter.GetBytes(module)[0],
				BitConverter.GetBytes(module)[1]
			});
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	public static OperateResult<byte[]> BuildAsciiReadSmartModule(ushort module, string address, ushort length)
	{
		try
		{
			uint value = uint.Parse(address);
			byte[] array = new byte[24]
			{
				48, 54, 48, 49, 48, 48, 48, 48, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0
			};
			SoftBasic.BuildAsciiBytesFrom(value).CopyTo(array, 8);
			SoftBasic.BuildAsciiBytesFrom(length).CopyTo(array, 16);
			SoftBasic.BuildAsciiBytesFrom(module).CopyTo(array, 20);
			return OperateResult.Ok(array);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 根据三菱的错误码去查找对象描述信息
	/// </summary>
	/// <param name="code">错误码</param>
	/// <returns>描述信息</returns>
	public static string GetErrorDescription(int code)
	{
		ErrorCode errcode = code switch
		{
			2 => ErrorCode.MelsecError02,
			81 => ErrorCode.MelsecError51,
			82 => ErrorCode.MelsecError52,
			84 => ErrorCode.MelsecError54,
			85 => ErrorCode.MelsecError55,
			86 => ErrorCode.MelsecError56,
			88 => ErrorCode.MelsecError58,
			89 => ErrorCode.MelsecError59,
			49229 => ErrorCode.MelsecErrorC04D,
			49232 => ErrorCode.MelsecErrorC050,
			49233 or 49234 or 49235 or 49236 => ErrorCode.MelsecErrorC051_54,
			49237 => ErrorCode.MelsecErrorC055,
			49238 => ErrorCode.MelsecErrorC056,
			49239 => ErrorCode.MelsecErrorC057,
			49240 => ErrorCode.MelsecErrorC058,
			49241 => ErrorCode.MelsecErrorC059,
			49242 or 49243 => ErrorCode.MelsecErrorC05A_B,
			49244 => ErrorCode.MelsecErrorC05C,
			49245 => ErrorCode.MelsecErrorC05D,
			49246 => ErrorCode.MelsecErrorC05E,
			49247 => ErrorCode.MelsecErrorC05F,
			49248 => ErrorCode.MelsecErrorC060,
			49249 => ErrorCode.MelsecErrorC061,
			49250 => ErrorCode.MelsecErrorC062,
			49264 => ErrorCode.MelsecErrorC070,
			49266 => ErrorCode.MelsecErrorC072,
			49268 => ErrorCode.MelsecErrorC074,
			_ => ErrorCode.MelsecPleaseReferToManualDocument,
		};
		return errcode.Desc();
	}

	/// <summary>
	/// 从三菱的地址中构建MC协议的6字节的ASCII格式的地址
	/// </summary>
	/// <param name="address">三菱地址</param>
	/// <param name="type">三菱的数据类型</param>
	/// <returns>6字节的ASCII格式的地址</returns>
	internal static byte[] BuildBytesFromAddress(int address, MelsecMcDataType type)
	{
		return Encoding.ASCII.GetBytes(address.ToString((type.FromBase == 10) ? "D6" : "X6"));
	}

	/// <summary>
	/// 将0，1，0，1的字节数组压缩成三菱格式的字节数组来表示开关量的
	/// </summary>
	/// <param name="value">原始的数据字节</param>
	/// <returns>压缩过后的数据字节</returns>
	internal static byte[] TransBoolArrayToByteData(byte[] value)
	{
		return TransBoolArrayToByteData(value.Select((byte m) => m != 0).ToArray());
	}

	/// <summary>
	/// 将bool的组压缩成三菱格式的字节数组来表示开关量的
	/// </summary>
	/// <param name="value">原始的数据字节</param>
	/// <returns>压缩过后的数据字节</returns>
	internal static byte[] TransBoolArrayToByteData(bool[] value)
	{
		int num = (value.Length + 1) / 2;
		byte[] array = new byte[num];
		for (int i = 0; i < num; i++)
		{
			if (value[i * 2])
			{
				array[i] += 16;
			}
			if (i * 2 + 1 < value.Length && value[i * 2 + 1])
			{
				array[i]++;
			}
		}
		return array;
	}

	internal static byte[] TransByteArrayToAsciiByteArray(byte[] value)
	{
		if (value == null)
		{
			return Array.Empty<byte>();
		}

		byte[] array = new byte[value.Length * 2];
		for (int i = 0; i < value.Length / 2; i++)
		{
			SoftBasic.BuildAsciiBytesFrom(BitConverter.ToUInt16(value, i * 2)).CopyTo(array, 4 * i);
		}
		return array;
	}

	internal static byte[] TransAsciiByteArrayToByteArray(byte[] value)
	{
		byte[] array = new byte[value.Length / 2];
		for (int i = 0; i < array.Length / 2; i++)
		{
			ushort value2 = Convert.ToUInt16(Encoding.ASCII.GetString(value, i * 4, 4), 16);
			BitConverter.GetBytes(value2).CopyTo(array, i * 2);
		}
		return array;
	}

	/// <summary>
	/// 计算Fx协议指令的和校验信息
	/// </summary>
	/// <param name="data">字节数据</param>
	/// <returns>校验之后的数据</returns>
	internal static byte[] FxCalculateCRC(byte[] data)
	{
		int num = 0;
		for (int i = 1; i < data.Length - 2; i++)
		{
			num += data[i];
		}
		return SoftBasic.BuildAsciiBytesFrom((byte)num);
	}

	/// <summary>
	/// 检查指定的和校验是否是正确的
	/// </summary>
	/// <param name="data">字节数据</param>
	/// <returns>是否成功</returns>
	internal static bool CheckCRC(byte[] data)
	{
		byte[] array = FxCalculateCRC(data);
		if (array[0] != data[^2])
		{
			return false;
		}

		if (array[1] != data[^1])
		{
			return false;
		}
		return true;
	}
}
