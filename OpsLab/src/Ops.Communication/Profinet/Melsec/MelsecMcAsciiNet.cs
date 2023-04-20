using System.Text;
using Ops.Communication.Address;
using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;
using Ops.Communication.Extensions;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯类，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为ASCII通讯格式。
/// </summary>
public class MelsecMcAsciiNet : NetworkDeviceBase
{
	public byte NetworkNumber { get; set; } = 0;

	public byte NetworkStationNumber { get; set; } = 0;

	public MelsecMcAsciiNet()
	{
		base.WordLength = 1;
		LogMsgFormatBinary = false;
		base.ByteTransform = new RegularByteTransform();
	}

	public MelsecMcAsciiNet(string ipAddress, int port)
	{
		base.WordLength = 1;
		IpAddress = ipAddress;
		Port = port;
		LogMsgFormatBinary = false;
		base.ByteTransform = new RegularByteTransform();
	}

	protected override INetMessage GetNewNetMessage()
	{
		return new MelsecQnA3EAsciiMessage();
	}

	protected virtual OperateResult<McAddressData> McAnalysisAddress(string address, ushort length)
	{
		return McAddressData.ParseMelsecFrom(address, length);
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		OperateResult<McAddressData> operateResult = McAnalysisAddress(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		var list = new List<byte>();
		ushort num = 0;
		while (num < length)
		{
			ushort num2 = (ushort)Math.Min(length - num, 450);
			operateResult.Content.Length = num2;
			OperateResult<byte[]> operateResult2 = ReadAddressData(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}

			list.AddRange(operateResult2.Content);
			num = (ushort)(num + num2);
			if (operateResult.Content.McDataType.DataType == 0)
			{
				operateResult.Content.AddressStart += num2;
			}
			else
			{
				operateResult.Content.AddressStart += num2 * 16;
			}
		}

		return OperateResult.Ok(list.ToArray());
	}

	private OperateResult<byte[]> ReadAddressData(McAddressData addressData)
	{
		byte[] mcCore = MelsecHelper.BuildAsciiReadMcCoreCommand(addressData, isBit: false);
		OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		OperateResult operateResult2 = CheckResponseContent(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}
		return ExtractActualData(operateResult.Content, isBit: false);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		OperateResult<McAddressData> operateResult = McAnalysisAddress(address, 0);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		byte[] mcCore = MelsecHelper.BuildAsciiWriteWordCoreCommand(operateResult.Content, value);
		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(addressResult);
		}

		var bytesContent = new List<byte>();
		ushort alreadyFinished = 0;
		while (alreadyFinished < length)
		{
			ushort readLength = (ushort)Math.Min(length - alreadyFinished, 450);
			addressResult.Content.Length = readLength;
			OperateResult<byte[]> read = await ReadAddressDataAsync(addressResult.Content).ConfigureAwait(false);
			if (!read.IsSuccess)
			{
				return read;
			}

			bytesContent.AddRange(read.Content);
			alreadyFinished = (ushort)(alreadyFinished + readLength);
			if (addressResult.Content.McDataType.DataType == 0)
			{
				addressResult.Content.AddressStart += readLength;
			}
			else
			{
				addressResult.Content.AddressStart += readLength * 16;
			}
		}
		return OperateResult.Ok(bytesContent.ToArray());
	}

