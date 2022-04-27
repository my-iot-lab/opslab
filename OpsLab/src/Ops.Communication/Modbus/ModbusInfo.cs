using Ops.Communication.Basic;
using Ops.Communication.Core;
using Ops.Communication.Address;
using Ops.Communication.Extensions;
using Ops.Communication.Serial;

namespace Ops.Communication.Modbus;

/// <summary>
/// Modbus协议相关的一些信息，包括功能码定义，报文的生成的定义等等信息。
/// </summary>
public class ModbusInfo
{
	/// <summary>
	/// 读取线圈
	/// </summary>
	public const byte ReadCoil = 1;

	/// <summary>
	/// 读取离散量
	/// </summary>
	public const byte ReadDiscrete = 2;

	/// <summary>
	/// 读取寄存器
	/// </summary>
	public const byte ReadRegister = 3;

	/// <summary>
	/// 读取输入寄存器
	/// </summary>
	public const byte ReadInputRegister = 4;

	/// <summary>
	/// 写单个线圈
	/// </summary>
	public const byte WriteOneCoil = 5;

	/// <summary>
	/// 写单个寄存器
	/// </summary>
	public const byte WriteOneRegister = 6;

	/// <summary>
	/// 写多个线圈
	/// </summary>
	public const byte WriteCoil = 15;

	/// <summary>
	/// 写多个寄存器
	/// </summary>
	public const byte WriteRegister = 16;

	/// <summary>
	/// 使用掩码的方式写入寄存器
	/// </summary>
	public const byte WriteMaskRegister = 22;

	/// <summary>
	/// 不支持该功能码
	/// </summary>
	public const byte FunctionCodeNotSupport = 1;

	/// <summary>
	/// 该地址越界
	/// </summary>
	public const byte FunctionCodeOverBound = 2;

	/// <summary>
	/// 读取长度超过最大值
	/// </summary>
	public const byte FunctionCodeQuantityOver = 3;

	/// <summary>
	/// 读写异常
	/// </summary>
	public const byte FunctionCodeReadWriteException = 4;

	private static void CheckModbusAddressStart(ModbusAddress mAddress, bool isStartWithZero)
	{
		if (!isStartWithZero)
		{
			if (mAddress.Address < 1)
			{
				throw new Exception("ModbusAddressMustMoreThanOne");
			}
			mAddress.Address--;
		}
	}

	/// <summary>
	/// 构建Modbus读取数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码应该根据bool或是字来区分。
	/// </summary>
	/// <param name="address">Modbus的富文本地址</param>
	/// <param name="length">读取的数据长度</param>
	/// <param name="station">默认的站号信息</param>
	/// <param name="isStartWithZero">起始地址是否从0开始</param>
	/// <param name="defaultFunction">默认的功能码</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[][]> BuildReadModbusCommand(string address, ushort length, byte station, bool isStartWithZero, byte defaultFunction)
	{
		try
		{
			var mAddress = new ModbusAddress(address, station, defaultFunction);
			CheckModbusAddressStart(mAddress, isStartWithZero);
			return BuildReadModbusCommand(mAddress, length);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[][]>(ex.Message);
		}
	}

	/// <summary>
	/// 构建Modbus读取数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码应该根据bool或是字来区分。
	/// </summary>
	/// <param name="mAddress">Modbus的富文本地址</param>
	/// <param name="length">读取的数据长度</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[][]> BuildReadModbusCommand(ModbusAddress mAddress, ushort length)
	{
		var list = new List<byte[]>();
		if (mAddress.Function == ReadCoil || mAddress.Function == ReadDiscrete || mAddress.Function == ReadRegister || mAddress.Function == ReadInputRegister)
		{
			var operateResult = OpsHelper.SplitReadLength(mAddress.Address, length, (ushort)((mAddress.Function == ReadCoil || mAddress.Function == ReadDiscrete) ? 2000 : 120));
			for (int i = 0; i < operateResult.Content1.Length; i++)
			{
				list.Add(new byte[6]
				{
					(byte)mAddress.Station,
					(byte)mAddress.Function,
					BitConverter.GetBytes(operateResult.Content1[i])[1],
					BitConverter.GetBytes(operateResult.Content1[i])[0],
					BitConverter.GetBytes(operateResult.Content2[i])[1],
					BitConverter.GetBytes(operateResult.Content2[i])[0]
				});
			}
		}
		else
		{
			list.Add(new byte[6]
			{
				(byte)mAddress.Station,
				(byte)mAddress.Function,
				BitConverter.GetBytes(mAddress.Address)[1],
				BitConverter.GetBytes(mAddress.Address)[0],
				BitConverter.GetBytes(length)[1],
				BitConverter.GetBytes(length)[0]
			});
		}

		return OperateResult.Ok(list.ToArray());
	}

