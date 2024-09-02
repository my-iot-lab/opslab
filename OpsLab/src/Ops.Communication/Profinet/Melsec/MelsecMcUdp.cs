using System.Text;
using Ops.Communication.Address;
using Ops.Communication.Core;
using Ops.Communication.Core.Net;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯类，采用UDP的协议实现，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为二进制通讯。
/// </summary>
public class MelsecMcUdp : NetworkUdpDeviceBase
{
	public byte NetworkNumber { get; set; } = 0;

	public byte NetworkStationNumber { get; set; } = 0;

	public MelsecMcUdp()
	{
		base.WordLength = 1;
		base.ByteTransform = new RegularByteTransform();
	}

	public MelsecMcUdp(string ipAddress, int port)
	{
		base.WordLength = 1;
		IpAddress = ipAddress;
		Port = port;
		base.ByteTransform = new RegularByteTransform();
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
			ushort num2 = (ushort)Math.Min(length - num, 900);
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
		byte[] mcCore = MelsecHelper.BuildReadMcCoreCommand(addressData, isBit: false);
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
		return MelsecMcNet.ExtractActualData(SoftBasic.ArrayRemoveBegin(operateResult.Content, 11), isBit: false);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		OperateResult<McAddressData> operateResult = McAnalysisAddress(address, 0);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}
		return WriteAddressData(operateResult.Content, value);
	}

	private OperateResult WriteAddressData(McAddressData addressData, byte[] value)
	{
		byte[] mcCore = MelsecHelper.BuildWriteWordCoreCommand(addressData, value);
		var operateResult = ReadFromCoreServer(MelsecMcNet.PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = MelsecMcNet.CheckResponseContentHelper(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
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

		byte[] mcCore = MelsecHelper.BuildReadRandomWordCommand(array);
		var operateResult2 = ReadFromCoreServer(MelsecMcNet.PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = MelsecMcNet.CheckResponseContentHelper(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return MelsecMcNet.ExtractActualData(SoftBasic.ArrayRemoveBegin(operateResult2.Content, 11), isBit: false);
	}

	public OperateResult<byte[]> ReadRandom(string[] address, ushort[] length)
	{
		if (length.Length != address.Length)
		{
			return new OperateResult<byte[]>((int)OpsErrorCode.TwoParametersLengthIsNotSame, OpsErrorCode.TwoParametersLengthIsNotSame.Desc());
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

		byte[] mcCore = MelsecHelper.BuildReadRandomCommand(array);
		var operateResult2 = ReadFromCoreServer(MelsecMcNet.PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = MelsecMcNet.CheckResponseContentHelper(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return MelsecMcNet.ExtractActualData(SoftBasic.ArrayRemoveBegin(operateResult2.Content, 11), isBit: false);
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

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		OperateResult<McAddressData> operateResult = McAnalysisAddress(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		byte[] mcCore = MelsecHelper.BuildReadMcCoreCommand(operateResult.Content, isBit: true);
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

		var operateResult4 = MelsecMcNet.ExtractActualData(SoftBasic.ArrayRemoveBegin(operateResult2.Content, 11), isBit: true);
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

		byte[] mcCore = MelsecHelper.BuildWriteBitCoreCommand(operateResult.Content, values);
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

	public OperateResult RemoteRun()
	{
		var operateResult = ReadFromCoreServer(MelsecMcNet.PackMcCommand(new byte[8] { 1, 16, 0, 0, 1, 0, 0, 0 }, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = MelsecMcNet.CheckResponseContentHelper(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return OperateResult.Ok();
	}

	public OperateResult RemoteStop()
	{
		var operateResult = ReadFromCoreServer(MelsecMcNet.PackMcCommand(new byte[6] { 2, 16, 0, 0, 1, 0 }, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = MelsecMcNet.CheckResponseContentHelper(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return OperateResult.Ok();
	}

	public OperateResult RemoteReset()
	{
		var operateResult = ReadFromCoreServer(MelsecMcNet.PackMcCommand(new byte[6] { 6, 16, 0, 0, 1, 0 }, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = MelsecMcNet.CheckResponseContentHelper(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return OperateResult.Ok();
	}

	public OperateResult<string> ReadPlcType()
	{
		var operateResult = ReadFromCoreServer(MelsecMcNet.PackMcCommand(new byte[4] { 1, 1, 0, 0 }, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult);
		}

		OperateResult operateResult2 = MelsecMcNet.CheckResponseContentHelper(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult2);
		}
		return OperateResult.Ok(Encoding.ASCII.GetString(operateResult.Content, 11, 16).TrimEnd(Array.Empty<char>()));
	}

	public OperateResult ErrorStateReset()
	{
		var operateResult = ReadFromCoreServer(MelsecMcNet.PackMcCommand(new byte[4] { 23, 22, 0, 0 }, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = MelsecMcNet.CheckResponseContentHelper(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return OperateResult.Ok();
	}

	public override string ToString()
	{
		return $"MelsecMcUdp[{IpAddress}:{Port}]";
	}
}
