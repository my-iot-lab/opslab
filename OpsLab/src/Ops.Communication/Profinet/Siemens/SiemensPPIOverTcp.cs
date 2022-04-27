using Ops.Communication.Address;
using Ops.Communication.Basic;
using Ops.Communication.Core;
using Ops.Communication.Core.Net;

namespace Ops.Communication.Profinet.Siemens;

public class SiemensPPIOverTcp : NetworkDeviceBase
{
	private byte station = 2;

	private readonly object communicationLock;

	public byte Station
	{
		get
		{
			return station;
		}
		set
		{
			station = value;
		}
	}

	public SiemensPPIOverTcp()
	{
		base.WordLength = 2;
		base.ByteTransform = new ReverseBytesTransform();
		communicationLock = new object();
		base.SleepTime = 20;
	}

	/// <summary>
	/// 使用指定的ip地址和端口号来实例化对象<br />
	/// Instantiate the object with the specified IP address and port number
	/// </summary>
	/// <param name="ipAddress">Ip地址信息</param>
	/// <param name="port">端口号信息</param>
	public SiemensPPIOverTcp(string ipAddress, int port)
		: this()
	{
		IpAddress = ipAddress;
		Port = port;
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = BuildReadCommand(b, address, length, isBit: false);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 229)
			{
				return new OperateResult<byte[]>("PLC Receive Check Failed:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			}
			OperateResult<byte[]> operateResult3 = ReadFromCoreServer(GetExecuteConfirm(b));
			if (!operateResult3.IsSuccess)
			{
				return operateResult3;
			}
			OperateResult operateResult4 = CheckResponse(operateResult3.Content);
			if (!operateResult4.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult4);
			}
			byte[] array = new byte[length];
			if (operateResult3.Content[21] == byte.MaxValue && operateResult3.Content[22] == 4)
			{
				Array.Copy(operateResult3.Content, 25, array, 0, length);
			}
			return OperateResult.Ok(array);
		}
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = BuildReadCommand(b, address, length, isBit: true);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult2);
			}
			if (operateResult2.Content[0] != 229)
			{
				return new OperateResult<bool[]>("PLC Receive Check Failed:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			}
			OperateResult<byte[]> operateResult3 = ReadFromCoreServer(GetExecuteConfirm(b));
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult3);
			}
			OperateResult operateResult4 = CheckResponse(operateResult3.Content);
			if (!operateResult4.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult4);
			}
			byte[] array = new byte[operateResult3.Content.Length - 27];
			if (operateResult3.Content[21] == byte.MaxValue && operateResult3.Content[22] == 3)
			{
				Array.Copy(operateResult3.Content, 25, array, 0, array.Length);
			}
			return OperateResult.Ok(SoftBasic.ByteToBoolArray(array, length));
		}
	}

	public override OperateResult Write(string address, byte[] value)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = BuildWriteCommand(b, address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 229)
			{
				return new OperateResult<byte[]>("PLC Receive Check Failed:" + operateResult2.Content[0]);
			}
			OperateResult<byte[]> operateResult3 = ReadFromCoreServer(GetExecuteConfirm(b));
			if (!operateResult3.IsSuccess)
			{
				return operateResult3;
			}
			OperateResult operateResult4 = CheckResponse(operateResult3.Content);
			if (!operateResult4.IsSuccess)
			{
				return operateResult4;
			}
			return OperateResult.Ok();
		}
	}

	public override OperateResult Write(string address, bool[] value)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = BuildWriteCommand(b, address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 229)
			{
				return new OperateResult<byte[]>("PLC Receive Check Failed:" + operateResult2.Content[0]);
			}
			OperateResult<byte[]> operateResult3 = ReadFromCoreServer(GetExecuteConfirm(b));
			if (!operateResult3.IsSuccess)
			{
				return operateResult3;
			}
			OperateResult operateResult4 = CheckResponse(operateResult3.Content);
			if (!operateResult4.IsSuccess)
			{
				return operateResult4;
			}
			return OperateResult.Ok();
		}
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		byte stat = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> command = BuildReadCommand(stat, address, length, isBit: false);
		if (!command.IsSuccess)
		{
			return command;
		}

		OperateResult<byte[]> read1 = await ReadFromCoreServerAsync(command.Content);
		if (!read1.IsSuccess)
		{
			return read1;
		}
		if (read1.Content[0] != 229)
		{
			return new OperateResult<byte[]>("PLC Receive Check Failed:" + SoftBasic.ByteToHexString(read1.Content, ' '));
		}
		OperateResult<byte[]> read2 = await ReadFromCoreServerAsync(GetExecuteConfirm(stat));
		if (!read2.IsSuccess)
		{
			return read2;
		}
		OperateResult check = CheckResponse(read2.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		byte[] buffer = new byte[length];
		if (read2.Content[21] == byte.MaxValue && read2.Content[22] == 4)
		{
			Array.Copy(read2.Content, 25, buffer, 0, length);
		}
		return OperateResult.Ok(buffer);
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		byte stat = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> command = BuildReadCommand(stat, address, length, isBit: true);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<bool[]>(command);
		}
		OperateResult<byte[]> read1 = await ReadFromCoreServerAsync(command.Content);
		if (!read1.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read1);
		}
		if (read1.Content[0] != 229)
		{
			return new OperateResult<bool[]>("PLC Receive Check Failed:" + SoftBasic.ByteToHexString(read1.Content, ' '));
		}
		OperateResult<byte[]> read2 = await ReadFromCoreServerAsync(GetExecuteConfirm(stat));
		if (!read2.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read2);
		}
		OperateResult check = CheckResponse(read2.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<bool[]>(check);
		}
		byte[] buffer = new byte[read2.Content.Length - 27];
		if (read2.Content[21] == byte.MaxValue && read2.Content[22] == 3)
		{
			Array.Copy(read2.Content, 25, buffer, 0, buffer.Length);
		}
		return OperateResult.Ok(SoftBasic.ByteToBoolArray(buffer, length));
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		byte stat = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> command = BuildWriteCommand(stat, address, value);
		if (!command.IsSuccess)
		{
			return command;
		}
		OperateResult<byte[]> read1 = await ReadFromCoreServerAsync(command.Content);
		if (!read1.IsSuccess)
		{
			return read1;
		}
		if (read1.Content[0] != 229)
		{
			return new OperateResult<byte[]>("PLC Receive Check Failed:" + read1.Content[0]);
		}
		OperateResult<byte[]> read2 = await ReadFromCoreServerAsync(GetExecuteConfirm(stat));
		if (!read2.IsSuccess)
		{
			return read2;
		}
		OperateResult check = CheckResponse(read2.Content);
		if (!check.IsSuccess)
		{
			return check;
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult> WriteAsync(string address, bool[] value)
	{
		byte stat = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> command = BuildWriteCommand(stat, address, value);
		if (!command.IsSuccess)
		{
			return command;
		}
		OperateResult<byte[]> read1 = await ReadFromCoreServerAsync(command.Content);
		if (!read1.IsSuccess)
		{
			return read1;
		}
		if (read1.Content[0] != 229)
		{
			return new OperateResult<byte[]>("PLC Receive Check Failed:" + read1.Content[0]);
		}
		OperateResult<byte[]> read2 = await ReadFromCoreServerAsync(GetExecuteConfirm(stat));
		if (!read2.IsSuccess)
		{
			return read2;
		}
		OperateResult check = CheckResponse(read2.Content);
		if (!check.IsSuccess)
		{
			return check;
		}
		return OperateResult.Ok();
	}

	public OperateResult<byte> ReadByte(string address)
	{
		return ByteTransformHelper.GetResultFromArray(Read(address, 1));
	}

	public OperateResult Write(string address, byte value)
	{
		return Write(address, new byte[1] { value });
	}

	public async Task<OperateResult<byte>> ReadByteAsync(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1));
	}

	public async Task<OperateResult> WriteAsync(string address, byte value)
	{
		return await WriteAsync(address, new byte[1] { value });
	}

	public OperateResult Start(string parameter = "")
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref parameter, "s", Station);
		byte[] obj = new byte[39]
		{
			104, 33, 33, 104, 0, 0, 108, 50, 1, 0,
			0, 0, 0, 0, 20, 0, 0, 40, 0, 0,
			0, 0, 0, 0, 253, 0, 0, 9, 80, 95,
			80, 82, 79, 71, 82, 65, 77, 170, 22
		};
		obj[4] = station;
		byte[] send = obj;
		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult = ReadFromCoreServer(send);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			if (operateResult.Content[0] != 229)
			{
				return new OperateResult<byte[]>("PLC Receive Check Failed:" + operateResult.Content[0]);
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(GetExecuteConfirm(b));
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			return OperateResult.Ok();
		}
	}

	public OperateResult Stop(string parameter = "")
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref parameter, "s", Station);
		byte[] obj = new byte[35]
		{
			104, 29, 29, 104, 0, 0, 108, 50, 1, 0,
			0, 0, 0, 0, 16, 0, 0, 41, 0, 0,
			0, 0, 0, 9, 80, 95, 80, 82, 79, 71,
			82, 65, 77, 170, 22
		};
		obj[4] = station;
		byte[] send = obj;
		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult = ReadFromCoreServer(send);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			if (operateResult.Content[0] != 229)
			{
				return new OperateResult<byte[]>("PLC Receive Check Failed:" + operateResult.Content[0]);
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(GetExecuteConfirm(b));
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			return OperateResult.Ok();
		}
	}

	public async Task<OperateResult> StartAsync(string parameter = "")
	{
		byte stat = (byte)OpsHelper.ExtractParameter(ref parameter, "s", Station);
		byte[] obj = new byte[39]
		{
			104, 33, 33, 104, 0, 0, 108, 50, 1, 0,
			0, 0, 0, 0, 20, 0, 0, 40, 0, 0,
			0, 0, 0, 0, 253, 0, 0, 9, 80, 95,
			80, 82, 79, 71, 82, 65, 77, 170, 22
		};
		obj[4] = station;
		byte[] cmd = obj;
		OperateResult<byte[]> read1 = await ReadFromCoreServerAsync(cmd);
		if (!read1.IsSuccess)
		{
			return read1;
		}
		if (read1.Content[0] != 229)
		{
			return new OperateResult<byte[]>("PLC Receive Check Failed:" + read1.Content[0]);
		}
		OperateResult<byte[]> read2 = await ReadFromCoreServerAsync(GetExecuteConfirm(stat));
		if (!read2.IsSuccess)
		{
			return read2;
		}
		return OperateResult.Ok();
	}

	public async Task<OperateResult> StopAsync(string parameter = "")
	{
		byte stat = (byte)OpsHelper.ExtractParameter(ref parameter, "s", Station);
		byte[] obj = new byte[35]
		{
			104, 29, 29, 104, 0, 0, 108, 50, 1, 0,
			0, 0, 0, 0, 16, 0, 0, 41, 0, 0,
			0, 0, 0, 9, 80, 95, 80, 82, 79, 71,
			82, 65, 77, 170, 22
		};
		obj[4] = station;
		byte[] cmd = obj;
		OperateResult<byte[]> read1 = await ReadFromCoreServerAsync(cmd);
		if (!read1.IsSuccess)
		{
			return read1;
		}
		if (read1.Content[0] != 229)
		{
			return new OperateResult<byte[]>("PLC Receive Check Failed:" + read1.Content[0]);
		}
		OperateResult<byte[]> read2 = await ReadFromCoreServerAsync(GetExecuteConfirm(stat));
		if (!read2.IsSuccess)
		{
			return read2;
		}
		return OperateResult.Ok();
	}

	public override string ToString()
	{
		return $"SiemensPPIOverTcp[{IpAddress}:{Port}]";
	}

	/// <summary>
	/// 解析数据地址，解析出地址类型，起始地址，DB块的地址<br />
	/// Parse data address, parse out address type, start address, db block address
	/// </summary>
	/// <param name="address">起始地址，例如M100，I0，Q0，V100 -&gt;
	/// Start address, such as M100,I0,Q0,V100</param>
	/// <returns>解析数据地址，解析出地址类型，起始地址，DB块的地址 -&gt;
	/// Parse data address, parse out address type, start address, db block address</returns>
	public static OperateResult<byte, int, ushort> AnalysisAddress(string address)
	{
		OperateResult<byte, int, ushort> operateResult = new OperateResult<byte, int, ushort>();
		try
		{
			operateResult.Content3 = 0;
			if (address[..2] == "AI")
			{
				operateResult.Content1 = 6;
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[2..]);
			}
			else if (address[..2] == "AQ")
			{
				operateResult.Content1 = 7;
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[2..]);
			}
			else if (address[0] == 'T')
			{
				operateResult.Content1 = 31;
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[1..]);
			}
			else if (address[0] == 'C')
			{
				operateResult.Content1 = 30;
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[1..]);
			}
			else if (address[..2] == "SM")
			{
				operateResult.Content1 = 5;
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[2..]);
			}
			else if (address[0] == 'S')
			{
				operateResult.Content1 = 4;
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[1..]);
			}
			else if (address[0] == 'I')
			{
				operateResult.Content1 = 129;
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[1..]);
			}
			else if (address[0] == 'Q')
			{
				operateResult.Content1 = 130;
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[1..]);
			}
			else if (address[0] == 'M')
			{
				operateResult.Content1 = 131;
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[1..]);
			}
			else if (address[0] == 'D' || address[..2] == "DB")
			{
				operateResult.Content1 = 132;
				string[] array = address.Split(new char[1] { '.' });
				if (address[1] == 'B')
				{
					operateResult.Content3 = Convert.ToUInt16(array[0][2..]);
				}
				else
				{
					operateResult.Content3 = Convert.ToUInt16(array[0][1..]);
				}
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[(address.IndexOf('.') + 1)..]);
			}
			else
			{
				if (address[0] != 'V')
				{
					operateResult.Message = "NotSupportedDataType";
					operateResult.Content1 = 0;
					operateResult.Content2 = 0;
					operateResult.Content3 = 0;
					return operateResult;
				}
				operateResult.Content1 = 132;
				operateResult.Content3 = 1;
				operateResult.Content2 = S7AddressData.CalculateAddressStarted(address[1..]);
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
	/// 生成一个读取字数据指令头的通用方法<br />
	/// A general method for generating a command header to read a Word data
	/// </summary>
	/// <param name="station">设备的站号信息 -&gt; Station number information for the device</param>
	/// <param name="address">起始地址，例如M100，I0，Q0，V100 -&gt;
	/// Start address, such as M100,I0,Q0,V100</param>
	/// <param name="length">读取数据长度 -&gt; Read Data length</param>
	/// <param name="isBit">是否为位读取</param>
	/// <returns>包含结果对象的报文 -&gt; Message containing the result object</returns>
	public static OperateResult<byte[]> BuildReadCommand(byte station, string address, ushort length, bool isBit)
	{
		OperateResult<byte, int, ushort> operateResult = AnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}
		byte[] array = new byte[33];
		array[0] = 104;
		array[1] = BitConverter.GetBytes(array.Length - 6)[0];
		array[2] = BitConverter.GetBytes(array.Length - 6)[0];
		array[3] = 104;
		array[4] = station;
		array[5] = 0;
		array[6] = 108;
		array[7] = 50;
		array[8] = 1;
		array[9] = 0;
		array[10] = 0;
		array[11] = 0;
		array[12] = 0;
		array[13] = 0;
		array[14] = 14;
		array[15] = 0;
		array[16] = 0;
		array[17] = 4;
		array[18] = 1;
		array[19] = 18;
		array[20] = 10;
		array[21] = 16;
		array[22] = (byte)(isBit ? 1 : 2);
		array[23] = 0;
		array[24] = BitConverter.GetBytes(length)[0];
		array[25] = BitConverter.GetBytes(length)[1];
		array[26] = (byte)operateResult.Content3;
		array[27] = operateResult.Content1;
		array[28] = BitConverter.GetBytes(operateResult.Content2)[2];
		array[29] = BitConverter.GetBytes(operateResult.Content2)[1];
		array[30] = BitConverter.GetBytes(operateResult.Content2)[0];
		int num = 0;
		for (int i = 4; i < 31; i++)
		{
			num += array[i];
		}
		array[31] = BitConverter.GetBytes(num)[0];
		array[32] = 22;
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 生成一个写入PLC数据信息的报文内容
	/// </summary>
	/// <param name="station">PLC的站号</param>
	/// <param name="address">地址</param>
	/// <param name="values">数据值</param>
	/// <returns>是否写入成功</returns>
	public static OperateResult<byte[]> BuildWriteCommand(byte station, string address, byte[] values)
	{
		OperateResult<byte, int, ushort> operateResult = AnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}
		int num = values.Length;
		byte[] array = new byte[37 + values.Length];
		array[0] = 104;
		array[1] = BitConverter.GetBytes(array.Length - 6)[0];
		array[2] = BitConverter.GetBytes(array.Length - 6)[0];
		array[3] = 104;
		array[4] = station;
		array[5] = 0;
		array[6] = 124;
		array[7] = 50;
		array[8] = 1;
		array[9] = 0;
		array[10] = 0;
		array[11] = 0;
		array[12] = 0;
		array[13] = 0;
		array[14] = 14;
		array[15] = 0;
		array[16] = (byte)(values.Length + 4);
		array[17] = 5;
		array[18] = 1;
		array[19] = 18;
		array[20] = 10;
		array[21] = 16;
		array[22] = 2;
		array[23] = 0;
		array[24] = BitConverter.GetBytes(num)[0];
		array[25] = BitConverter.GetBytes(num)[1];
		array[26] = (byte)operateResult.Content3;
		array[27] = operateResult.Content1;
		array[28] = BitConverter.GetBytes(operateResult.Content2)[2];
		array[29] = BitConverter.GetBytes(operateResult.Content2)[1];
		array[30] = BitConverter.GetBytes(operateResult.Content2)[0];
		array[31] = 0;
		array[32] = 4;
		array[33] = BitConverter.GetBytes(num * 8)[1];
		array[34] = BitConverter.GetBytes(num * 8)[0];
		values.CopyTo(array, 35);
		int num2 = 0;
		for (int i = 4; i < array.Length - 2; i++)
		{
			num2 += array[i];
		}
		array[array.Length - 2] = BitConverter.GetBytes(num2)[0];
		array[array.Length - 1] = 22;
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 根据错误信息，获取到文本信息
	/// </summary>
	/// <param name="code">状态</param>
	/// <returns>消息文本</returns>
	public static string GetMsgFromStatus(byte code)
	{
		return code switch
		{
			byte.MaxValue => "No error",
			1 => "Hardware fault",
			3 => "Illegal object access",
			5 => "Invalid address(incorrent variable address)",
			6 => "Data type is not supported",
			10 => "Object does not exist or length error",
			_ => "UnknownError",
		};
	}

	/// <summary>
	/// 根据错误信息，获取到文本信息
	/// </summary>
	/// <param name="errorClass">错误类型</param>
	/// <param name="errorCode">错误代码</param>
	/// <returns>错误信息</returns>
	public static string GetMsgFromStatus(byte errorClass, byte errorCode)
	{
		if (errorClass == 128 && errorCode == 1)
		{
			return "Switch\u2002in\u2002wrong\u2002position\u2002for\u2002requested\u2002operation";
		}
		if (errorClass == 129 && errorCode == 4)
		{
			return "Miscellaneous\u2002structure\u2002error\u2002in\u2002command.\u2002\u2002Command is not supportedby CPU";
		}
		if (errorClass == 132 && errorCode == 4)
		{
			return "CPU is busy processing an upload or download CPU cannot process command because of system fault condition";
		}
		if (errorClass == 133 && errorCode == 0)
		{
			return "Length fields are not correct or do not agree with the amount of data received";
		}
		int num;
		switch (errorClass)
		{
			case 210:
				return "Error in upload or download command";
			case 214:
				return "Protection error(password)";
			case 220:
				num = ((errorCode == 1) ? 1 : 0);
				break;
			default:
				num = 0;
				break;
		}
		if (num != 0)
		{
			return "Error in time-of-day clock data";
		}
		return "UnknownError";
	}

	/// <summary>
	/// 创建写入PLC的bool类型数据报文指令
	/// </summary>
	/// <param name="station">PLC的站号信息</param>
	/// <param name="address">地址信息</param>
	/// <param name="values">bool[]数据值</param>
	/// <returns>带有成功标识的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteCommand(byte station, string address, bool[] values)
	{
		OperateResult<byte, int, ushort> operateResult = AnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}
		byte[] array = SoftBasic.BoolArrayToByte(values);
		byte[] array2 = new byte[37 + array.Length];
		array2[0] = 104;
		array2[1] = BitConverter.GetBytes(array2.Length - 6)[0];
		array2[2] = BitConverter.GetBytes(array2.Length - 6)[0];
		array2[3] = 104;
		array2[4] = station;
		array2[5] = 0;
		array2[6] = 124;
		array2[7] = 50;
		array2[8] = 1;
		array2[9] = 0;
		array2[10] = 0;
		array2[11] = 0;
		array2[12] = 0;
		array2[13] = 0;
		array2[14] = 14;
		array2[15] = 0;
		array2[16] = 5;
		array2[17] = 5;
		array2[18] = 1;
		array2[19] = 18;
		array2[20] = 10;
		array2[21] = 16;
		array2[22] = 1;
		array2[23] = 0;
		array2[24] = BitConverter.GetBytes(values.Length)[0];
		array2[25] = BitConverter.GetBytes(values.Length)[1];
		array2[26] = (byte)operateResult.Content3;
		array2[27] = operateResult.Content1;
		array2[28] = BitConverter.GetBytes(operateResult.Content2)[2];
		array2[29] = BitConverter.GetBytes(operateResult.Content2)[1];
		array2[30] = BitConverter.GetBytes(operateResult.Content2)[0];
		array2[31] = 0;
		array2[32] = 3;
		array2[33] = BitConverter.GetBytes(values.Length)[1];
		array2[34] = BitConverter.GetBytes(values.Length)[0];
		array.CopyTo(array2, 35);
		int num = 0;
		for (int i = 4; i < array2.Length - 2; i++)
		{
			num += array2[i];
		}
		array2[array2.Length - 2] = BitConverter.GetBytes(num)[0];
		array2[array2.Length - 1] = 22;
		return OperateResult.Ok(array2);
	}

	/// <summary>
	/// 检查西门子PLC的返回的数据和合法性，对反馈的数据进行初步的校验
	/// </summary>
	/// <param name="content">服务器返回的原始的数据内容</param>
	/// <returns>是否校验成功</returns>
	public static OperateResult CheckResponse(byte[] content)
	{
		if (content.Length < 21)
		{
			return new OperateResult(10000, "Failed, data too short:" + SoftBasic.ByteToHexString(content, ' '));
		}
		if (content[17] != 0 || content[18] != 0)
		{
			return new OperateResult(content[19], GetMsgFromStatus(content[18], content[19]));
		}
		if (content[21] != byte.MaxValue)
		{
			return new OperateResult(content[21], GetMsgFromStatus(content[21]));
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 根据站号信息获取命令二次确认的报文信息
	/// </summary>
	/// <param name="station">站号信息</param>
	/// <returns>二次命令确认的报文</returns>
	public static byte[] GetExecuteConfirm(byte station)
	{
		byte[] array = new byte[6] { 16, 2, 0, 92, 94, 22 };
		array[1] = station;
		int num = 0;
		for (int i = 1; i < 4; i++)
		{
			num += array[i];
		}
		array[4] = (byte)num;
		return array;
	}
}
