using Ops.Communication.Extensions;
using Ops.Communication.Serial;
using Ops.Communication.Utils;

namespace Ops.Communication.Modbus;

/// <summary>
/// Modbus协议相关辅助类
/// </summary>
internal class ModbusHelper
{
	public static OperateResult<byte[]> ExtraRtuResponseContent(byte[] send, byte[] response)
	{
		if (response.Length < 5)
		{
			return new OperateResult<byte[]>("接收的数据长度太短：5");
		}

		if (!SoftCRC16.CheckCRC16(response))
		{
			return new OperateResult<byte[]>("Modbus CRC Check Failed" + SoftBasic.ByteToHexString(response, ' '));
		}

		if (send[1] + 128 == response[1])
		{
			return new OperateResult<byte[]>(response[2], ModbusInfo.GetDescriptionByErrorCode(response[2]));
		}

		if (send[1] != response[1])
		{
			return new OperateResult<byte[]>(response[1], "Receive Command Check Failed: ");
		}
		return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeRtuCommandToCore(response));
	}

	public static OperateResult<byte[]> Read(IModbus modbus, string address, ushort length)
	{
		var operateResult = modbus.TranslateToModbusAddress(address, 3);
		if (!operateResult.IsSuccess)
		{
			return operateResult.ConvertError<byte[]>();
		}

		var operateResult2 = ModbusInfo.BuildReadModbusCommand(operateResult.Content, length, modbus.Station, modbus.AddressStartWithZero, 3);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		var list = new List<byte>();
		for (int i = 0; i < operateResult2.Content.Length; i++)
		{
			var operateResult3 = modbus.ReadFromCoreServer(operateResult2.Content[i]);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult3);
			}

			list.AddRange(operateResult3.Content);
		}

		return OperateResult.Ok(list.ToArray());
	}

	public static async Task<OperateResult<byte[]>> ReadAsync(IModbus modbus, string address, ushort length)
	{
		var modbusAddress = modbus.TranslateToModbusAddress(address, 3);
		if (!modbusAddress.IsSuccess)
		{
			return modbusAddress.ConvertError<byte[]>();
		}

		var command = ModbusInfo.BuildReadModbusCommand(modbusAddress.Content, length, modbus.Station, modbus.AddressStartWithZero, 3);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<byte[]>(command);
		}

		var resultArray = new List<byte>();
		for (int i = 0; i < command.Content.Length; i++)
		{
			var read = await modbus.ReadFromCoreServerAsync(command.Content[i]);
			if (!read.IsSuccess)
			{
				return OperateResult.Error<byte[]>(read);
			}
			resultArray.AddRange(read.Content);
		}

		return OperateResult.Ok(resultArray.ToArray());
	}

	public static OperateResult Write(IModbus modbus, string address, byte[] value)
	{
		var operateResult = modbus.TranslateToModbusAddress(address, 16);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ModbusInfo.BuildWriteWordModbusCommand(operateResult.Content, value, modbus.Station, modbus.AddressStartWithZero, 16);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		return modbus.ReadFromCoreServer(operateResult2.Content);
	}

	public static async Task<OperateResult> WriteAsync(IModbus modbus, string address, byte[] value)
	{
		var modbusAddress = modbus.TranslateToModbusAddress(address, 16);
		if (!modbusAddress.IsSuccess)
		{
			return modbusAddress;
		}

		var command = ModbusInfo.BuildWriteWordModbusCommand(modbusAddress.Content, value, modbus.Station, modbus.AddressStartWithZero, 16);
		if (!command.IsSuccess)
		{
			return command;
		}

		return await modbus.ReadFromCoreServerAsync(command.Content);
	}

	public static OperateResult Write(IModbus modbus, string address, short value)
	{
		var operateResult = modbus.TranslateToModbusAddress(address, 6);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ModbusInfo.BuildWriteWordModbusCommand(operateResult.Content, value, modbus.Station, modbus.AddressStartWithZero, 6);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return modbus.ReadFromCoreServer(operateResult2.Content);
	}

	public static async Task<OperateResult> WriteAsync(IModbus modbus, string address, short value)
	{
		var modbusAddress = modbus.TranslateToModbusAddress(address, 6);
		if (!modbusAddress.IsSuccess)
		{
			return modbusAddress;
		}

		var command = ModbusInfo.BuildWriteWordModbusCommand(modbusAddress.Content, value, modbus.Station, modbus.AddressStartWithZero, 6);
		if (!command.IsSuccess)
		{
			return command;
		}

		return await modbus.ReadFromCoreServerAsync(command.Content);
	}

	public static OperateResult Write(IModbus modbus, string address, ushort value)
	{
		var operateResult = modbus.TranslateToModbusAddress(address, 6);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ModbusInfo.BuildWriteWordModbusCommand(operateResult.Content, value, modbus.Station, modbus.AddressStartWithZero, 6);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		return modbus.ReadFromCoreServer(operateResult2.Content);
	}

	public static async Task<OperateResult> WriteAsync(IModbus modbus, string address, ushort value)
	{
		var modbusAddress = modbus.TranslateToModbusAddress(address, 6);
		if (!modbusAddress.IsSuccess)
		{
			return modbusAddress;
		}

		var command = ModbusInfo.BuildWriteWordModbusCommand(modbusAddress.Content, value, modbus.Station, modbus.AddressStartWithZero, 6);
		if (!command.IsSuccess)
		{
			return command;
		}

		return await modbus.ReadFromCoreServerAsync(command.Content);
	}

	public static OperateResult WriteMask(IModbus modbus, string address, ushort andMask, ushort orMask)
	{
		var operateResult = modbus.TranslateToModbusAddress(address, 22);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ModbusInfo.BuildWriteMaskModbusCommand(operateResult.Content, andMask, orMask, modbus.Station, modbus.AddressStartWithZero, 22);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		return modbus.ReadFromCoreServer(operateResult2.Content);
	}

	public static async Task<OperateResult> WriteMaskAsync(IModbus modbus, string address, ushort andMask, ushort orMask)
	{
		var modbusAddress = modbus.TranslateToModbusAddress(address, 22);
		if (!modbusAddress.IsSuccess)
		{
			return modbusAddress;
		}

		var command = ModbusInfo.BuildWriteMaskModbusCommand(modbusAddress.Content, andMask, orMask, modbus.Station, modbus.AddressStartWithZero, 22);
		if (!command.IsSuccess)
		{
			return command;
		}

		return await modbus.ReadFromCoreServerAsync(command.Content);
	}

	internal static OperateResult<bool[]> ReadBoolHelper(IModbus modbus, string address, ushort length, byte function)
	{
		if (address.IndexOf('.') > 0)
		{
			string[] array = address.SplitDot();
			int num;
			try
			{
				num = Convert.ToInt32(array[1]);
			}
			catch (Exception ex)
			{
				return new OperateResult<bool[]>("Bit Index format wrong, " + ex.Message);
			}

			ushort length2 = (ushort)((length + num + 15) / 16);
			var operateResult = modbus.Read(array[0], length2);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult);
			}

			return OperateResult.Ok(SoftBasic.BytesReverseByWord(operateResult.Content).ToBoolArray().SelectMiddle(num, length));
		}

		var operateResult2 = modbus.TranslateToModbusAddress(address, function);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2.ConvertError<bool[]>();
		}

		var operateResult3 = ModbusInfo.BuildReadModbusCommand(operateResult2.Content, length, modbus.Station, modbus.AddressStartWithZero, function);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult3);
		}

		var list = new List<bool>();
		for (int i = 0; i < operateResult3.Content.Length; i++)
		{
			var operateResult4 = modbus.ReadFromCoreServer(operateResult3.Content[i]);
			if (!operateResult4.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult4);
			}

			int length3 = operateResult3.Content[i][4] * 256 + operateResult3.Content[i][5];
			list.AddRange(SoftBasic.ByteToBoolArray(operateResult4.Content, length3));
		}

		return OperateResult.Ok(list.ToArray());
	}

	internal static async Task<OperateResult<bool[]>> ReadBoolHelperAsync(IModbus modbus, string address, ushort length, byte function)
	{
		if (address.IndexOf('.') > 0)
		{
			string[] addressSplits = address.SplitDot();
			int bitIndex;
			try
			{
				bitIndex = Convert.ToInt32(addressSplits[1]);
			}
			catch (Exception ex2)
			{
				Exception ex = ex2;
				return new OperateResult<bool[]>("Bit Index format wrong, " + ex.Message);
			}

			var read2 = await modbus.ReadAsync(length: (ushort)((length + bitIndex + 15) / 16), address: addressSplits[0]);
			if (!read2.IsSuccess)
			{
				return OperateResult.Error<bool[]>(read2);
			}

			return OperateResult.Ok(SoftBasic.BytesReverseByWord(read2.Content).ToBoolArray().SelectMiddle(bitIndex, length));
		}

		var modbusAddress = modbus.TranslateToModbusAddress(address, function);
		if (!modbusAddress.IsSuccess)
		{
			return modbusAddress.ConvertError<bool[]>();
		}

		var command = ModbusInfo.BuildReadModbusCommand(modbusAddress.Content, length, modbus.Station, modbus.AddressStartWithZero, function);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<bool[]>(command);
		}

		var resultArray = new List<bool>();
		for (int i = 0; i < command.Content.Length; i++)
		{
			var read = await modbus.ReadFromCoreServerAsync(command.Content[i]);
			if (!read.IsSuccess)
			{
				return OperateResult.Error<bool[]>(read);
			}
			resultArray.AddRange(SoftBasic.ByteToBoolArray(length: command.Content[i][4] * 256 + command.Content[i][5], InBytes: read.Content));
		}

		return OperateResult.Ok(resultArray.ToArray());
	}

	public static OperateResult Write(IModbus modbus, string address, bool[] values)
	{
		var operateResult = modbus.TranslateToModbusAddress(address, 15);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ModbusInfo.BuildWriteBoolModbusCommand(operateResult.Content, values, modbus.Station, modbus.AddressStartWithZero, 15);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		return modbus.ReadFromCoreServer(operateResult2.Content);
	}

	public static async Task<OperateResult> WriteAsync(IModbus modbus, string address, bool[] values)
	{
		var modbusAddress = modbus.TranslateToModbusAddress(address, 15);
		if (!modbusAddress.IsSuccess)
		{
			return modbusAddress;
		}

		var command = ModbusInfo.BuildWriteBoolModbusCommand(modbusAddress.Content, values, modbus.Station, modbus.AddressStartWithZero, 15);
		if (!command.IsSuccess)
		{
			return command;
		}

		return await modbus.ReadFromCoreServerAsync(command.Content);
	}

	public static OperateResult Write(IModbus modbus, string address, bool value)
	{
		var operateResult = modbus.TranslateToModbusAddress(address, 5);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ModbusInfo.BuildWriteBoolModbusCommand(operateResult.Content, value, modbus.Station, modbus.AddressStartWithZero, 5);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		return modbus.ReadFromCoreServer(operateResult2.Content);
	}

	public static async Task<OperateResult> WriteAsync(IModbus modbus, string address, bool value)
	{
		var modbusAddress = modbus.TranslateToModbusAddress(address, 5);
		if (!modbusAddress.IsSuccess)
		{
			return modbusAddress;
		}

		var command = ModbusInfo.BuildWriteBoolModbusCommand(modbusAddress.Content, value, modbus.Station, modbus.AddressStartWithZero, 5);
		if (!command.IsSuccess)
		{
			return command;
		}

		return await modbus.ReadFromCoreServerAsync(command.Content);
	}
}
