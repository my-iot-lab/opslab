using Ops.Communication.Address;
using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;
using Ops.Communication.Extensions;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 三菱的R系列的MC协议，支持的地址类型和 <see cref="MelsecMcNet" /> 有区别，详细请查看对应的API文档说明
/// </summary>
public sealed class MelsecMcRNet : NetworkDeviceBase
{
	public byte NetworkNumber { get; set; } = 0;

	public byte NetworkStationNumber { get; set; } = 0;


	/// <summary>
	/// 实例化三菱R系列的Qna兼容3E帧协议的通讯对象。
	/// </summary>
	public MelsecMcRNet()
	{
		base.WordLength = 1;
		base.ByteTransform = new RegularByteTransform();
	}

	/// <summary>
	/// 指定ip地址和端口号来实例化一个默认的对象。
	/// </summary>
	/// <param name="ipAddress">PLC的Ip地址</param>
	/// <param name="port">PLC的端口</param>
	public MelsecMcRNet(string ipAddress, int port)
	{
		base.WordLength = 1;
		IpAddress = ipAddress;
		Port = port;
		base.ByteTransform = new RegularByteTransform();
	}

	protected override INetMessage GetNewNetMessage()
	{
		return new MelsecQnA3EBinaryMessage();
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		OperateResult<McRAddressData> operateResult = McRAddressData.ParseMelsecRFrom(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		var list = new List<byte>();
		ushort num = 0;
		while (num < length)
		{
			ushort num2 = (ushort)Math.Min(length - num, 900);
			operateResult.Content.Length = num2;
			OperateResult<byte[]> operateResult2 = ReadAddressData(operateResult.Content, isBit: false);
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

	private OperateResult<byte[]> ReadAddressData(McRAddressData address, bool isBit)
	{
		byte[] mcCore = BuildReadMcCoreCommand(address, isBit);
		OperateResult<byte[]> operateResult = ReadFromCoreServer(MelsecMcNet.PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		OperateResult operateResult2 = MelsecMcNet.CheckResponseContentHelper(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}
		return MelsecMcNet.ExtractActualData(operateResult.Content.RemoveBegin(11), isBit);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		OperateResult<McRAddressData> operateResult = McRAddressData.ParseMelsecRFrom(address, 0);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}
		return WriteAddressData(operateResult.Content, value);
	}

	private OperateResult WriteAddressData(McRAddressData addressData, byte[] value)
	{
		byte[] mcCore = BuildWriteWordCoreCommand(addressData, value);
		var operateResult = ReadFromCoreServer(MelsecMcNet.PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = MelsecMcNet.CheckResponseContentHelper(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, length);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(addressResult);
		}

		var bytesContent = new List<byte>();
		ushort alreadyFinished = 0;
		while (alreadyFinished < length)
		{
			ushort readLength = (ushort)Math.Min(length - alreadyFinished, 900);
			addressResult.Content.Length = readLength;
			OperateResult<byte[]> read = await ReadAddressDataAsync(addressResult.Content, isBit: false).ConfigureAwait(false);
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

	private async Task<OperateResult<byte[]>> ReadAddressDataAsync(McRAddressData address, bool isBit)
	{
		byte[] coreResult = BuildReadMcCoreCommand(address, isBit);
		var read = await ReadFromCoreServerAsync(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = MelsecMcNet.CheckResponseContentHelper(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}

		return MelsecMcNet.ExtractActualData(read.Content.RemoveBegin(11), isBit);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, 0);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(addressResult);
		}
		return await WriteAddressDataAsync(addressResult.Content, value).ConfigureAwait(false);
	}

	private async Task<OperateResult> WriteAddressDataAsync(McRAddressData addressData, byte[] value)
	{
		byte[] coreResult = BuildWriteWordCoreCommand(addressData, value);
		var read = await ReadFromCoreServerAsync(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = MelsecMcNet.CheckResponseContentHelper(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return OperateResult.Ok();
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		var operateResult = McRAddressData.ParseMelsecRFrom(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		byte[] mcCore = BuildReadMcCoreCommand(operateResult.Content, isBit: true);
		var operateResult2 = ReadFromCoreServer(MelsecMcNet.PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult2);
		}

		OperateResult operateResult3 = MelsecMcNet.CheckResponseContentHelper(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult3);
		}

		var operateResult4 = MelsecMcNet.ExtractActualData(operateResult2.Content.RemoveBegin(11), isBit: true);
		if (!operateResult4.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult4);
		}
		return OperateResult.Ok(operateResult4.Content.Select((byte m) => m == 1).Take(length).ToArray());
	}

	public override OperateResult Write(string address, bool[] values)
	{
		OperateResult<McRAddressData> operateResult = McRAddressData.ParseMelsecRFrom(address, 0);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		byte[] mcCore = BuildWriteBitCoreCommand(operateResult.Content, values);
		var operateResult2 = ReadFromCoreServer(MelsecMcNet.PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult operateResult3 = MelsecMcNet.CheckResponseContentHelper(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, length);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(addressResult);
		}

		byte[] coreResult = BuildReadMcCoreCommand(addressResult.Content, isBit: true);
		var read = await ReadFromCoreServerAsync(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read);
		}

		OperateResult check = MelsecMcNet.CheckResponseContentHelper(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<bool[]>(check);
		}

		var extract = MelsecMcNet.ExtractActualData(read.Content.RemoveBegin(11), isBit: true);
		if (!extract.IsSuccess)
		{
			return OperateResult.Error<bool[]>(extract);
		}
		return OperateResult.Ok(extract.Content.Select((byte m) => m == 1).Take(length).ToArray());
	}

	public override async Task<OperateResult> WriteAsync(string address, bool[] values)
	{
		OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, 0);
		if (!addressResult.IsSuccess)
		{
			return addressResult;
		}

		byte[] coreResult = BuildWriteBitCoreCommand(addressResult.Content, values);
		var read = await ReadFromCoreServerAsync(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = MelsecMcNet.CheckResponseContentHelper(read.Content);
		if (!check.IsSuccess)
		{
			return check;
		}
		return OperateResult.Ok();
	}

	public override string ToString()
	{
		return $"MelsecMcRNet[{IpAddress}:{Port}]";
	}

	/// <summary>
	/// 分析三菱R系列的地址，并返回解析后的数据对象
	/// </summary>
	/// <param name="address">字符串地址</param>
	/// <returns>是否解析成功</returns>
	public static OperateResult<MelsecMcRDataType, int> AnalysisAddress(string address)
	{
		try
		{
			if (address.StartsWith("LSTS"))
			{
				return OperateResult.Ok(MelsecMcRDataType.LSTS, Convert.ToInt32(address[4..], MelsecMcRDataType.LSTS.FromBase));
			}
			if (address.StartsWith("LSTC"))
			{
				return OperateResult.Ok(MelsecMcRDataType.LSTC, Convert.ToInt32(address[4..], MelsecMcRDataType.LSTC.FromBase));
			}
			if (address.StartsWith("LSTN"))
			{
				return OperateResult.Ok(MelsecMcRDataType.LSTN, Convert.ToInt32(address[4..], MelsecMcRDataType.LSTN.FromBase));
			}
			if (address.StartsWith("STS"))
			{
				return OperateResult.Ok(MelsecMcRDataType.STS, Convert.ToInt32(address[3..], MelsecMcRDataType.STS.FromBase));
			}
			if (address.StartsWith("STC"))
			{
				return OperateResult.Ok(MelsecMcRDataType.STC, Convert.ToInt32(address[3..], MelsecMcRDataType.STC.FromBase));
			}
			if (address.StartsWith("STN"))
			{
				return OperateResult.Ok(MelsecMcRDataType.STN, Convert.ToInt32(address[3..], MelsecMcRDataType.STN.FromBase));
			}
			if (address.StartsWith("LTS"))
			{
				return OperateResult.Ok(MelsecMcRDataType.LTS, Convert.ToInt32(address[3..], MelsecMcRDataType.LTS.FromBase));
			}
			if (address.StartsWith("LTC"))
			{
				return OperateResult.Ok(MelsecMcRDataType.LTC, Convert.ToInt32(address[3..], MelsecMcRDataType.LTC.FromBase));
			}
			if (address.StartsWith("LTN"))
			{
				return OperateResult.Ok(MelsecMcRDataType.LTN, Convert.ToInt32(address[3..], MelsecMcRDataType.LTN.FromBase));
			}
			if (address.StartsWith("LCS"))
			{
				return OperateResult.Ok(MelsecMcRDataType.LCS, Convert.ToInt32(address[3..], MelsecMcRDataType.LCS.FromBase));
			}
			if (address.StartsWith("LCC"))
			{
				return OperateResult.Ok(MelsecMcRDataType.LCC, Convert.ToInt32(address[3..], MelsecMcRDataType.LCC.FromBase));
			}
			if (address.StartsWith("LCN"))
			{
				return OperateResult.Ok(MelsecMcRDataType.LCN, Convert.ToInt32(address[3..], MelsecMcRDataType.LCN.FromBase));
			}
			if (address.StartsWith("TS"))
			{
				return OperateResult.Ok(MelsecMcRDataType.TS, Convert.ToInt32(address[2..], MelsecMcRDataType.TS.FromBase));
			}
			if (address.StartsWith("TC"))
			{
				return OperateResult.Ok(MelsecMcRDataType.TC, Convert.ToInt32(address[2..], MelsecMcRDataType.TC.FromBase));
			}
			if (address.StartsWith("TN"))
			{
				return OperateResult.Ok(MelsecMcRDataType.TN, Convert.ToInt32(address[2..], MelsecMcRDataType.TN.FromBase));
			}
			if (address.StartsWith("CS"))
			{
				return OperateResult.Ok(MelsecMcRDataType.CS, Convert.ToInt32(address[2..], MelsecMcRDataType.CS.FromBase));
			}
			if (address.StartsWith("CC"))
			{
				return OperateResult.Ok(MelsecMcRDataType.CC, Convert.ToInt32(address[2..], MelsecMcRDataType.CC.FromBase));
			}
			if (address.StartsWith("CN"))
			{
				return OperateResult.Ok(MelsecMcRDataType.CN, Convert.ToInt32(address[2..], MelsecMcRDataType.CN.FromBase));
			}
			if (address.StartsWith("SM"))
			{
				return OperateResult.Ok(MelsecMcRDataType.SM, Convert.ToInt32(address[2..], MelsecMcRDataType.SM.FromBase));
			}
			if (address.StartsWith("SB"))
			{
				return OperateResult.Ok(MelsecMcRDataType.SB, Convert.ToInt32(address[2..], MelsecMcRDataType.SB.FromBase));
			}
			if (address.StartsWith("DX"))
			{
				return OperateResult.Ok(MelsecMcRDataType.DX, Convert.ToInt32(address[2..], MelsecMcRDataType.DX.FromBase));
			}
			if (address.StartsWith("DY"))
			{
				return OperateResult.Ok(MelsecMcRDataType.DY, Convert.ToInt32(address[2..], MelsecMcRDataType.DY.FromBase));
			}
			if (address.StartsWith("SD"))
			{
				return OperateResult.Ok(MelsecMcRDataType.SD, Convert.ToInt32(address[2..], MelsecMcRDataType.SD.FromBase));
			}
			if (address.StartsWith("SW"))
			{
				return OperateResult.Ok(MelsecMcRDataType.SW, Convert.ToInt32(address[2..], MelsecMcRDataType.SW.FromBase));
			}
			if (address.StartsWith("X"))
			{
				return OperateResult.Ok(MelsecMcRDataType.X, Convert.ToInt32(address[1..], MelsecMcRDataType.X.FromBase));
			}
			if (address.StartsWith("Y"))
			{
				return OperateResult.Ok(MelsecMcRDataType.Y, Convert.ToInt32(address[1..], MelsecMcRDataType.Y.FromBase));
			}
			if (address.StartsWith("M"))
			{
				return OperateResult.Ok(MelsecMcRDataType.M, Convert.ToInt32(address[1..], MelsecMcRDataType.M.FromBase));
			}
			if (address.StartsWith("L"))
			{
				return OperateResult.Ok(MelsecMcRDataType.L, Convert.ToInt32(address[1..], MelsecMcRDataType.L.FromBase));
			}
			if (address.StartsWith("F"))
			{
				return OperateResult.Ok(MelsecMcRDataType.F, Convert.ToInt32(address[1..], MelsecMcRDataType.F.FromBase));
			}
			if (address.StartsWith("V"))
			{
				return OperateResult.Ok(MelsecMcRDataType.V, Convert.ToInt32(address[1..], MelsecMcRDataType.V.FromBase));
			}
			if (address.StartsWith("S"))
			{
				return OperateResult.Ok(MelsecMcRDataType.S, Convert.ToInt32(address[1..], MelsecMcRDataType.S.FromBase));
			}
			if (address.StartsWith("B"))
			{
				return OperateResult.Ok(MelsecMcRDataType.B, Convert.ToInt32(address[1..], MelsecMcRDataType.B.FromBase));
			}
			if (address.StartsWith("D"))
			{
				return OperateResult.Ok(MelsecMcRDataType.D, Convert.ToInt32(address[1..], MelsecMcRDataType.D.FromBase));
			}
			if (address.StartsWith("W"))
			{
				return OperateResult.Ok(MelsecMcRDataType.W, Convert.ToInt32(address[1..], MelsecMcRDataType.W.FromBase));
			}
			if (address.StartsWith("R"))
			{
				return OperateResult.Ok(MelsecMcRDataType.R, Convert.ToInt32(address[1..], MelsecMcRDataType.R.FromBase));
			}
			if (address.StartsWith("Z"))
			{
				return OperateResult.Ok(MelsecMcRDataType.Z, Convert.ToInt32(address[1..], MelsecMcRDataType.Z.FromBase));
			}
			return new OperateResult<MelsecMcRDataType, int>((int)ErrorCode.NotSupportedDataType, ErrorCode.NotSupportedDataType.Desc());
		}
		catch (Exception ex)
		{
			return new OperateResult<MelsecMcRDataType, int>((int)ErrorCode.NotSupportedDataType, ex.Message);
		}
	}

	/// <summary>
	/// 从三菱地址，是否位读取进行创建读取的MC的核心报文
	/// </summary>
	/// <param name="address">地址数据</param>
	/// <param name="isBit">是否进行了位读取操作</param>
	/// <returns>带有成功标识的报文对象</returns>
	public static byte[] BuildReadMcCoreCommand(McRAddressData address, bool isBit)
	{
		return new byte[12]
		{
			1,
			4,
			(byte)(isBit ? 1 : 0),
			0,
			BitConverter.GetBytes(address.AddressStart)[0],
			BitConverter.GetBytes(address.AddressStart)[1],
			BitConverter.GetBytes(address.AddressStart)[2],
			BitConverter.GetBytes(address.AddressStart)[3],
			address.McDataType.DataCode[0],
			address.McDataType.DataCode[1],
			(byte)(address.Length % 256),
			(byte)(address.Length / 256)
		};
	}

	/// <summary>
	/// 以字为单位，创建数据写入的核心报文
	/// </summary>
	/// <param name="address">三菱的数据地址</param>
	/// <param name="value">实际的原始数据信息</param>
	/// <returns>带有成功标识的报文对象</returns>
	public static byte[] BuildWriteWordCoreCommand(McRAddressData address, byte[] value)
	{
		value ??= Array.Empty<byte>();

		byte[] array = new byte[12 + value.Length];
		array[0] = 1;
		array[1] = 20;
		array[2] = 0;
		array[3] = 0;
		array[4] = BitConverter.GetBytes(address.AddressStart)[0];
		array[5] = BitConverter.GetBytes(address.AddressStart)[1];
		array[6] = BitConverter.GetBytes(address.AddressStart)[2];
		array[7] = BitConverter.GetBytes(address.AddressStart)[3];
		array[8] = address.McDataType.DataCode[0];
		array[9] = address.McDataType.DataCode[1];
		array[10] = (byte)(value.Length / 2 % 256);
		array[11] = (byte)(value.Length / 2 / 256);
		value.CopyTo(array, 12);
		return array;
	}

	/// <summary>
	/// 以位为单位，创建数据写入的核心报文
	/// </summary>
	/// <param name="address">三菱的地址信息</param>
	/// <param name="value">原始的bool数组数据</param>
	/// <returns>带有成功标识的报文对象</returns>
	public static byte[] BuildWriteBitCoreCommand(McRAddressData address, bool[] value)
	{
		value ??= Array.Empty<bool>();

		byte[] array = MelsecHelper.TransBoolArrayToByteData(value);
		byte[] array2 = new byte[12 + array.Length];
		array2[0] = 1;
		array2[1] = 20;
		array2[2] = 1;
		array2[3] = 0;
		array2[4] = BitConverter.GetBytes(address.AddressStart)[0];
		array2[5] = BitConverter.GetBytes(address.AddressStart)[1];
		array2[6] = BitConverter.GetBytes(address.AddressStart)[2];
		array2[7] = BitConverter.GetBytes(address.AddressStart)[3];
		array2[8] = address.McDataType.DataCode[0];
		array2[9] = address.McDataType.DataCode[1];
		array2[10] = (byte)(value.Length % 256);
		array2[11] = (byte)(value.Length / 256);
		array.CopyTo(array2, 12);
		return array2;
	}
}
