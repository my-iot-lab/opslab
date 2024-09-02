using System.Text;
using Ops.Communication.Core;
using Ops.Communication.Core.Net;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 三菱串口协议的网络版
/// </summary>
/// <remarks>
/// 字读写地址支持的列表如下：
/// <list type="table">
///   <listheader>
///     <term>地址名称</term>
///     <term>地址代号</term>
///     <term>示例</term>
///     <term>地址范围</term>
///     <term>地址进制</term>
///     <term>备注</term>
///   </listheader>
///   <item>
///     <term>数据寄存器</term>
///     <term>D</term>
///     <term>D100,D200</term>
///     <term>D0-D511,D8000-D8255</term>
///     <term>10</term>
///     <term></term>
///   </item>
///   <item>
///     <term>定时器的值</term>
///     <term>TN</term>
///     <term>TN10,TN20</term>
///     <term>TN0-TN255</term>
///     <term>10</term>
///     <term></term>
///   </item>
///   <item>
///     <term>计数器的值</term>
///     <term>CN</term>
///     <term>CN10,CN20</term>
///     <term>CN0-CN199,CN200-CN255</term>
///     <term>10</term>
///     <term></term>
///   </item>
/// </list>
/// 位地址支持的列表如下：
/// <list type="table">
///   <listheader>
///     <term>地址名称</term>
///     <term>地址代号</term>
///     <term>示例</term>
///     <term>地址范围</term>
///     <term>地址进制</term>
///     <term>备注</term>
///   </listheader>
///   <item>
///     <term>内部继电器</term>
///     <term>M</term>
///     <term>M100,M200</term>
///     <term>M0-M1023,M8000-M8255</term>
///     <term>10</term>
///     <term></term>
///   </item>
///   <item>
///     <term>输入继电器</term>
///     <term>X</term>
///     <term>X1,X20</term>
///     <term>X0-X177</term>
///     <term>8</term>
///     <term></term>
///   </item>
///   <item>
///     <term>输出继电器</term>
///     <term>Y</term>
///     <term>Y10,Y20</term>
///     <term>Y0-Y177</term>
///     <term>8</term>
///     <term></term>
///   </item>
///   <item>
///     <term>步进继电器</term>
///     <term>S</term>
///     <term>S100,S200</term>
///     <term>S0-S999</term>
///     <term>10</term>
///     <term></term>
///   </item>
///   <item>
///     <term>定时器触点</term>
///     <term>TS</term>
///     <term>TS10,TS20</term>
///     <term>TS0-TS255</term>
///     <term>10</term>
///     <term></term>
///   </item>
///   <item>
///     <term>定时器线圈</term>
///     <term>TC</term>
///     <term>TC10,TC20</term>
///     <term>TC0-TC255</term>
///     <term>10</term>
///     <term></term>
///   </item>
///   <item>
///     <term>计数器触点</term>
///     <term>CS</term>
///     <term>CS10,CS20</term>
///     <term>CS0-CS255</term>
///     <term>10</term>
///     <term></term>
///   </item>
///   <item>
///     <term>计数器线圈</term>
///     <term>CC</term>
///     <term>CC10,CC20</term>
///     <term>CC0-CC255</term>
///     <term>10</term>
///     <term></term>
///   </item>
/// </list>
/// </remarks>
public sealed class MelsecFxSerialOverTcp : NetworkDeviceBase
{
	/// <summary>
	/// 当前的编程口协议是否为新版，默认为新版，如果无法读取，切换旧版再次尝试。
	/// </summary>
	public bool IsNewVersion { get; set; }

	/// <summary>
	/// 实例化网络版的三菱的串口协议的通讯对象。
	/// </summary>
	public MelsecFxSerialOverTcp()
	{
		base.WordLength = 1;
		base.ByteTransform = new RegularByteTransform();
		IsNewVersion = true;
		base.ByteTransform.IsStringReverseByteWord = true;
		base.SleepTime = 20;
	}