	private async Task<OperateResult<byte[]>> ReadAddressDataAsync(McAddressData addressData)
	{
		byte[] coreResult = MelsecHelper.BuildAsciiReadMcCoreCommand(addressData, isBit: false);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content, isBit: false);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(addressResult);
		}

		byte[] coreResult = MelsecHelper.BuildAsciiWriteWordCoreCommand(addressResult.Content, value);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return check;
		}
		return OperateResult.Ok();
	}

	public OperateResult<byte[]> ReadRandom(string[] address)
	{
		McAddressData[] array = new McAddressData[address.Length];
		for (int i = 0; i < address.Length; i++)
		{
			OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address[i], 1);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult);
			}
			array[i] = operateResult.Content;
		}

		byte[] mcCore = MelsecHelper.BuildAsciiReadRandomWordCommand(array);
		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return ExtractActualData(operateResult2.Content, isBit: false);
	}

	public OperateResult<byte[]> ReadRandom(string[] address, ushort[] length)
	{
		if (length.Length != address.Length)
		{
			return new OperateResult<byte[]>(ErrorCode.TwoParametersLengthIsNotSame.Desc());
		}

		McAddressData[] array = new McAddressData[address.Length];
		for (int i = 0; i < address.Length; i++)
		{
			OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address[i], length[i]);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult);
			}
			array[i] = operateResult.Content;
		}

		byte[] mcCore = MelsecHelper.BuildAsciiReadRandomCommand(array);
		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return ExtractActualData(operateResult2.Content, isBit: false);
	}

	public OperateResult<short[]> ReadRandomInt16(string[] address)
	{
		OperateResult<byte[]> operateResult = ReadRandom(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<short[]>(operateResult);
		}
		return OperateResult.Ok(base.ByteTransform.TransInt16(operateResult.Content, 0, address.Length));
	}

	public async Task<OperateResult<byte[]>> ReadRandomAsync(string[] address)
	{
		McAddressData[] mcAddressDatas = new McAddressData[address.Length];
		for (int i = 0; i < address.Length; i++)
		{
			OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address[i], 1);
			if (!addressResult.IsSuccess)
			{
				return OperateResult.Error<byte[]>(addressResult);
			}
			mcAddressDatas[i] = addressResult.Content;
		}

		byte[] coreResult = MelsecHelper.BuildAsciiReadRandomWordCommand(mcAddressDatas);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content, isBit: false);
	}

	public async Task<OperateResult<byte[]>> ReadRandomAsync(string[] address, ushort[] length)
	{
		if (length.Length != address.Length)
		{
			return new OperateResult<byte[]>(ErrorCode.TwoParametersLengthIsNotSame.Desc());
		}

		McAddressData[] mcAddressDatas = new McAddressData[address.Length];
		for (int i = 0; i < address.Length; i++)
		{
			OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address[i], length[i]);
			if (!addressResult.IsSuccess)
			{
				return OperateResult.Error<byte[]>(addressResult);
			}
			mcAddressDatas[i] = addressResult.Content;
		}

		byte[] coreResult = MelsecHelper.BuildAsciiReadRandomCommand(mcAddressDatas);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content, isBit: false);
	}

	public async Task<OperateResult<short[]>> ReadRandomInt16Async(string[] address)
	{
		OperateResult<byte[]> read = await ReadRandomAsync(address).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<short[]>(read);
		}
		return OperateResult.Ok(base.ByteTransform.TransInt16(read.Content, 0, address.Length));
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		OperateResult<McAddressData> operateResult = McAnalysisAddress(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		byte[] mcCore = MelsecHelper.BuildAsciiReadMcCoreCommand(operateResult.Content, isBit: true);
		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
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
		OperateResult<McAddressData> operateResult = McAnalysisAddress(address, 0);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		byte[] mcCore = MelsecHelper.BuildAsciiWriteBitCoreCommand(operateResult.Content, values);
		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(addressResult);
		}

		byte[] coreResult = MelsecHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, isBit: true);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
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
		OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
		if (!addressResult.IsSuccess)
		{
			return addressResult;
		}

		byte[] coreResult = MelsecHelper.BuildAsciiWriteBitCoreCommand(addressResult.Content, values);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return check;
		}
		return OperateResult.Ok();
	}

	public OperateResult<byte[]> ReadMemory(string address, ushort length)
	{
		OperateResult<byte[]> operateResult = MelsecHelper.BuildAsciiReadMemoryCommand(address, length);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackMcCommand(operateResult.Content, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return ExtractActualData(operateResult2.Content, isBit: false);
	}

	public async Task<OperateResult<byte[]>> ReadMemoryAsync(string address, ushort length)
	{
		OperateResult<byte[]> coreResult = MelsecHelper.BuildAsciiReadMemoryCommand(address, length);
		if (!coreResult.IsSuccess)
		{
			return coreResult;
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult.Content, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content, isBit: false);
	}

	public OperateResult<byte[]> ReadSmartModule(ushort module, string address, ushort length)
	{
		OperateResult<byte[]> operateResult = MelsecHelper.BuildAsciiReadSmartModule(module, address, length);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackMcCommand(operateResult.Content, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return ExtractActualData(operateResult2.Content, isBit: false);
	}

	public async Task<OperateResult<byte[]>> ReadSmartModuleAsync(ushort module, string address, ushort length)
	{
		OperateResult<byte[]> coreResult = MelsecHelper.BuildAsciiReadSmartModule(module, address, length);
		if (!coreResult.IsSuccess)
		{
			return coreResult;
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult.Content, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content, isBit: false);
	}

	public OperateResult RemoteRun()
	{
		OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("1001000000010000"), NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = CheckResponseContent(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return OperateResult.Ok();
	}

	public OperateResult RemoteStop()
	{
		OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("100200000001"), NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = CheckResponseContent(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return OperateResult.Ok();
	}

	public OperateResult RemoteReset()
	{
		OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("100600000001"), NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = CheckResponseContent(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return OperateResult.Ok();
	}

	public OperateResult<string> ReadPlcType()
	{
		OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult);
		}

		OperateResult operateResult2 = CheckResponseContent(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult2);
		}
		return OperateResult.Ok(Encoding.ASCII.GetString(operateResult.Content, 22, 16).TrimEnd(Array.Empty<char>()));
	}

	public OperateResult ErrorStateReset()
	{
		OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = CheckResponseContent(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return OperateResult.Ok();
	}

	public async Task<OperateResult> RemoteRunAsync()
	{
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(Encoding.ASCII.GetBytes("1001000000010000"), NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return check;
		}
		return OperateResult.Ok();
	}

	public async Task<OperateResult> RemoteStopAsync()
	{
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(Encoding.ASCII.GetBytes("100200000001"), NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return check;
		}
		return OperateResult.Ok();
	}

	public async Task<OperateResult> RemoteResetAsync()
	{
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(Encoding.ASCII.GetBytes("100600000001"), NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return check;
		}
		return OperateResult.Ok();
	}

	public async Task<OperateResult<string>> ReadPlcTypeAsync()
	{
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<string>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<string>(check);
		}
		return OperateResult.Ok(Encoding.ASCII.GetString(read.Content, 22, 16).TrimEnd(Array.Empty<char>()));
	}

	public async Task<OperateResult> ErrorStateResetAsync()
	{
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return check;
		}
		return OperateResult.Ok();
	}

	public override string ToString()
	{
		return $"MelsecMcAsciiNet[{IpAddress}:{Port}]";
	}

	/// <summary>
	/// 将MC协议的核心报文打包成一个可以直接对PLC进行发送的原始报文
	/// </summary>
	/// <param name="mcCore">MC协议的核心报文</param>
	/// <param name="networkNumber">网络号</param>
	/// <param name="networkStationNumber">网络站号</param>
	/// <returns>原始报文信息</returns>
	public static byte[] PackMcCommand(byte[] mcCore, byte networkNumber = 0, byte networkStationNumber = 0)
	{
		byte[] array = new byte[22 + mcCore.Length];
		array[0] = 53;
		array[1] = 48;
		array[2] = 48;
		array[3] = 48;
		array[4] = SoftBasic.BuildAsciiBytesFrom(networkNumber)[0];
		array[5] = SoftBasic.BuildAsciiBytesFrom(networkNumber)[1];
		array[6] = 70;
		array[7] = 70;
		array[8] = 48;
		array[9] = 51;
		array[10] = 70;
		array[11] = 70;
		array[12] = SoftBasic.BuildAsciiBytesFrom(networkStationNumber)[0];
		array[13] = SoftBasic.BuildAsciiBytesFrom(networkStationNumber)[1];
		array[14] = SoftBasic.BuildAsciiBytesFrom((ushort)(array.Length - 18))[0];
		array[15] = SoftBasic.BuildAsciiBytesFrom((ushort)(array.Length - 18))[1];
		array[16] = SoftBasic.BuildAsciiBytesFrom((ushort)(array.Length - 18))[2];
		array[17] = SoftBasic.BuildAsciiBytesFrom((ushort)(array.Length - 18))[3];
		array[18] = 48;
		array[19] = 48;
		array[20] = 49;
		array[21] = 48;
		mcCore.CopyTo(array, 22);
		return array;
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
			return OperateResult.Ok((from m in response.RemoveBegin(22)
									 select (byte)((m != 48) ? 1 : 0)).ToArray());
		}
		return OperateResult.Ok(MelsecHelper.TransAsciiByteArrayToByteArray(response.RemoveBegin(22)));
	}

	/// <summary>
	/// 检查反馈的内容是否正确的
	/// </summary>
	/// <param name="content">MC的反馈的内容</param>
	/// <returns>是否正确</returns>
	public static OperateResult CheckResponseContent(byte[] content)
	{
		ushort num = Convert.ToUInt16(Encoding.ASCII.GetString(content, 18, 4), 16);
		if (num != 0)
		{
			return new OperateResult(num, MelsecHelper.GetErrorDescription(num));
		}
		return OperateResult.Ok();
	}
}
