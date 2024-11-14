using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;
using Ops.Communication.Extensions;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯协议，采用A兼容1E帧协议实现，使用ASCII码通讯，请根据实际型号来进行选取。
/// </summary>
public sealed class MelsecA1EAsciiNet : NetworkDeviceBase
{
	public byte PLCNumber { get; set; } = byte.MaxValue;

	/// <summary>
	/// 实例化一个默认的对象。
	/// </summary>
	public MelsecA1EAsciiNet()
	{
		base.WordLength = 1;
		LogMsgFormatBinary = false;
		base.ByteTransform = new RegularByteTransform();
	}

	/// <summary>
	/// 指定ip地址和端口来来实例化一个默认的对象。
	/// </summary>
	/// <param name="ipAddress">PLC的Ip地址</param>
	/// <param name="port">PLC的端口</param>
	public MelsecA1EAsciiNet(string ipAddress, int port)
	{
		base.WordLength = 1;
		IpAddress = ipAddress;
		Port = port;
		LogMsgFormatBinary = false;
		base.ByteTransform = new RegularByteTransform();
	}

	protected override INetMessage GetNewNetMessage()
	{
		return new MelsecA1EAsciiMessage();
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		OperateResult<byte[]> operateResult = BuildReadCommand(address, length, isBit: false, PLCNumber);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckResponseLegal(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}

		return ExtractActualData(operateResult2.Content, isBit: false);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		OperateResult<byte[]> operateResult = BuildWriteWordCommand(address, value, PLCNumber);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult operateResult3 = CheckResponseLegal(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		OperateResult<byte[]> command = BuildReadCommand(address, length, isBit: false, PLCNumber);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<byte[]>(command);
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseLegal(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content, isBit: false);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		OperateResult<byte[]> command = BuildWriteWordCommand(address, value, PLCNumber);
		if (!command.IsSuccess)
		{
			return command;
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = CheckResponseLegal(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return OperateResult.Ok();
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		OperateResult<byte[]> operateResult = BuildReadCommand(address, length, isBit: true, PLCNumber);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckResponseLegal(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult3);
		}

		OperateResult<byte[]> operateResult4 = ExtractActualData(operateResult2.Content, isBit: true);
		if (!operateResult4.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult4);
		}
		return OperateResult.Ok(operateResult4.Content.Select((byte m) => m == 1).Take(length).ToArray());
	}

	public override OperateResult Write(string address, bool[] values)
	{
		OperateResult<byte[]> operateResult = BuildWriteBoolCommand(address, values, PLCNumber);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return CheckResponseLegal(operateResult2.Content);
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		OperateResult<byte[]> command = BuildReadCommand(address, length, isBit: true, PLCNumber);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<bool[]>(command);
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read);
		}

		OperateResult check = CheckResponseLegal(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<bool[]>(check);
		}