	/// <summary>
	/// 指定ip地址及端口号来实例化三菱的串口协议的通讯对象<br />
	/// Specify the IP address and port number to instantiate the communication object of Mitsubishi's serial protocol
	/// </summary>
	/// <param name="ipAddress">Ip地址</param>
	/// <param name="port">端口号</param>
	public MelsecFxSerialOverTcp(string ipAddress, int port)
		: this()
	{
		IpAddress = ipAddress;
		Port = port;
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		return ReadHelper(address, length, ReadFromCoreServer, IsNewVersion);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		return WriteHelper(address, value, ReadFromCoreServer, IsNewVersion);
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		OperateResult<byte[]> command = BuildReadWordCommand(address, length, IsNewVersion);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<byte[]>(command);
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult ackResult = CheckPlcReadResponse(read.Content);
		if (!ackResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(ackResult);
		}
		return ExtractActualData(read.Content);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		OperateResult<byte[]> command = BuildWriteWordCommand(address, value, IsNewVersion);
		if (!command.IsSuccess)
		{
			return command;
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult checkResult = CheckPlcWriteResponse(read.Content);
		if (!checkResult.IsSuccess)
		{
			return checkResult;
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 从三菱PLC中批量读取位软元件，返回读取结果，该读取地址最好从0，16，32...等开始读取，这样可以读取比较长得数据数组。
	/// </summary>
	/// <param name="address">起始地址</param>
	/// <param name="length">读取的长度</param>
	/// <returns>带成功标志的结果数据对象</returns>
	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		return ReadBoolHelper(address, length, ReadFromCoreServer);
	}

	public override OperateResult Write(string address, bool value)
	{
		return WriteHelper(address, value, ReadFromCoreServer);
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		OperateResult<byte[], int> command = BuildReadBoolCommand(address, length);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<bool[]>(command);
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content1).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read);
		}