	/// <summary>
	/// 构建Modbus写入bool数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="address">Modbus的富文本地址</param>
	/// <param name="values">bool数组的信息</param>
	/// <param name="station">默认的站号信息</param>
	/// <param name="isStartWithZero">起始地址是否从0开始</param>
	/// <param name="defaultFunction">默认的功能码</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteBoolModbusCommand(string address, bool[] values, byte station, bool isStartWithZero, byte defaultFunction)
	{
		try
		{
			var mAddress = new ModbusAddress(address, station, defaultFunction);
			CheckModbusAddressStart(mAddress, isStartWithZero);
			return BuildWriteBoolModbusCommand(mAddress, values);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 构建Modbus写入bool数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="address">Modbus的富文本地址</param>
	/// <param name="value">bool的信息</param>
	/// <param name="station">默认的站号信息</param>
	/// <param name="isStartWithZero">起始地址是否从0开始</param>
	/// <param name="defaultFunction">默认的功能码</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteBoolModbusCommand(string address, bool value, byte station, bool isStartWithZero, byte defaultFunction)
	{
		try
		{
			if (address.IndexOf('.') <= 0)
			{
				var mAddress = new ModbusAddress(address, station, defaultFunction);
				CheckModbusAddressStart(mAddress, isStartWithZero);
				return BuildWriteBoolModbusCommand(mAddress, value);
			}

			int num = Convert.ToInt32(address[(address.IndexOf('.') + 1)..]);
			if (num < 0 || num > 15)
			{
				return new OperateResult<byte[]>("ModbusBitIndexOverstep");
			}

			int num2 = 1 << num;
			int num3 = ~num2;
			if (!value)
			{
				num2 = 0;
			}

			return BuildWriteMaskModbusCommand(address[..address.IndexOf('.')], (ushort)num3, (ushort)num2, station, isStartWithZero, 22);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 构建Modbus写入bool数组的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码<br />
	/// To construct the core message that Modbus writes to the bool array, you need to specify the address, length, 
	/// station number, whether the starting address is 0, and the default function code
	/// </summary>
	/// <param name="mAddress">Modbus的富文本地址</param>
	/// <param name="values">bool数组的信息</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteBoolModbusCommand(ModbusAddress mAddress, bool[] values)
	{
		try
		{
			byte[] array = SoftBasic.BoolArrayToByte(values);
			byte[] array2 = new byte[7 + array.Length];
			array2[0] = (byte)mAddress.Station;
			array2[1] = (byte)mAddress.Function;
			array2[2] = BitConverter.GetBytes(mAddress.Address)[1];
			array2[3] = BitConverter.GetBytes(mAddress.Address)[0];
			array2[4] = (byte)(values.Length / 256);
			array2[5] = (byte)(values.Length % 256);
			array2[6] = (byte)array.Length;
			array.CopyTo(array2, 7);
			return OperateResult.Ok(array2);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 构建Modbus写入bool数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="mAddress">Modbus的富文本地址</param>
	/// <param name="value">bool数据的信息</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteBoolModbusCommand(ModbusAddress mAddress, bool value)
	{
		byte[] array = new byte[6]
		{
			(byte)mAddress.Station,
			(byte)mAddress.Function,
			BitConverter.GetBytes(mAddress.Address)[1],
			BitConverter.GetBytes(mAddress.Address)[0],
			0,
			0
		};

		if (value)
		{
			array[4] = byte.MaxValue;
			array[5] = 0;
		}
		else
		{
			array[4] = 0;
			array[5] = 0;
		}
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 构建Modbus写入字数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="address">Modbus的富文本地址</param>
	/// <param name="values">bool数组的信息</param>
	/// <param name="station">默认的站号信息</param>
	/// <param name="isStartWithZero">起始地址是否从0开始</param>
	/// <param name="defaultFunction">默认的功能码</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteWordModbusCommand(string address, byte[] values, byte station, bool isStartWithZero, byte defaultFunction)
	{
		try
		{
			var modbusAddress = new ModbusAddress(address, station, defaultFunction);
			if (modbusAddress.Function == 3)
			{
				modbusAddress.Function = defaultFunction;
			}
			CheckModbusAddressStart(modbusAddress, isStartWithZero);
			return BuildWriteWordModbusCommand(modbusAddress, values);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 构建Modbus写入字数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="address">Modbus的富文本地址</param>
	/// <param name="value">short数据信息</param>
	/// <param name="station">默认的站号信息</param>
	/// <param name="isStartWithZero">起始地址是否从0开始</param>
	/// <param name="defaultFunction">默认的功能码</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteWordModbusCommand(string address, short value, byte station, bool isStartWithZero, byte defaultFunction)
	{
		try
		{
			var modbusAddress = new ModbusAddress(address, station, defaultFunction);
			if (modbusAddress.Function == 3)
			{
				modbusAddress.Function = defaultFunction;
			}
			CheckModbusAddressStart(modbusAddress, isStartWithZero);
			return BuildWriteOneRegisterModbusCommand(modbusAddress, value);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 构建Modbus写入字数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="address">Modbus的富文本地址</param>
	/// <param name="value">bool数组的信息</param>
	/// <param name="station">默认的站号信息</param>
	/// <param name="isStartWithZero">起始地址是否从0开始</param>
	/// <param name="defaultFunction">默认的功能码</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteWordModbusCommand(string address, ushort value, byte station, bool isStartWithZero, byte defaultFunction)
	{
		try
		{
			var modbusAddress = new ModbusAddress(address, station, defaultFunction);
			if (modbusAddress.Function == 3)
			{
				modbusAddress.Function = defaultFunction;
			}
			CheckModbusAddressStart(modbusAddress, isStartWithZero);
			return BuildWriteOneRegisterModbusCommand(modbusAddress, value);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 构建Modbus写入掩码的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="address">Modbus的富文本地址</param>
	/// <param name="andMask">进行与操作的掩码信息</param>
	/// <param name="orMask">进行或操作的掩码信息</param>
	/// <param name="station">默认的站号信息</param>
	/// <param name="isStartWithZero">起始地址是否从0开始</param>
	/// <param name="defaultFunction">默认的功能码</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteMaskModbusCommand(string address, ushort andMask, ushort orMask, byte station, bool isStartWithZero, byte defaultFunction)
	{
		try
		{
			var modbusAddress = new ModbusAddress(address, station, defaultFunction);
			if (modbusAddress.Function == 3)
			{
				modbusAddress.Function = defaultFunction;
			}
			CheckModbusAddressStart(modbusAddress, isStartWithZero);
			return BuildWriteMaskModbusCommand(modbusAddress, andMask, orMask);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 构建Modbus写入字数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="mAddress">Modbus的富文本地址</param>
	/// <param name="values">bool数组的信息</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteWordModbusCommand(ModbusAddress mAddress, byte[] values)
	{
		byte[] array = new byte[7 + values.Length];
		array[0] = (byte)mAddress.Station;
		array[1] = (byte)mAddress.Function;
		array[2] = BitConverter.GetBytes(mAddress.Address)[1];
		array[3] = BitConverter.GetBytes(mAddress.Address)[0];
		array[4] = (byte)(values.Length / 2 / 256);
		array[5] = (byte)(values.Length / 2 % 256);
		array[6] = (byte)values.Length;
		values.CopyTo(array, 7);
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 构建Modbus写入掩码数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="mAddress">Modbus的富文本地址</param>
	/// <param name="andMask">等待进行与操作的掩码</param>
	/// <param name="orMask">等待进行或操作的掩码</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteMaskModbusCommand(ModbusAddress mAddress, ushort andMask, ushort orMask)
	{
		return OperateResult.Ok(new byte[8]
		{
			(byte)mAddress.Station,
			(byte)mAddress.Function,
			BitConverter.GetBytes(mAddress.Address)[1],
			BitConverter.GetBytes(mAddress.Address)[0],
			BitConverter.GetBytes(andMask)[1],
			BitConverter.GetBytes(andMask)[0],
			BitConverter.GetBytes(orMask)[1],
			BitConverter.GetBytes(orMask)[0]
		});
	}

	/// <summary>
	/// 构建Modbus写入字数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="mAddress">Modbus的富文本地址</param>
	/// <param name="value">short的值</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteOneRegisterModbusCommand(ModbusAddress mAddress, short value)
	{
		return OperateResult.Ok(new byte[6]
		{
			(byte)mAddress.Station,
			(byte)mAddress.Function,
			BitConverter.GetBytes(mAddress.Address)[1],
			BitConverter.GetBytes(mAddress.Address)[0],
			BitConverter.GetBytes(value)[1],
			BitConverter.GetBytes(value)[0]
		});
	}

	/// <summary>
	/// 构建Modbus写入字数据的核心报文，需要指定地址，长度，站号，是否起始地址0，默认的功能码。
	/// </summary>
	/// <param name="mAddress">Modbus的富文本地址</param>
	/// <param name="value">ushort的值</param>
	/// <returns>包含最终命令的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteOneRegisterModbusCommand(ModbusAddress mAddress, ushort value)
	{
		return OperateResult.Ok(new byte[6]
		{
			(byte)mAddress.Station,
			(byte)mAddress.Function,
			BitConverter.GetBytes(mAddress.Address)[1],
			BitConverter.GetBytes(mAddress.Address)[0],
			BitConverter.GetBytes(value)[1],
			BitConverter.GetBytes(value)[0]
		});
	}

	/// <summary>
	/// 从返回的modbus的书内容中，提取出真实的数据，适用于写入和读取操作。
	/// </summary>
	/// <param name="response">返回的核心modbus报文信息</param>
	/// <returns>结果数据内容</returns>
	public static OperateResult<byte[]> ExtractActualData(byte[] response)
	{
		try
		{
			if (response[1] >= 128)
			{
				return new OperateResult<byte[]>(GetDescriptionByErrorCode(response[2]));
			}
			if (response.Length > 3)
			{
				return OperateResult.Ok(SoftBasic.ArrayRemoveBegin(response, 3));
			}
			return OperateResult.Ok(new byte[0]);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>(ex.Message);
		}
	}

	/// <summary>
	/// 将modbus指令打包成Modbus-Tcp指令，需要指定ID信息来添加6个字节的报文头。
	/// </summary>
	/// <param name="modbus">Modbus核心指令</param>
	/// <param name="id">消息的序号</param>
	/// <returns>Modbus-Tcp指令</returns>
	public static byte[] PackCommandToTcp(byte[] modbus, ushort id)
	{
		byte[] array = new byte[modbus.Length + 6];
		array[0] = BitConverter.GetBytes(id)[1];
		array[1] = BitConverter.GetBytes(id)[0];
		array[4] = BitConverter.GetBytes(modbus.Length)[1];
		array[5] = BitConverter.GetBytes(modbus.Length)[0];
		modbus.CopyTo(array, 6);
		return array;
	}

	/// <summary>
	/// 将modbus-tcp的报文数据重新还原成modbus指令，移除6个字节的报文头数据。
	/// </summary>
	/// <param name="modbusTcp">modbus-tcp的报文</param>
	/// <returns>modbus数据报文</returns>
	public static byte[] ExplodeTcpCommandToCore(byte[] modbusTcp)
	{
		return modbusTcp.RemoveBegin(6);
	}

	/// <summary>
	/// 将modbus-rtu的数据重新还原成modbus数据，移除CRC校验的内容。
	/// </summary>
	/// <param name="modbusRtu">modbus-rtu的报文</param>
	/// <returns>modbus数据报文</returns>
	public static byte[] ExplodeRtuCommandToCore(byte[] modbusRtu)
	{
		return modbusRtu.RemoveLast(2);
	}

	/// <summary>
	/// 将modbus指令打包成Modbus-Rtu指令，在报文的末尾添加CRC16的校验码。
	/// </summary>
	/// <param name="modbus">Modbus指令</param>
	/// <returns>Modbus-Rtu指令</returns>
	public static byte[] PackCommandToRtu(byte[] modbus)
	{
		return SoftCRC16.CRC16(modbus);
	}

	/// <summary>
	/// 将一个modbus核心的数据报文，转换成modbus-ascii的数据报文，增加LRC校验，增加首尾标记数据。
	/// </summary>
	/// <param name="modbus">modbus-rtu的完整报文，携带相关的校验码</param>
	/// <returns>可以用于直接发送的modbus-ascii的报文</returns>
	public static byte[] TransModbusCoreToAsciiPackCommand(byte[] modbus)
	{
		byte[] inBytes = SoftLRC.LRC(modbus);
		byte[] array = SoftBasic.BytesToAsciiBytes(inBytes);
		return SoftBasic.SpliceArray(new byte[1] { 58 }, array, new byte[2] { 13, 10 });
	}

	/// <summary>
	/// 将一个modbus-ascii的数据报文，转换成的modbus核心数据报文，移除首尾标记，移除LRC校验。
	/// </summary>
	/// <param name="modbusAscii">modbus-ascii的完整报文，携带相关的校验码</param>
	/// <returns>可以用于直接发送的modbus的报文</returns>
	public static OperateResult<byte[]> TransAsciiPackCommandToCore(byte[] modbusAscii)
	{
		try
		{
			if (modbusAscii[0] != 58 || modbusAscii[^2] != 13 || modbusAscii[^1] != 10)
			{
				return new OperateResult<byte[]>
				{
					Message = "ModbusAsciiFormatCheckFailed:" + modbusAscii.ToHexString(' ')
				};
			}

			byte[] array = SoftBasic.AsciiBytesToBytes(modbusAscii.RemoveDouble(1, 2));
			if (!SoftLRC.CheckLRC(array))
			{
				return new OperateResult<byte[]>
				{
					Message = "ModbusLRCCheckFailed" + array.ToHexString(' ')
				};
			}

			return OperateResult.Ok(array.RemoveLast(1));
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>
			{
				Message = ex.Message + modbusAscii.ToHexString(' ')
			};
		}
	}

	/// <summary>
	/// 分析Modbus协议的地址信息，该地址适应于tcp及rtu模式。
	/// </summary>
	/// <param name="address">带格式的地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
	/// <param name="defaultStation">默认的站号信息</param>
	/// <param name="isStartWithZero">起始地址是否从0开始</param>
	/// <param name="defaultFunction">默认的功能码信息</param>
	/// <returns>转换后的地址信息</returns>
	public static OperateResult<ModbusAddress> AnalysisAddress(string address, byte defaultStation, bool isStartWithZero, byte defaultFunction)
	{
		try
		{
			var modbusAddress = new ModbusAddress(address, defaultStation, defaultFunction);
			if (!isStartWithZero)
			{
				if (modbusAddress.Address < 1)
				{
					throw new Exception("ModbusAddressMustMoreThanOne");
				}
				modbusAddress.Address--;
			}
			return OperateResult.Ok(modbusAddress);
		}
		catch (Exception ex)
		{
			return new OperateResult<ModbusAddress>
			{
				Message = ex.Message
			};
		}
	}

	/// <summary>
	/// 通过错误码来获取到对应的文本消息。
	/// </summary>
	/// <param name="code">错误码</param>
	/// <returns>错误的文本描述</returns>
	public static string GetDescriptionByErrorCode(byte code)
	{
		return code switch
		{
			1 => "ModbusTcpFunctionCodeNotSupport",
			2 => "ModbusTcpFunctionCodeOverBound",
			3 => "ModbusTcpFunctionCodeQuantityOver",
			4 => "ModbusTcpFunctionCodeReadWriteException",
			_ => "UnknownError",
		};
	}
}