		OperateResult<byte[]> extract = ExtractActualData(read.Content, isBit: true);
		if (!extract.IsSuccess)
		{
			return OperateResult.Error<bool[]>(extract);
		}
		return OperateResult.Ok(extract.Content.Select((byte m) => m == 1).Take(length).ToArray());
	}

	public override async Task<OperateResult> WriteAsync(string address, bool[] values)
	{
		OperateResult<byte[]> command = BuildWriteBoolCommand(address, values, PLCNumber);
		if (!command.IsSuccess)
		{
			return command;
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}
		return CheckResponseLegal(read.Content);
	}

	public override string ToString()
	{
		return $"MelsecA1ENet[{IpAddress}:{Port}]";
	}

	/// <summary>
	/// 根据类型地址长度确认需要读取的指令头
	/// </summary>
	/// <param name="address">起始地址</param>
	/// <param name="length">长度</param>
	/// <param name="isBit">指示是否按照位成批的读出</param>
	/// <param name="plcNumber">PLC编号</param>
	/// <returns>带有成功标志的指令数据</returns>
	public static OperateResult<byte[]> BuildReadCommand(string address, ushort length, bool isBit, byte plcNumber)
	{
		OperateResult<MelsecA1EDataType, int> operateResult = MelsecHelper.McA1EAnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		byte value = (byte)((!isBit) ? 1 : 0);
		return OperateResult.Ok(new byte[24]
		{
			SoftBasic.BuildAsciiBytesFrom(value)[0],
			SoftBasic.BuildAsciiBytesFrom(value)[1],
			SoftBasic.BuildAsciiBytesFrom(plcNumber)[0],
			SoftBasic.BuildAsciiBytesFrom(plcNumber)[1],
			48,
			48,
			48,
			65,
			SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[0])[0],
			SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[0])[1],
			SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[1])[0],
			SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[1])[1],
			SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[0],
			SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[1],
			SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[0],
			SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[1],
			SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[0],
			SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[1],
			SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[0],
			SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[1],
			SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes((int)length % 256)[0])[0],
			SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes((int)length % 256)[0])[1],
			48,
			48
		});
	}

	/// <summary>
	/// 根据类型地址以及需要写入的数据来生成指令头
	/// </summary>
	/// <param name="address">起始地址</param>
	/// <param name="value">数据值</param>
	/// <param name="plcNumber">PLC编号</param>
	/// <returns>带有成功标志的指令数据</returns>
	public static OperateResult<byte[]> BuildWriteWordCommand(string address, byte[] value, byte plcNumber)
	{
		OperateResult<MelsecA1EDataType, int> operateResult = MelsecHelper.McA1EAnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		value = MelsecHelper.TransByteArrayToAsciiByteArray(value);
		byte[] array = new byte[24 + value.Length];
		array[0] = 48;
		array[1] = 51;
		array[2] = SoftBasic.BuildAsciiBytesFrom(plcNumber)[0];
		array[3] = SoftBasic.BuildAsciiBytesFrom(plcNumber)[1];
		array[4] = 48;
		array[5] = 48;
		array[6] = 48;
		array[7] = 65;
		array[8] = SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[0])[0];
		array[9] = SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[0])[1];
		array[10] = SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[1])[0];
		array[11] = SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[1])[1];
		array[12] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[0];
		array[13] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[1];
		array[14] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[0];
		array[15] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[1];
		array[16] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[0];
		array[17] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[1];
		array[18] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[0];
		array[19] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[1];
		array[20] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(value.Length / 4)[0])[0];
		array[21] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(value.Length / 4)[0])[1];
		array[22] = 48;
		array[23] = 48;
		value.CopyTo(array, 24);
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 根据类型地址以及需要写入的数据来生成指令头
	/// </summary>
	/// <param name="address">起始地址</param>
	/// <param name="value">数据值</param>
	/// <param name="plcNumber">PLC编号</param>
	/// <returns>带有成功标志的指令数据</returns>
	public static OperateResult<byte[]> BuildWriteBoolCommand(string address, bool[] value, byte plcNumber)
	{
		OperateResult<MelsecA1EDataType, int> operateResult = MelsecHelper.McA1EAnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		byte[] array = value.Select((bool m) => (byte)(m ? 49 : 48)).ToArray();
		if (array.Length % 2 == 1)
		{
			array = SoftBasic.SpliceArray(array, new byte[1] { 48 });
		}

		byte[] array2 = new byte[24 + array.Length];
		array2[0] = 48;
		array2[1] = 50;
		array2[2] = SoftBasic.BuildAsciiBytesFrom(plcNumber)[0];
		array2[3] = SoftBasic.BuildAsciiBytesFrom(plcNumber)[1];
		array2[4] = 48;
		array2[5] = 48;
		array2[6] = 48;
		array2[7] = 65;
		array2[8] = SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[0])[0];
		array2[9] = SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[0])[1];
		array2[10] = SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[1])[0];
		array2[11] = SoftBasic.BuildAsciiBytesFrom(operateResult.Content1.DataCode[1])[1];
		array2[12] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[0];
		array2[13] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[1];
		array2[14] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[0];
		array2[15] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[1];
		array2[16] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[0];
		array2[17] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[1];
		array2[18] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[0];
		array2[19] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[1];
		array2[20] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(value.Length)[0])[0];
		array2[21] = SoftBasic.BuildAsciiBytesFrom(BitConverter.GetBytes(value.Length)[0])[1];
		array2[22] = 48;
		array2[23] = 48;
		array.CopyTo(array2, 24);
		return OperateResult.Ok(array2);
	}

	/// <summary>
	/// 检测反馈的消息是否合法
	/// </summary>
	/// <param name="response">接收的报文</param>
	/// <returns>是否成功</returns>
	public static OperateResult CheckResponseLegal(byte[] response)
	{
		if (response.Length < 4)
		{
			return new OperateResult((int)ConnErrorCode.MelsecPleaseReferToManualDocument, ConnErrorCode.ReceiveDataLengthTooShort.Desc());
		}
		if (response[2] == 48 && response[3] == 48)
		{
			return OperateResult.Ok();
		}
		if (response[2] == 53 && response[3] == 66)
		{
			return new OperateResult((int)ConnErrorCode.MelsecPleaseReferToManualDocument, ConnErrorCode.MelsecPleaseReferToManualDocument.Desc());
		}
		return new OperateResult((int)ConnErrorCode.MelsecPleaseReferToManualDocument, ConnErrorCode.MelsecPleaseReferToManualDocument.Desc());
	}

	/// <summary>
	/// 从PLC反馈的数据中提取出实际的数据内容，需要传入反馈数据，是否位读取
	/// </summary>
	/// <param name="response">反馈的数据内容</param>
	/// <param name="isBit">是否位读取</param>
	/// <returns>解析后的结果对象</returns>
	public static OperateResult<byte[]> ExtractActualData(byte[] response, bool isBit)
	{
		if (isBit)
		{
			return OperateResult.Ok((from m in response.RemoveBegin(4)
									 select (byte)((m != 48) ? 1 : 0)).ToArray());
		}
		return OperateResult.Ok(MelsecHelper.TransAsciiByteArrayToByteArray(response.RemoveBegin(4)));
	}
}