		OperateResult ackResult = CheckPlcReadResponse(read.Content);
		if (!ackResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(ackResult);
		}
		return ExtractActualBoolData(read.Content, command.Content2, length);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool value)
	{
		OperateResult<byte[]> command = BuildWriteBoolPacket(address, value);
		if (!command.IsSuccess)
		{
			return command;
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult checkResult = CheckPlcWriteResponse(read.Content);
		if (!checkResult.IsSuccess)
		{
			return checkResult;
		}
		return OperateResult.Ok();
	}

	public override string ToString()
	{
		return $"MelsecFxSerialOverTcp[{IpAddress}:{Port}]";
	}

	/// <summary>
	/// 从三菱PLC中读取想要的数据，返回读取结果
	/// </summary>
	/// <param name="address">读取地址，，支持的类型参考文档说明</param>
	/// <param name="length">读取的数据长度</param>
	/// <param name="readCore">指定的通道信息</param>
	/// <param name="isNewVersion">是否是新版的串口访问类</param>
	/// <returns>带成功标志的结果数据对象</returns>
	public static OperateResult<byte[]> ReadHelper(string address, ushort length, Func<byte[], OperateResult<byte[]>> readCore, bool isNewVersion)
	{
		OperateResult<byte[]> operateResult = BuildReadWordCommand(address, length, isNewVersion);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		OperateResult<byte[]> operateResult2 = readCore(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckPlcReadResponse(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return ExtractActualData(operateResult2.Content);
	}

	/// <summary>
	/// 从三菱PLC中批量读取位软元件，返回读取结果，该读取地址最好从0，16，32...等开始读取，这样可以读取比较长得数据数组
	/// </summary>
	/// <param name="address">起始地址</param>
	/// <param name="length">读取的长度</param>
	/// <param name="readCore">指定的通道信息</param>
	/// <returns>带成功标志的结果数据对象</returns>
	public static OperateResult<bool[]> ReadBoolHelper(string address, ushort length, Func<byte[], OperateResult<byte[]>> readCore)
	{
		OperateResult<byte[], int> operateResult = BuildReadBoolCommand(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		OperateResult<byte[]> operateResult2 = readCore(operateResult.Content1);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckPlcReadResponse(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult3);
		}
		return ExtractActualBoolData(operateResult2.Content, operateResult.Content2, length);
	}

	/// <summary>
	/// 向PLC写入数据，数据格式为原始的字节类型
	/// </summary>
	/// <param name="address">初始地址，支持的类型参考文档说明</param>
	/// <param name="value">原始的字节数据</param>
	/// <param name="readCore">指定的通道信息</param>
	/// <param name="isNewVersion">是否是新版的串口访问类</param>
	/// <returns>是否写入成功的结果对象</returns>
	public static OperateResult WriteHelper(string address, byte[] value, Func<byte[], OperateResult<byte[]>> readCore, bool isNewVersion)
	{
		OperateResult<byte[]> operateResult = BuildWriteWordCommand(address, value, isNewVersion);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = readCore(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult operateResult3 = CheckPlcWriteResponse(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 强制写入位数据的通断，支持的类型参考文档说明
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="value">是否为通</param>
	/// <param name="readCore">指定的通道信息</param>
	/// <returns>是否写入成功的结果对象</returns>
	public static OperateResult WriteHelper(string address, bool value, Func<byte[], OperateResult<byte[]>> readCore)
	{
		OperateResult<byte[]> operateResult = BuildWriteBoolPacket(address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = readCore(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult operateResult3 = CheckPlcWriteResponse(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 检查PLC返回的读取数据是否是正常的
	/// </summary>
	/// <param name="ack">Plc反馈的数据信息</param>
	/// <returns>检查结果</returns>
	public static OperateResult CheckPlcReadResponse(byte[] ack)
	{
		if (ack.Length == 0)
		{
			return new OperateResult((int)OpsErrorCode.MelsecFxReceiveZero, OpsErrorCode.MelsecFxReceiveZero.Desc());
		}

		if (ack[0] == 21)
		{
			return new OperateResult((int)OpsErrorCode.MelsecFxAckNagative, $"{OpsErrorCode.MelsecFxAckNagative.Desc()} Actual: {SoftBasic.ByteToHexString(ack, ' ')}");
		}

		if (ack[0] != 2)
		{
			return new OperateResult((int)OpsErrorCode.MelsecFxAckWrong, $"{OpsErrorCode.MelsecFxAckWrong.Desc()} {ack[0]} Actual: {SoftBasic.ByteToHexString(ack, ' ')}");
		}

		if (!MelsecHelper.CheckCRC(ack))
		{
			return new OperateResult((int)OpsErrorCode.MelsecFxCrcCheckFailed, OpsErrorCode.MelsecFxCrcCheckFailed.Desc());
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 检查PLC返回的写入的数据是否是正常的
	/// </summary>
	/// <param name="ack">Plc反馈的数据信息</param>
	/// <returns>检查结果</returns>
	public static OperateResult CheckPlcWriteResponse(byte[] ack)
	{
		if (ack.Length == 0)
		{
			return new OperateResult((int)OpsErrorCode.MelsecFxReceiveZero, OpsErrorCode.MelsecFxReceiveZero.Desc());
		}

		if (ack[0] == 21)
		{
			return new OperateResult((int)OpsErrorCode.MelsecFxAckNagative, $"{OpsErrorCode.MelsecFxAckNagative.Desc()} Actual: {SoftBasic.ByteToHexString(ack, ' ')}");
		}

		if (ack[0] != 6)
		{
			return new OperateResult((int)OpsErrorCode.MelsecFxAckWrong, $"{OpsErrorCode.MelsecFxAckWrong.Desc()} {ack[0]} Actual: {SoftBasic.ByteToHexString(ack, ' ')}");
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 生成位写入的数据报文信息，该报文可直接用于发送串口给PLC
	/// </summary>
	/// <param name="address">地址信息，每个地址存在一定的范围，需要谨慎传入数据。举例：M10,S10,X5,Y10,C10,T10</param>
	/// <param name="value"><c>True</c>或是<c>False</c></param>
	/// <returns>带报文信息的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteBoolPacket(string address, bool value)
	{
		OperateResult<MelsecMcDataType, ushort> operateResult = FxAnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		ushort content = operateResult.Content2;
		if (operateResult.Content1 == MelsecMcDataType.M)
		{
			content = (content < 8000) ? ((ushort)(content + 2048)) : ((ushort)(content - 8000 + 3840));
		}
		else if (operateResult.Content1 == MelsecMcDataType.S)
		{
			//content = content;
		}
		else if (operateResult.Content1 == MelsecMcDataType.X)
		{
			content = (ushort)(content + 1024);
		}
		else if (operateResult.Content1 == MelsecMcDataType.Y)
		{
			content = (ushort)(content + 1280);
		}
		else if (operateResult.Content1 == MelsecMcDataType.CS)
		{
			content = (ushort)(content + 448);
		}
		else if (operateResult.Content1 == MelsecMcDataType.CC)
		{
			content = (ushort)(content + 960);
		}
		else if (operateResult.Content1 == MelsecMcDataType.CN)
		{
			content = (ushort)(content + 3584);
		}
		else if (operateResult.Content1 == MelsecMcDataType.TS)
		{
			content = (ushort)(content + 192);
		}
		else if (operateResult.Content1 == MelsecMcDataType.TC)
		{
			content = (ushort)(content + 704);
		}
		else
		{
			if (operateResult.Content1 != MelsecMcDataType.TN)
			{
				return new OperateResult<byte[]>((int)OpsErrorCode.MelsecCurrentTypeNotSupportedBitOperate, OpsErrorCode.MelsecCurrentTypeNotSupportedBitOperate.Desc());
			}
			content = (ushort)(content + 1536);
		}

		byte[] array = new byte[9]
		{
			2,
			(byte)(value ? 55 : 56),
			SoftBasic.BuildAsciiBytesFrom(content)[2],
			SoftBasic.BuildAsciiBytesFrom(content)[3],
			SoftBasic.BuildAsciiBytesFrom(content)[0],
			SoftBasic.BuildAsciiBytesFrom(content)[1],
			3,
			0,
			0
		};
		MelsecHelper.FxCalculateCRC(array).CopyTo(array, 7);
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 根据类型地址长度确认需要读取的指令头
	/// </summary>
	/// <param name="address">起始地址</param>
	/// <param name="length">长度</param>
	/// <param name="isNewVersion">是否是新版的串口访问类</param>
	/// <returns>带有成功标志的指令数据</returns>
	public static OperateResult<byte[]> BuildReadWordCommand(string address, ushort length, bool isNewVersion)
	{
		OperateResult<ushort> operateResult = FxCalculateWordStartAddress(address, isNewVersion);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		length = (ushort)(length * 2);
		ushort content = operateResult.Content;
		if (isNewVersion)
		{
			byte[] array = new byte[13]
			{
				2,
				69,
				48,
				48,
				SoftBasic.BuildAsciiBytesFrom(content)[0],
				SoftBasic.BuildAsciiBytesFrom(content)[1],
				SoftBasic.BuildAsciiBytesFrom(content)[2],
				SoftBasic.BuildAsciiBytesFrom(content)[3],
				SoftBasic.BuildAsciiBytesFrom((byte)length)[0],
				SoftBasic.BuildAsciiBytesFrom((byte)length)[1],
				3,
				0,
				0
			};
			MelsecHelper.FxCalculateCRC(array).CopyTo(array, 11);
			return OperateResult.Ok(array);
		}

		byte[] array2 = new byte[11]
		{
			2,
			48,
			SoftBasic.BuildAsciiBytesFrom(content)[0],
			SoftBasic.BuildAsciiBytesFrom(content)[1],
			SoftBasic.BuildAsciiBytesFrom(content)[2],
			SoftBasic.BuildAsciiBytesFrom(content)[3],
			SoftBasic.BuildAsciiBytesFrom((byte)length)[0],
			SoftBasic.BuildAsciiBytesFrom((byte)length)[1],
			3,
			0,
			0
		};
		MelsecHelper.FxCalculateCRC(array2).CopyTo(array2, 9);
		return OperateResult.Ok(array2);
	}

	/// <summary>
	/// 根据类型地址长度确认需要读取的指令头
	/// </summary>
	/// <param name="address">起始地址</param>
	/// <param name="length">bool数组长度</param>
	/// <returns>带有成功标志的指令数据</returns>
	public static OperateResult<byte[], int> BuildReadBoolCommand(string address, ushort length)
	{
		OperateResult<ushort, ushort, ushort> operateResult = FxCalculateBoolStartAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[], int>(operateResult);
		}

		ushort num = (ushort)((operateResult.Content2 + length - 1) / 8 - operateResult.Content2 / 8 + 1);
		ushort content = operateResult.Content1;
		byte[] array = new byte[11]
		{
			2,
			48,
			SoftBasic.BuildAsciiBytesFrom(content)[0],
			SoftBasic.BuildAsciiBytesFrom(content)[1],
			SoftBasic.BuildAsciiBytesFrom(content)[2],
			SoftBasic.BuildAsciiBytesFrom(content)[3],
			SoftBasic.BuildAsciiBytesFrom((byte)num)[0],
			SoftBasic.BuildAsciiBytesFrom((byte)num)[1],
			3,
			0,
			0
		};
		MelsecHelper.FxCalculateCRC(array).CopyTo(array, 9);
		return OperateResult.Ok(array, (int)operateResult.Content3);
	}

	/// <summary>
	/// 根据类型地址以及需要写入的数据来生成指令头
	/// </summary>
	/// <param name="address">起始地址</param>
	/// <param name="value">实际的数据信息</param>
	/// <param name="isNewVersion">是否是新版的串口访问类</param>
	/// <returns>带有成功标志的指令数据</returns>
	public static OperateResult<byte[]> BuildWriteWordCommand(string address, byte[] value, bool isNewVersion)
	{
		OperateResult<ushort> operateResult = FxCalculateWordStartAddress(address, isNewVersion);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		if (value != null)
		{
			value = SoftBasic.BuildAsciiBytesFrom(value);
		}

		ushort content = operateResult.Content;
		if (isNewVersion)
		{
			byte[] array = new byte[13 + value.Length];
			array[0] = 2;
			array[1] = 69;
			array[2] = 49;
			array[3] = 48;
			array[4] = SoftBasic.BuildAsciiBytesFrom(content)[0];
			array[5] = SoftBasic.BuildAsciiBytesFrom(content)[1];
			array[6] = SoftBasic.BuildAsciiBytesFrom(content)[2];
			array[7] = SoftBasic.BuildAsciiBytesFrom(content)[3];
			array[8] = SoftBasic.BuildAsciiBytesFrom((byte)(value.Length / 2))[0];
			array[9] = SoftBasic.BuildAsciiBytesFrom((byte)(value.Length / 2))[1];
			Array.Copy(value, 0, array, 10, value.Length);
			array[^3] = 3;
			MelsecHelper.FxCalculateCRC(array).CopyTo(array, array.Length - 2);
			return OperateResult.Ok(array);
		}

		byte[] array2 = new byte[11 + value.Length];
		array2[0] = 2;
		array2[1] = 49;
		array2[2] = SoftBasic.BuildAsciiBytesFrom(content)[0];
		array2[3] = SoftBasic.BuildAsciiBytesFrom(content)[1];
		array2[4] = SoftBasic.BuildAsciiBytesFrom(content)[2];
		array2[5] = SoftBasic.BuildAsciiBytesFrom(content)[3];
		array2[6] = SoftBasic.BuildAsciiBytesFrom((byte)(value.Length / 2))[0];
		array2[7] = SoftBasic.BuildAsciiBytesFrom((byte)(value.Length / 2))[1];
		Array.Copy(value, 0, array2, 8, value.Length);
		array2[^3] = 3;
		MelsecHelper.FxCalculateCRC(array2).CopyTo(array2, array2.Length - 2);
		return OperateResult.Ok(array2);
	}

	/// <summary>
	/// 从PLC反馈的数据进行提炼操作
	/// </summary>
	/// <param name="response">PLC反馈的真实数据</param>
	/// <returns>数据提炼后的真实数据</returns>
	public static OperateResult<byte[]> ExtractActualData(byte[] response)
	{
		try
		{
			byte[] array = new byte[(response.Length - 4) / 2];
			for (int i = 0; i < array.Length; i++)
			{
				byte[] bytes = new byte[2]
				{
					response[i * 2 + 1],
					response[i * 2 + 2]
				};
				array[i] = Convert.ToByte(Encoding.ASCII.GetString(bytes), 16);
			}
			return OperateResult.Ok(array);
		}
		catch (Exception ex)
		{
            OperateResult<byte[]> operateResult = new()
            {
                ErrorCode = (int)OpsErrorCode.MelsecError,
                Message = "Extract Msg：" + ex.Message + Environment.NewLine + "Data: " + SoftBasic.ByteToHexString(response)
            };
            return operateResult;
		}
	}

	/// <summary>
	/// 从PLC反馈的数据进行提炼bool数组操作
	/// </summary>
	/// <param name="response">PLC反馈的真实数据</param>
	/// <param name="start">起始提取的点信息</param>
	/// <param name="length">bool数组的长度</param>
	/// <returns>数据提炼后的真实数据</returns>
	public static OperateResult<bool[]> ExtractActualBoolData(byte[] response, int start, int length)
	{
		OperateResult<byte[]> operateResult = ExtractActualData(response);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		try
		{
			bool[] array = new bool[length];
			bool[] array2 = SoftBasic.ByteToBoolArray(operateResult.Content, operateResult.Content.Length * 8);
			for (int i = 0; i < length; i++)
			{
				array[i] = array2[i + start];
			}
			return OperateResult.Ok(array);
		}
		catch (Exception ex)
		{
			var operateResult2 = new OperateResult<bool[]>
			{
				ErrorCode = (int)OpsErrorCode.MelsecError,
				Message = "Extract Msg：" + ex.Message + Environment.NewLine + "Data: " + SoftBasic.ByteToHexString(response)
			};
			return operateResult2;
		}
	}

	/// <summary>
	/// 解析数据地址成不同的三菱地址类型
	/// </summary>
	/// <param name="address">数据地址</param>
	/// <returns>地址结果对象</returns>
	private static OperateResult<MelsecMcDataType, ushort> FxAnalysisAddress(string address)
	{
		var operateResult = new OperateResult<MelsecMcDataType, ushort>();
		try
		{
			switch (address[0])
			{
				case 'M' or 'm':
					operateResult.Content1 = MelsecMcDataType.M;
					operateResult.Content2 = Convert.ToUInt16(address[1..], MelsecMcDataType.M.FromBase);
					break;
				case 'X' or 'x':
					operateResult.Content1 = MelsecMcDataType.X;
					operateResult.Content2 = Convert.ToUInt16(address[1..], 8);
					break;
				case 'Y' or 'y':
					operateResult.Content1 = MelsecMcDataType.Y;
					operateResult.Content2 = Convert.ToUInt16(address[1..], 8);
					break;
				case 'D' or 'd':
					operateResult.Content1 = MelsecMcDataType.D;
					operateResult.Content2 = Convert.ToUInt16(address[1..], MelsecMcDataType.D.FromBase);
					break;
				case 'S' or 's':
					operateResult.Content1 = MelsecMcDataType.S;
					operateResult.Content2 = Convert.ToUInt16(address[1..], MelsecMcDataType.S.FromBase);
					break;
				case 'T' or 't':
					if (address[1] == 'N' || address[1] == 'n')
					{
						operateResult.Content1 = MelsecMcDataType.TN;
						operateResult.Content2 = Convert.ToUInt16(address[2..], MelsecMcDataType.TN.FromBase);
						break;
					}
					if (address[1] == 'S' || address[1] == 's')
					{
						operateResult.Content1 = MelsecMcDataType.TS;
						operateResult.Content2 = Convert.ToUInt16(address[2..], MelsecMcDataType.TS.FromBase);
						break;
					}
					if (address[1] == 'C' || address[1] == 'c')
					{
						operateResult.Content1 = MelsecMcDataType.TC;
						operateResult.Content2 = Convert.ToUInt16(address[2..], MelsecMcDataType.TC.FromBase);
						break;
					}
					throw new Exception(OpsErrorCode.NotSupportedDataType.Desc());
				case 'C' or 'c':
					if (address[1] == 'N' || address[1] == 'n')
					{
						operateResult.Content1 = MelsecMcDataType.CN;
						operateResult.Content2 = Convert.ToUInt16(address[2..], MelsecMcDataType.CN.FromBase);
						break;
					}
					if (address[1] == 'S' || address[1] == 's')
					{
						operateResult.Content1 = MelsecMcDataType.CS;
						operateResult.Content2 = Convert.ToUInt16(address[2..], MelsecMcDataType.CS.FromBase);
						break;
					}
					if (address[1] == 'C' || address[1] == 'c')
					{
						operateResult.Content1 = MelsecMcDataType.CC;
						operateResult.Content2 = Convert.ToUInt16(address[2..], MelsecMcDataType.CC.FromBase);
						break;
					}
					throw new Exception(OpsErrorCode.NotSupportedDataType.Desc());
				default:
					throw new Exception(OpsErrorCode.NotSupportedDataType.Desc());
			}
		}
		catch (Exception ex)
		{
			operateResult.ErrorCode = (int)OpsErrorCode.NotSupportedDataType;
            operateResult.Message = ex.Message;
			return operateResult;
		}

		operateResult.IsSuccess = true;
		return operateResult;
	}

	/// <summary>
	/// 返回读取的地址及长度信息
	/// </summary>
	/// <param name="address">读取的地址信息</param>
	/// <param name="isNewVersion">是否是新版的串口访问类</param>
	/// <returns>带起始地址的结果对象</returns>
	private static OperateResult<ushort> FxCalculateWordStartAddress(string address, bool isNewVersion)
	{
		OperateResult<MelsecMcDataType, ushort> operateResult = FxAnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<ushort>(operateResult);
		}

		ushort content = operateResult.Content2;
		if (operateResult.Content1 == MelsecMcDataType.D)
		{
			content = ((content < 8000) ? (isNewVersion ? ((ushort)(content * 2 + 16384)) : ((ushort)(content * 2 + 4096))) : ((ushort)((content - 8000) * 2 + 3584)));
		}
		else if (operateResult.Content1 == MelsecMcDataType.CN)
		{
			content = ((content < 200) ? ((ushort)(content * 2 + 2560)) : ((ushort)((content - 200) * 4 + 3072)));
		}
		else
		{
			if (operateResult.Content1 != MelsecMcDataType.TN)
			{
				return new OperateResult<ushort>((int)OpsErrorCode.MelsecCurrentTypeNotSupportedWordOperate, OpsErrorCode.MelsecCurrentTypeNotSupportedWordOperate.Desc());
			}
			content = (ushort)(content * 2 + 2048);
		}
		return OperateResult.Ok(content);
	}

	/// <summary>
	/// 返回读取的地址及长度信息，以及当前的偏置信息
	/// </summary><param name="address">读取的地址信息</param>
	/// <returns>带起始地址的结果对象</returns>
	private static OperateResult<ushort, ushort, ushort> FxCalculateBoolStartAddress(string address)
	{
		OperateResult<MelsecMcDataType, ushort> operateResult = FxAnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<ushort, ushort, ushort>(operateResult);
		}

		ushort content = operateResult.Content2;
		if (operateResult.Content1 == MelsecMcDataType.M)
		{
			content = ((content < 8000) ? ((ushort)(content / 8 + 256)) : ((ushort)((content - 8000) / 8 + 480)));
		}
		else if (operateResult.Content1 == MelsecMcDataType.X)
		{
			content = (ushort)(content / 8 + 128);
		}
		else if (operateResult.Content1 == MelsecMcDataType.Y)
		{
			content = (ushort)(content / 8 + 160);
		}
		else if (operateResult.Content1 == MelsecMcDataType.S)
		{
			content = (ushort)(content / 8);
		}
		else if (operateResult.Content1 == MelsecMcDataType.CS)
		{
			content = (ushort)(content / 8 + 448);
		}
		else if (operateResult.Content1 == MelsecMcDataType.CC)
		{
			content = (ushort)(content / 8 + 960);
		}
		else if (operateResult.Content1 == MelsecMcDataType.TS)
		{
			content = (ushort)(content / 8 + 192);
		}
		else
		{
			if (operateResult.Content1 != MelsecMcDataType.TC)
			{
				return new OperateResult<ushort, ushort, ushort>((int)OpsErrorCode.MelsecCurrentTypeNotSupportedBitOperate, OpsErrorCode.MelsecCurrentTypeNotSupportedBitOperate.Desc());
			}
			content = (ushort)(content / 8 + 704);
		}
		return OperateResult.Ok(content, operateResult.Content2, (ushort)(operateResult.Content2 % 8));
	}
}
