using System.Net.Sockets;
using System.Text;
using Ops.Communication.Address;
using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;
using Ops.Communication.Extensions;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Siemens;

/// <summary>
/// 一个西门子的客户端类，使用S7协议来进行数据交互，对于s300,s400需要关注<see cref="Slot" />和<see cref="Rack" />的设置值，
/// 对于s200，需要关注<see cref="LocalTSAP" />和<see cref="DestTSAP" />的设置值，详细参考demo的设置
/// <para>
/// 暂时不支持bool[]的批量写入操作，请使用 Write(string, byte[]) 替换。
/// 对于200smartPLC的V区，就是DB1.X，例如，V100=DB1.100，当然了你也可以输入V100.
/// </para>
/// <para>对于200smartPLC的V区，就是DB1.X，例如，V100=DB1.100</para>
/// </summary>
public class SiemensS7Net : NetworkDeviceBase
{
	private byte[] plcHead1 = new byte[22]
	{
		3, 0, 0, 22, 17, 224, 0, 0, 0, 1,
		0, 192, 1, 10, 193, 2, 1, 2, 194, 2,
		1, 0
	};

	private byte[] plcHead2 = new byte[25]
	{
		3, 0, 0, 25, 2, 240, 128, 50, 1, 0,
		0, 4, 0, 0, 8, 0, 0, 240, 0, 0,
		1, 0, 1, 1, 224
	};

	private readonly byte[] plcOrderNumber = new byte[33]
	{
		3, 0, 0, 33, 2, 240, 128, 50, 7, 0,
		0, 0, 1, 0, 8, 0, 8, 0, 1, 18,
		4, 17, 68, 1, 0, 255, 9, 0, 4, 0,
		17, 0, 0
	};

	private SiemensPLCS CurrentPlc = SiemensPLCS.S1200;

	private readonly byte[] plcHead1_200smart = new byte[22]
	{
		3, 0, 0, 22, 17, 224, 0, 0, 0, 1,
		0, 193, 2, 16, 0, 194, 2, 3, 0, 192,
		1, 10
	};

	private readonly byte[] plcHead2_200smart = new byte[25]
	{
		3, 0, 0, 25, 2, 240, 128, 50, 1, 0,
		0, 204, 193, 0, 8, 0, 0, 240, 0, 0,
		1, 0, 1, 3, 192
	};

	private readonly byte[] plcHead1_200 = new byte[22]
	{
		3, 0, 0, 22, 17, 224, 0, 0, 0, 1,
		0, 193, 2, 77, 87, 194, 2, 77, 87, 192,
		1, 9
	};

	private readonly byte[] plcHead2_200 = new byte[25]
	{
		3, 0, 0, 25, 2, 240, 128, 50, 1, 0,
		0, 0, 0, 0, 8, 0, 0, 240, 0, 0,
		1, 0, 1, 3, 192
	};

	private readonly byte[] S7_STOP = new byte[33]
	{
		3, 0, 0, 33, 2, 240, 128, 50, 1, 0,
		0, 14, 0, 0, 16, 0, 0, 41, 0, 0,
		0, 0, 0, 9, 80, 95, 80, 82, 79, 71,
		82, 65, 77
	};

	private readonly byte[] S7_HOT_START = new byte[37]
	{
		3, 0, 0, 37, 2, 240, 128, 50, 1, 0,
		0, 12, 0, 0, 20, 0, 0, 40, 0, 0,
		0, 0, 0, 0, 253, 0, 0, 9, 80, 95,
		80, 82, 79, 71, 82, 65, 77
	};

	private readonly byte[] S7_COLD_START = new byte[39]
	{
		3, 0, 0, 39, 2, 240, 128, 50, 1, 0,
		0, 15, 0, 0, 22, 0, 0, 40, 0, 0,
		0, 0, 0, 0, 253, 0, 2, 67, 32, 9,
		80, 95, 80, 82, 79, 71, 82, 65, 77
	};

	private byte plc_rack = 0;

	private byte plc_slot = 0;

	private int pdu_length = 0;

	/// <summary>
	/// PLC的槽号，针对S7-400的PLC设置的。
	/// </summary>
	public byte Slot
	{
		get
		{
			return plc_slot;
		}
		set
		{
			plc_slot = value;
			plcHead1[21] = (byte)(plc_rack * 32 + plc_slot);
		}
	}

	/// <summary>
	/// PLC的机架号，针对S7-400的PLC设置的。
	/// </summary>
	public byte Rack
	{
		get
		{
			return plc_rack;
		}
		set
		{
			plc_rack = value;
			plcHead1[21] = (byte)(plc_rack * 32 + plc_slot);
		}
	}

	/// <summary>
	/// 获取或设置当前PLC的连接方式，PG: 0x01，OP: 0x02，S7Basic: 0x03...0x10。
	/// </summary>
	public byte ConnectionType
	{
		get
		{
			return plcHead1[20];
		}
		set
		{
			if (CurrentPlc != SiemensPLCS.S200 && CurrentPlc != SiemensPLCS.S200Smart)
			{
				plcHead1[20] = value;
			}
		}
	}

	/// <summary>
	/// 西门子相关的本地TSAP参数信息。
	/// </summary>
	public int LocalTSAP
	{
		get
		{
			if (CurrentPlc == SiemensPLCS.S200 || CurrentPlc == SiemensPLCS.S200Smart)
			{
				return plcHead1[13] * 256 + plcHead1[14];
			}
			return plcHead1[16] * 256 + plcHead1[17];
		}
		set
		{
			if (CurrentPlc == SiemensPLCS.S200 || CurrentPlc == SiemensPLCS.S200Smart)
			{
				plcHead1[13] = BitConverter.GetBytes(value)[1];
				plcHead1[14] = BitConverter.GetBytes(value)[0];
			}
			else
			{
				plcHead1[16] = BitConverter.GetBytes(value)[1];
				plcHead1[17] = BitConverter.GetBytes(value)[0];
			}
		}
	}

	/// <summary>
	/// 西门子相关的远程TSAP参数信息<br />
	/// A parameter information related to Siemens
	/// </summary>
	public int DestTSAP
	{
		get
		{
			if (CurrentPlc == SiemensPLCS.S200 || CurrentPlc == SiemensPLCS.S200Smart)
			{
				return plcHead1[17] * 256 + plcHead1[18];
			}
			return plcHead1[20] * 256 + plcHead1[21];
		}
		set
		{
			if (CurrentPlc == SiemensPLCS.S200 || CurrentPlc == SiemensPLCS.S200Smart)
			{
				plcHead1[17] = BitConverter.GetBytes(value)[1];
				plcHead1[18] = BitConverter.GetBytes(value)[0];
			}
			else
			{
				plcHead1[20] = BitConverter.GetBytes(value)[1];
				plcHead1[21] = BitConverter.GetBytes(value)[0];
			}
		}
	}

	/// <summary>
	/// 获取当前西门子的PDU的长度信息，不同型号PLC的值会不一样。
	/// </summary>
	public int PDULength => pdu_length;

	/// <summary>
	/// 实例化一个西门子的S7协议的通讯对象。
	/// </summary>
	/// <param name="siemens">指定西门子的型号</param>
	public SiemensS7Net(SiemensPLCS siemens)
	{
		Initialization(siemens, string.Empty);
	}

	/// <summary>
	/// 实例化一个西门子的S7协议的通讯对象并指定Ip地址。
	/// </summary>
	/// <param name="siemens">指定西门子的型号</param>
	/// <param name="ipAddress">Ip地址</param>
	public SiemensS7Net(SiemensPLCS siemens, string ipAddress)
	{
		Initialization(siemens, ipAddress);
	}

	protected override INetMessage GetNewNetMessage()
	{
		return new S7Message();
	}

	/// <summary>
	/// 初始化方法。
	/// </summary>
	/// <param name="siemens">指定西门子的型号</param>
	/// <param name="ipAddress">Ip地址</param>
	private void Initialization(SiemensPLCS siemens, string ipAddress)
	{
		base.WordLength = 2;
		IpAddress = ipAddress;
		Port = 102;
		CurrentPlc = siemens;
		base.ByteTransform = new ReverseBytesTransform();
		switch (siemens)
		{
			case SiemensPLCS.S1200:
				plcHead1[21] = 0;
				break;
			case SiemensPLCS.S300:
				plcHead1[21] = 2;
				break;
			case SiemensPLCS.S400:
				plcHead1[21] = 3;
				plcHead1[17] = 0;
				break;
			case SiemensPLCS.S1500:
				plcHead1[21] = 0;
				break;
			case SiemensPLCS.S200Smart:
				plcHead1 = plcHead1_200smart;
				plcHead2 = plcHead2_200smart;
				break;
			case SiemensPLCS.S200:
				plcHead1 = plcHead1_200;
				plcHead2 = plcHead2_200;
				break;
			default:
				plcHead1[18] = 0;
				break;
		}
	}

	public override OperateResult<byte[]> ReadFromCoreServer(Socket socket, byte[] send, bool hasResponseData = true, bool usePackHeader = true)
	{
		OperateResult<byte[]> operateResult;
		do
		{
			operateResult = base.ReadFromCoreServer(socket, send, hasResponseData, usePackHeader);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
		}
		while (operateResult.Content[2] * 256 + operateResult.Content[3] == 7);
		return operateResult;
	}

	protected override OperateResult InitializationOnConnect(Socket socket)
	{
		OperateResult<byte[]> operateResult = ReadFromCoreServer(socket, plcHead1);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(socket, plcHead2);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		pdu_length = base.ByteTransform.TransUInt16(operateResult2.Content.SelectLast(2), 0) - 28;
		if (pdu_length < 200)
		{
			pdu_length = 200;
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(Socket socket, byte[] send, bool hasResponseData = true, bool usePackHeader = true)
	{
		OperateResult<byte[]> read;
		do
		{
			read = await base.ReadFromCoreServerAsync(socket, send, hasResponseData, usePackHeader);
			if (!read.IsSuccess)
			{
				return read;
			}
		}
		while (read.Content[2] * 256 + read.Content[3] == 7);
		return read;
	}

	protected override async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
	{
		OperateResult<byte[]> read_first = await ReadFromCoreServerAsync(socket, plcHead1);
		if (!read_first.IsSuccess)
		{
			return read_first;
		}

		OperateResult<byte[]> read_second = await ReadFromCoreServerAsync(socket, plcHead2);
		if (!read_second.IsSuccess)
		{
			return read_second;
		}

		pdu_length = base.ByteTransform.TransUInt16(read_second.Content.SelectLast(2), 0) - 28;
		if (pdu_length < 200)
		{
			pdu_length = 200;
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 从PLC读取订货号信息
	/// </summary>
	public OperateResult<string> ReadOrderNumber()
	{
		return ByteTransformHelper.GetSuccessResultFromOther(ReadFromCoreServer(plcOrderNumber), (byte[] m) => Encoding.ASCII.GetString(m, 71, 20));
	}

	public async Task<OperateResult<string>> ReadOrderNumberAsync()
	{
		return ByteTransformHelper.GetSuccessResultFromOther(await ReadFromCoreServerAsync(plcOrderNumber), (byte[] m) => Encoding.ASCII.GetString(m, 71, 20));
	}

	private OperateResult CheckStartResult(byte[] content)
	{
		if (content.Length < 19)
		{
			return new OperateResult("Receive error");
		}
		if (content[19] != 40)
		{
			return new OperateResult("Can not start PLC");
		}
		if (content[20] != 2)
		{
			return new OperateResult("Can not start PLC");
		}
		return OperateResult.Ok();
	}

	private OperateResult CheckStopResult(byte[] content)
	{
		if (content.Length < 19)
		{
			return new OperateResult("Receive error");
		}
		if (content[19] != 41)
		{
			return new OperateResult("Can not stop PLC");
		}
		if (content[20] != 7)
		{
			return new OperateResult("Can not stop PLC");
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 对PLC进行热启动，目前仅适用于200smart型号
	/// </summary>
	/// <returns>是否启动成功的结果对象</returns>
	public OperateResult HotStart()
	{
		return ByteTransformHelper.GetResultFromOther(ReadFromCoreServer(S7_HOT_START), CheckStartResult);
	}

	/// <summary>
	/// 对PLC进行冷启动，目前仅适用于200smart型号
	/// </summary>
	/// <returns>是否启动成功的结果对象</returns>
	public OperateResult ColdStart()
	{
		return ByteTransformHelper.GetResultFromOther(ReadFromCoreServer(S7_COLD_START), CheckStartResult);
	}

	/// <summary>
	/// 对PLC进行停止，目前仅适用于200smart型号
	/// </summary>
	/// <returns>是否启动成功的结果对象</returns>
	public OperateResult Stop()
	{
		return ByteTransformHelper.GetResultFromOther(ReadFromCoreServer(S7_STOP), CheckStopResult);
	}

	public async Task<OperateResult> HotStartAsync()
	{
		return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(S7_HOT_START), CheckStartResult);
	}

	public async Task<OperateResult> ColdStartAsync()
	{
		return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(S7_COLD_START), CheckStartResult);
	}

	public async Task<OperateResult> StopAsync()
	{
		return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(S7_STOP), CheckStopResult);
	}

	/// <summary>
	/// 从PLC读取原始的字节数据，地址格式为I100，Q100，DB20.100，M100，长度参数以字节为单位。
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100。</param>
	/// <param name="length">读取的数量，以字节为单位。</param>
	/// <returns>
	/// 是否读取成功的结果对象。
	/// </returns>
	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		var operateResult = S7AddressData.ParseFrom(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		var list = new List<byte>();
		ushort num = 0;
		while (num < length)
		{
			ushort num2 = (ushort)Math.Min(length - num, pdu_length);
			operateResult.Content.Length = num2;
			OperateResult<byte[]> operateResult2 = Read(new S7AddressData[1] { operateResult.Content });
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			list.AddRange(operateResult2.Content);
			num = (ushort)(num + num2);
			if (operateResult.Content.DataCode == 31 || operateResult.Content.DataCode == 30)
			{
				operateResult.Content.AddressStart += (int)num2 / 2;
			}
			else
			{
				operateResult.Content.AddressStart += num2 * 8;
			}
		}
		return OperateResult.Ok(list.ToArray());
	}

	/// <summary>
	/// 从PLC读取数据，地址格式为I100，Q100，DB20.100，M100，以位为单位。
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100。</param>
	/// <returns>是否读取成功的结果对象。</returns>
	private OperateResult<byte[]> ReadBitFromPLC(string address)
	{
		var operateResult = BuildBitReadCommand(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		var operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return AnalysisReadBit(operateResult2.Content);
	}

	/// <summary>
	/// 一次性从PLC获取所有的数据，按照先后顺序返回一个统一的Buffer，需要按照顺序处理，两个数组长度必须一致，数组长度无限制
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100。</param>
	/// <param name="length">数据长度数组。</param>
	/// <returns>是否读取成功的结果对象。</returns>
	/// <exception cref="T:System.NullReferenceException"></exception>
	/// <remarks>
	/// <note type="warning">原先的批量的长度为19，现在已经内部自动处理整合，目前的长度为任意和长度。</note>
	/// </remarks>
	public OperateResult<byte[]> Read(string[] address, ushort[] length)
	{
		var array = new S7AddressData[address.Length];
		for (int i = 0; i < address.Length; i++)
		{
			var operateResult = S7AddressData.ParseFrom(address[i], length[i]);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult);
			}
			array[i] = operateResult.Content;
		}
		return Read(array);
	}

	/// <summary>
	/// 读取西门子的地址数据信息，支持任意个数的数据读取
	/// </summary>
	/// <param name="s7Addresses">
	/// 西门子的数据地址。</param>
	/// <returns>返回的结果对象信息。</returns>
	public OperateResult<byte[]> Read(S7AddressData[] s7Addresses)
	{
		if (s7Addresses.Length > 19)
		{
			var list = new List<byte>();
			var list2 = SoftBasic.ArraySplitByLength(s7Addresses, 19);
			for (int i = 0; i < list2.Count; i++)
			{
				OperateResult<byte[]> operateResult = Read(list2[i]);
				if (!operateResult.IsSuccess)
				{
					return operateResult;
				}
				list.AddRange(operateResult.Content);
			}
			return OperateResult.Ok(list.ToArray());
		}
		return ReadS7AddressData(s7Addresses);
	}

	/// <summary>
	/// 单次的读取，只能读取最多19个数组的长度，所以不再对外公开该方法
	/// </summary>
	/// <param name="s7Addresses">西门子的地址对象</param>
	/// <returns>返回的结果对象信息</returns>
	private OperateResult<byte[]> ReadS7AddressData(S7AddressData[] s7Addresses)
	{
		var operateResult = BuildReadCommand(s7Addresses);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}
		return AnalysisReadByte(s7Addresses, operateResult2.Content);
	}

	/// <summary>
	/// 基础的写入数据的操作支持。
	/// </summary>
	/// <param name="entireValue">完整的字节数据。</param>
	/// <returns>是否写入成功的结果对象。</returns>
	private OperateResult WriteBase(byte[] entireValue)
	{
		return ByteTransformHelper.GetResultFromOther(ReadFromCoreServer(entireValue), AnalysisWrite);
	}

	/// <summary>
	/// 将数据写入到PLC数据，地址格式为I100，Q100，DB20.100，M100，以字节为单位。
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100</param>
	/// <param name="value">写入的原始数据</param>
	/// <returns>是否写入成功的结果对象</returns>
	/// <example>
	/// 假设起始地址为M100，M100,M101存储了温度，100.6℃值为1006，M102,M103存储了压力，1.23Mpa值为123，M104-M107存储了产量计数
	/// </example>
	public override OperateResult Write(string address, byte[] value)
	{
		OperateResult<S7AddressData> operateResult = S7AddressData.ParseFrom(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		int num = value.Length;
		ushort num2 = 0;
		while (num2 < num)
		{
			ushort num3 = (ushort)Math.Min(num - num2, pdu_length);
			byte[] data = base.ByteTransform.TransByte(value, num2, num3);
			var operateResult2 = BuildWriteByteCommand(operateResult, data);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			OperateResult operateResult3 = WriteBase(operateResult2.Content);
			if (!operateResult3.IsSuccess)
			{
				return operateResult3;
			}
			num2 = (ushort)(num2 + num3);
			operateResult.Content.AddressStart += num3 * 8;
		}

		return OperateResult.Ok();
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		var addressResult = S7AddressData.ParseFrom(address, length);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(addressResult);
		}

		var bytesContent = new List<byte>();
		ushort alreadyFinished = 0;
		while (alreadyFinished < length)
		{
			ushort readLength = (ushort)Math.Min(length - alreadyFinished, 200);
			addressResult.Content.Length = readLength;
			var read = await ReadAsync(new S7AddressData[1] { addressResult.Content });
			if (!read.IsSuccess)
			{
				return read;
			}

			bytesContent.AddRange(read.Content);
			alreadyFinished = (ushort)(alreadyFinished + readLength);
			if (addressResult.Content.DataCode == 31 || addressResult.Content.DataCode == 30)
			{
				addressResult.Content.AddressStart += (int)readLength / 2;
			}
			else
			{
				addressResult.Content.AddressStart += readLength * 8;
			}
		}
		return OperateResult.Ok(bytesContent.ToArray());
	}

	private async Task<OperateResult<byte[]>> ReadBitFromPLCAsync(string address)
	{
		var command = BuildBitReadCommand(address);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<byte[]>(command);
		}

		var read = await ReadFromCoreServerAsync(command.Content);
		if (!read.IsSuccess)
		{
			return read;
		}
		return AnalysisReadBit(read.Content);
	}

	public async Task<OperateResult<byte[]>> ReadAsync(string[] address, ushort[] length)
	{
		var addressResult = new S7AddressData[address.Length];
		for (int i = 0; i < address.Length; i++)
		{
			var tmp = S7AddressData.ParseFrom(address[i], length[i]);
			if (!tmp.IsSuccess)
			{
				return OperateResult.Error<byte[]>(tmp);
			}
			addressResult[i] = tmp.Content;
		}
		return await ReadAsync(addressResult);
	}

	public async Task<OperateResult<byte[]>> ReadAsync(S7AddressData[] s7Addresses)
	{
		if (s7Addresses.Length > 19)
		{
			var bytes = new List<byte>();
			var groups = SoftBasic.ArraySplitByLength(s7Addresses, 19);
			for (int i = 0; i < groups.Count; i++)
			{
				OperateResult<byte[]> read = await ReadAsync(groups[i]);
				if (!read.IsSuccess)
				{
					return read;
				}
				bytes.AddRange(read.Content);
			}
			return OperateResult.Ok(bytes.ToArray());
		}
		return await ReadS7AddressDataAsync(s7Addresses);
	}

	private async Task<OperateResult<byte[]>> ReadS7AddressDataAsync(S7AddressData[] s7Addresses)
	{
		var command = BuildReadCommand(s7Addresses);
		if (!command.IsSuccess)
		{
			return command;
		}

		var read = await ReadFromCoreServerAsync(command.Content);
		if (!read.IsSuccess)
		{
			return read;
		}
		return AnalysisReadByte(s7Addresses, read.Content);
	}

	private async Task<OperateResult> WriteBaseAsync(byte[] entireValue)
	{
		return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(entireValue), AnalysisWrite);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		var analysis = S7AddressData.ParseFrom(address);
		if (!analysis.IsSuccess)
		{
			return OperateResult.Error<byte[]>(analysis);
		}

		int length = value.Length;
		ushort alreadyFinished = 0;
		while (alreadyFinished < length)
		{
			ushort writeLength = (ushort)Math.Min(length - alreadyFinished, 200);
			byte[] buffer = base.ByteTransform.TransByte(value, alreadyFinished, writeLength);
			var command = BuildWriteByteCommand(analysis, buffer);
			if (!command.IsSuccess)
			{
				return command;
			}

			OperateResult write = await WriteBaseAsync(command.Content);
			if (!write.IsSuccess)
			{
				return write;
			}
			alreadyFinished = (ushort)(alreadyFinished + writeLength);
			analysis.Content.AddressStart += writeLength * 8;
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 读取指定地址的bool数据，地址格式为I100，M100，Q100，DB20.100。
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100</param>
	/// <returns>是否读取成功的结果对象</returns>
	/// <remarks>
	/// <note type="important">
	/// 对于200smartPLC的V区，就是DB1.X，例如，V100=DB1.100
	/// </note>
	/// </remarks>
	/// <example>
	/// 假设读取M100.0的位是否通断
	/// </example>
	public override OperateResult<bool> ReadBool(string address)
	{
		return ByteTransformHelper.GetResultFromBytes(ReadBitFromPLC(address), (byte[] m) => m[0] != 0);
	}

	/// <summary>
	/// 读取指定地址的bool数组，地址格式为I100，M100，Q100，DB20.100。
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100</param>
	/// <param name="length">读取的长度信息</param>
	/// <returns>是否读取成功的结果对象</returns>
	/// <remarks>
	/// <note type="important">
	/// 对于200smartPLC的V区，就是DB1.X，例如，V100=DB1.100
	/// </note>
	/// </remarks>
	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		var operateResult = S7AddressData.ParseFrom(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		OpsHelper.CalculateStartBitIndexAndLength(operateResult.Content.AddressStart, length, out var newStart, out var byteLength, out var offset);
		operateResult.Content.AddressStart = newStart;
		operateResult.Content.Length = byteLength;
		var operateResult2 = Read(new S7AddressData[1] { operateResult.Content });
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult2);
		}
		return OperateResult.Ok(operateResult2.Content.ToBoolArray().SelectMiddle(offset, length));
	}

	/// <summary>
	/// 写入PLC的一个位，例如"M100.6"，"I100.7"，"Q100.0"，"DB20.100.0"，如果只写了"M100"默认为"M100.0"
	/// </summary>
	/// <param name="address">起始地址，格式为"M100.6",  "I100.7",  "Q100.0",  "DB20.100.0"</param>
	/// <param name="value">写入的数据，True或是False</param>
	/// <returns>是否写入成功的结果对象</returns>
	public override OperateResult Write(string address, bool value)
	{
		var operateResult = BuildWriteBitCommand(address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}
		return WriteBase(operateResult.Content);
	}

	/// <summary>
	/// [危险] 向PLC中写入bool数组，比如你写入M100,那么data[0]对应M100.0
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100</param>
	/// <param name="values">要写入的bool数组，长度为8的倍数</param>
	/// <returns>是否写入成功的结果对象</returns>
	/// <remarks>
	/// <note type="warning">
	/// 批量写入bool数组存在一定的风险，原因是只能批量写入长度为8的倍数的数组，否则会影响其他的位的数据，请谨慎使用。
	/// </note>
	/// </remarks>
	public override OperateResult Write(string address, bool[] values)
	{
		return Write(address, SoftBasic.BoolArrayToByte(values));
	}

	public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadBitFromPLCAsync(address), (byte[] m) => m[0] != 0);
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		var analysis = S7AddressData.ParseFrom(address);
		if (!analysis.IsSuccess)
		{
			return OperateResult.Error<bool[]>(analysis);
		}

		OpsHelper.CalculateStartBitIndexAndLength(analysis.Content.AddressStart, length, out var newStart, out var byteLength, out var offset);
		analysis.Content.AddressStart = newStart;
		analysis.Content.Length = byteLength;
		var read = await ReadAsync(new S7AddressData[1] { analysis.Content });
		if (!read.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read);
		}
		return OperateResult.Ok(read.Content.ToBoolArray().SelectMiddle(offset, length));
	}

	public override async Task<OperateResult> WriteAsync(string address, bool value)
	{
		var command = BuildWriteBitCommand(address, value);
		if (!command.IsSuccess)
		{
			return command;
		}
		return await WriteBaseAsync(command.Content);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool[] values)
	{
		return await WriteAsync(address, SoftBasic.BoolArrayToByte(values));
	}

	/// <summary>
	/// 读取指定地址的byte数据，地址格式I100，M100，Q100，DB20.100
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100</param>
	/// <returns>是否读取成功的结果对象</returns>
	public OperateResult<byte> ReadByte(string address)
	{
		return ByteTransformHelper.GetResultFromArray(Read(address, 1));
	}

	/// <summary>
	/// 向PLC中写入byte数据，返回值说明
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100</param>
	/// <param name="value">byte数据</param>
	/// <returns>是否写入成功的结果对象</returns>
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

	public override OperateResult Write(string address, string value, Encoding encoding)
	{
		if (value == null)
		{
			value = string.Empty;
		}

		byte[] array = encoding.GetBytes(value);
		if (encoding == Encoding.Unicode)
		{
			array = SoftBasic.BytesReverseByWord(array);
		}
		if (CurrentPlc != SiemensPLCS.S200Smart)
		{
			var operateResult = Read(address, 2);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			if (operateResult.Content[0] == byte.MaxValue)
			{
				return new OperateResult<string>(ErrorCode.SiemensValueOfPlcIsNotStringType.Desc());
			}
			if (operateResult.Content[0] == 0)
			{
				operateResult.Content[0] = 254;
			}
			if (value.Length > operateResult.Content[0])
			{
				return new OperateResult<string>(ErrorCode.SiemensStringlengthIsToolongThanPlcDefined.Desc());
			}
			return Write(address, SoftBasic.SpliceArray(new byte[2]
			{
				operateResult.Content[0],
				(byte)value.Length,
			}, array));
		}
		return Write(address, SoftBasic.SpliceArray<byte>(new byte[1] { (byte)value.Length }, array));
	}

	/// <summary>
	/// 使用双字节编码的方式，将字符串以 Unicode 编码写入到PLC的地址里，可以使用中文。
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100</param>
	/// <param name="value">字符串的值</param>
	/// <returns>是否写入成功的结果对象</returns>
	public OperateResult WriteWString(string address, string value)
	{
		return Write(address, value, Encoding.Unicode);
	}

	public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
	{
		if (length == 0)
		{
			return ReadString(address);
		}
		return base.ReadString(address, length, encoding);
	}

	/// <summary>
	/// 读取西门子的地址的字符串信息，这个信息是和西门子绑定在一起，长度随西门子的信息动态变化的
	/// </summary>
	/// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
	/// <returns>带有是否成功的字符串结果类对象</returns>
	public OperateResult<string> ReadString(string address)
	{
		if (CurrentPlc != SiemensPLCS.S200Smart)
		{
			var operateResult = Read(address, 2);  // TODO: 如何避免字符串每次请求的预读校验？
			if (!operateResult.IsSuccess)
			{
				return OperateResult.Error<string>(operateResult);
			}
			if (operateResult.Content[0] == 0 || operateResult.Content[0] == byte.MaxValue)
			{
				return new OperateResult<string>(ErrorCode.SiemensValueOfPlcIsNotStringType.Desc());
			}

			var operateResult2 = Read(address, (ushort)(2 + operateResult.Content[1]));
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.Error<string>(operateResult2);
			}
			return OperateResult.Ok(Encoding.ASCII.GetString(operateResult2.Content, 2, operateResult2.Content.Length - 2));
		}

		var operateResult3 = Read(address, 1);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult3);
		}

		var operateResult4 = Read(address, (ushort)(1 + operateResult3.Content[0]));
		if (!operateResult4.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult4);
		}
		return OperateResult.Ok(Encoding.ASCII.GetString(operateResult4.Content, 1, operateResult4.Content.Length - 1));
	}

	/// <summary>
	/// 读取西门子的地址的字符串信息，这个信息是和西门子绑定在一起，长度随西门子的信息动态变化的
	/// </summary>
	/// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
	/// <returns>带有是否成功的字符串结果类对象</returns>
	public OperateResult<string> ReadWString(string address)
	{
		if (CurrentPlc != SiemensPLCS.S200Smart)
		{
			var operateResult = Read(address, 2);  // TODO: 如何避免字符串每次请求的预读校验？
			if (!operateResult.IsSuccess)
			{
				return OperateResult.Error<string>(operateResult);
			}

			if (operateResult.Content[0] == 0 || operateResult.Content[0] == byte.MaxValue)
			{
				return new OperateResult<string>(ErrorCode.SiemensValueOfPlcIsNotStringType.Desc());
			}

			var operateResult2 = Read(address, (ushort)(2 + operateResult.Content[1] * 2));
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.Error<string>(operateResult2);
			}
			return OperateResult.Ok(Encoding.Unicode.GetString(SoftBasic.BytesReverseByWord(operateResult2.Content.RemoveBegin(2))));
		}

		var operateResult3 = Read(address, 1);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult3);
		}

		var operateResult4 = Read(address, (ushort)(1 + operateResult3.Content[0] * 2));
		if (!operateResult4.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult4);
		}
		return OperateResult.Ok(Encoding.Unicode.GetString(operateResult4.Content, 1, operateResult4.Content.Length - 1));
	}

	public override async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		byte[] buffer = encoding.GetBytes(value);
		if (encoding == Encoding.Unicode)
		{
			buffer = SoftBasic.BytesReverseByWord(buffer);
		}

		if (CurrentPlc != SiemensPLCS.S200Smart)
		{
			var readLength = await ReadAsync(address, 2);
			if (!readLength.IsSuccess)
			{
				return readLength;
			}
			if (readLength.Content[0] == byte.MaxValue)
			{
				return new OperateResult<string>(ErrorCode.SiemensValueOfPlcIsNotStringType.Desc());
			}
			if (readLength.Content[0] == 0)
			{
				readLength.Content[0] = 254;
			}
			if (value.Length > readLength.Content[0])
			{
				return new OperateResult<string>(ErrorCode.SiemensStringlengthIsToolongThanPlcDefined.Desc());
			}

			return await WriteAsync(address, SoftBasic.SpliceArray(new byte[2]
			{
				readLength.Content[0],
				(byte)value.Length
			}, buffer));
		}
		return await WriteAsync(address, SoftBasic.SpliceArray(new byte[1] { (byte)value.Length }, buffer));
	}

	public async Task<OperateResult> WriteWStringAsync(string address, string value)
	{
		return await WriteAsync(address, value, Encoding.Unicode);
	}

	public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
	{
		if (length == 0)
		{
			return await ReadStringAsync(address);
		}
		return await base.ReadStringAsync(address, length, encoding);
	}

	public async Task<OperateResult<string>> ReadStringAsync(string address)
	{
		if (CurrentPlc != SiemensPLCS.S200Smart)
		{
			OperateResult<byte[]> read2 = await ReadAsync(address, 2);
			if (!read2.IsSuccess)
			{
				return OperateResult.Error<string>(read2);
			}
			if (read2.Content[0] == 0 || read2.Content[0] == byte.MaxValue)
			{
				return new OperateResult<string>(ErrorCode.SiemensValueOfPlcIsNotStringType.Desc());
			}

			OperateResult<byte[]> readString2 = await ReadAsync(address, (ushort)(2 + read2.Content[1]));
			if (!readString2.IsSuccess)
			{
				return OperateResult.Error<string>(readString2);
			}
			return OperateResult.Ok(Encoding.ASCII.GetString(readString2.Content, 2, readString2.Content.Length - 2));
		}

		var read = await ReadAsync(address, 1);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<string>(read);
		}

		var readString = await ReadAsync(address, (ushort)(1 + read.Content[0]));
		if (!readString.IsSuccess)
		{
			return OperateResult.Error<string>(readString);
		}
		return OperateResult.Ok(Encoding.ASCII.GetString(readString.Content, 1, readString.Content.Length - 1));
	}

	public async Task<OperateResult<string>> ReadWStringAsync(string address)
	{
		if (CurrentPlc != SiemensPLCS.S200Smart)
		{
			var read2 = await ReadAsync(address, 2);
			if (!read2.IsSuccess)
			{
				return OperateResult.Error<string>(read2);
			}
			if (read2.Content[0] == 0 || read2.Content[0] == byte.MaxValue)
			{
				return new OperateResult<string>(ErrorCode.SiemensValueOfPlcIsNotStringType.Desc());
			}

			var readString2 = await ReadAsync(address, (ushort)(2 + read2.Content[1] * 2));
			if (!readString2.IsSuccess)
			{
				return OperateResult.Error<string>(readString2);
			}
			return OperateResult.Ok(Encoding.Unicode.GetString(SoftBasic.BytesReverseByWord(readString2.Content.RemoveBegin(2))));
		}

		var read = await ReadAsync(address, 1);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<string>(read);
		}

		var readString = await ReadAsync(address, (ushort)(1 + read.Content[0] * 2));
		if (!readString.IsSuccess)
		{
			return OperateResult.Error<string>(readString);
		}
		return OperateResult.Ok(Encoding.Unicode.GetString(readString.Content, 1, readString.Content.Length - 1));
	}

	/// <summary>
	/// 从PLC中读取时间格式的数据
	/// </summary>
	/// <param name="address">地址</param>
	/// <returns>时间对象</returns>
	public OperateResult<DateTime> ReadDateTime(string address)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, 8), SiemensDateTime.FromByteArray);
	}

	/// <summary>
	/// 向PLC中写入时间格式的数据
	/// </summary>
	/// <param name="address">地址</param>
	/// <param name="dateTime">时间</param>
	/// <returns>是否写入成功</returns>
	public OperateResult Write(string address, DateTime dateTime)
	{
		return Write(address, SiemensDateTime.ToByteArray(dateTime));
	}

	public async Task<OperateResult<DateTime>> ReadDateTimeAsync(string address)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 8), SiemensDateTime.FromByteArray);
	}

	public async Task<OperateResult> WriteAsync(string address, DateTime dateTime)
	{
		return await WriteAsync(address, SiemensDateTime.ToByteArray(dateTime));
	}

	public override string ToString()
	{
		return $"SiemensS7Net {CurrentPlc}[{IpAddress}:{Port}]";
	}

	/// <summary>
	/// A general method for generating a command header to read a Word data
	/// </summary>
	/// <param name="s7Addresses">siemens address</param>
	/// <returns>Message containing the result object</returns>
	public static OperateResult<byte[]> BuildReadCommand(S7AddressData[] s7Addresses)
	{
		if (s7Addresses == null)
		{
			throw new NullReferenceException(nameof(s7Addresses));
		}
		if (s7Addresses.Length > 19)
		{
			throw new Exception(ErrorCode.SiemensReadLengthCannotLargerThan19.Desc());
		}

		int num = s7Addresses.Length;
		byte[] array = new byte[19 + num * 12];
		array[0] = 3;
		array[1] = 0;
		array[2] = (byte)(array.Length / 256);
		array[3] = (byte)(array.Length % 256);
		array[4] = 2;
		array[5] = 240;
		array[6] = 128;
		array[7] = 50;
		array[8] = 1;
		array[9] = 0;
		array[10] = 0;
		array[11] = 0;
		array[12] = 1;
		array[13] = (byte)((array.Length - 17) / 256);
		array[14] = (byte)((array.Length - 17) % 256);
		array[15] = 0;
		array[16] = 0;
		array[17] = 4;
		array[18] = (byte)num;
		for (int i = 0; i < num; i++)
		{
			array[19 + i * 12] = 18;
			array[20 + i * 12] = 10;
			array[21 + i * 12] = 16;
			if (s7Addresses[i].DataCode == 30 || s7Addresses[i].DataCode == 31)
			{
				array[22 + i * 12] = s7Addresses[i].DataCode;
				array[23 + i * 12] = (byte)(s7Addresses[i].Length / 2 / 256);
				array[24 + i * 12] = (byte)(s7Addresses[i].Length / 2 % 256);
			}
			else if ((s7Addresses[i].DataCode == 6) | (s7Addresses[i].DataCode == 7))
			{
				array[22 + i * 12] = 4;
				array[23 + i * 12] = (byte)(s7Addresses[i].Length / 2 / 256);
				array[24 + i * 12] = (byte)(s7Addresses[i].Length / 2 % 256);
			}
			else
			{
				array[22 + i * 12] = 2;
				array[23 + i * 12] = (byte)(s7Addresses[i].Length / 256);
				array[24 + i * 12] = (byte)(s7Addresses[i].Length % 256);
			}
			array[25 + i * 12] = (byte)(s7Addresses[i].DbBlock / 256);
			array[26 + i * 12] = (byte)(s7Addresses[i].DbBlock % 256);
			array[27 + i * 12] = s7Addresses[i].DataCode;
			array[28 + i * 12] = (byte)(s7Addresses[i].AddressStart / 256 / 256 % 256);
			array[29 + i * 12] = (byte)(s7Addresses[i].AddressStart / 256 % 256);
			array[30 + i * 12] = (byte)(s7Addresses[i].AddressStart % 256);
		}
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 生成一个位读取数据指令头的通用方法。
	/// </summary>
	/// <param name="address">起始地址，例如M100.0，I0.1，Q0.1，DB2.100.2</param>
	/// <returns>包含结果对象的报文</returns>
	public static OperateResult<byte[]> BuildBitReadCommand(string address)
	{
		OperateResult<S7AddressData> operateResult = S7AddressData.ParseFrom(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		byte[] array = new byte[31];
		array[0] = 3;
		array[1] = 0;
		array[2] = (byte)(array.Length / 256);
		array[3] = (byte)(array.Length % 256);
		array[4] = 2;
		array[5] = 240;
		array[6] = 128;
		array[7] = 50;
		array[8] = 1;
		array[9] = 0;
		array[10] = 0;
		array[11] = 0;
		array[12] = 1;
		array[13] = (byte)((array.Length - 17) / 256);
		array[14] = (byte)((array.Length - 17) % 256);
		array[15] = 0;
		array[16] = 0;
		array[17] = 4;
		array[18] = 1;
		array[19] = 18;
		array[20] = 10;
		array[21] = 16;
		array[22] = 1;
		array[23] = 0;
		array[24] = 1;
		array[25] = (byte)(operateResult.Content.DbBlock / 256);
		array[26] = (byte)(operateResult.Content.DbBlock % 256);
		array[27] = operateResult.Content.DataCode;
		array[28] = (byte)(operateResult.Content.AddressStart / 256 / 256 % 256);
		array[29] = (byte)(operateResult.Content.AddressStart / 256 % 256);
		array[30] = (byte)(operateResult.Content.AddressStart % 256);
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 生成一个写入字节数据的指令。
	/// </summary>
	/// <param name="analysis">起始地址，示例M100,I100,Q100,DB1.100</param>
	/// <param name="data">原始的字节数据</param>
	/// <returns>包含结果对象的报文</returns>
	public static OperateResult<byte[]> BuildWriteByteCommand(OperateResult<S7AddressData> analysis, byte[] data)
	{
		byte[] array = new byte[35 + data.Length];
		array[0] = 3;
		array[1] = 0;
		array[2] = (byte)((35 + data.Length) / 256);
		array[3] = (byte)((35 + data.Length) % 256);
		array[4] = 2;
		array[5] = 240;
		array[6] = 128;
		array[7] = 50;
		array[8] = 1;
		array[9] = 0;
		array[10] = 0;
		array[11] = 0;
		array[12] = 1;
		array[13] = 0;
		array[14] = 14;
		array[15] = (byte)((4 + data.Length) / 256);
		array[16] = (byte)((4 + data.Length) % 256);
		array[17] = 5;
		array[18] = 1;
		array[19] = 18;
		array[20] = 10;
		array[21] = 16;
		if (analysis.Content.DataCode == 6 || analysis.Content.DataCode == 7)
		{
			array[22] = 4;
			array[23] = (byte)(data.Length / 2 / 256);
			array[24] = (byte)(data.Length / 2 % 256);
		}
		else
		{
			array[22] = 2;
			array[23] = (byte)(data.Length / 256);
			array[24] = (byte)(data.Length % 256);
		}
		array[25] = (byte)(analysis.Content.DbBlock / 256);
		array[26] = (byte)(analysis.Content.DbBlock % 256);
		array[27] = analysis.Content.DataCode;
		array[28] = (byte)(analysis.Content.AddressStart / 256 / 256 % 256);
		array[29] = (byte)(analysis.Content.AddressStart / 256 % 256);
		array[30] = (byte)(analysis.Content.AddressStart % 256);
		array[31] = 0;
		array[32] = 4;
		array[33] = (byte)(data.Length * 8 / 256);
		array[34] = (byte)(data.Length * 8 % 256);
		data.CopyTo(array, 35);
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 生成一个写入位数据的指令。
	/// </summary>
	/// <param name="address">起始地址，示例M100,I100,Q100,DB1.100</param>
	/// <param name="data">是否通断</param>
	/// <returns>包含结果对象的报文</returns>
	public static OperateResult<byte[]> BuildWriteBitCommand(string address, bool data)
	{
		OperateResult<S7AddressData> operateResult = S7AddressData.ParseFrom(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		byte[] array = new byte[1] { (byte)(data ? 1 : 0) };
		byte[] array2 = new byte[35 + array.Length];
		array2[0] = 3;
		array2[1] = 0;
		array2[2] = (byte)((35 + array.Length) / 256);
		array2[3] = (byte)((35 + array.Length) % 256);
		array2[4] = 2;
		array2[5] = 240;
		array2[6] = 128;
		array2[7] = 50;
		array2[8] = 1;
		array2[9] = 0;
		array2[10] = 0;
		array2[11] = 0;
		array2[12] = 1;
		array2[13] = 0;
		array2[14] = 14;
		array2[15] = (byte)((4 + array.Length) / 256);
		array2[16] = (byte)((4 + array.Length) % 256);
		array2[17] = 5;
		array2[18] = 1;
		array2[19] = 18;
		array2[20] = 10;
		array2[21] = 16;
		array2[22] = 1;
		array2[23] = (byte)(array.Length / 256);
		array2[24] = (byte)(array.Length % 256);
		array2[25] = (byte)(operateResult.Content.DbBlock / 256);
		array2[26] = (byte)(operateResult.Content.DbBlock % 256);
		array2[27] = operateResult.Content.DataCode;
		array2[28] = (byte)(operateResult.Content.AddressStart / 256 / 256);
		array2[29] = (byte)(operateResult.Content.AddressStart / 256);
		array2[30] = (byte)(operateResult.Content.AddressStart % 256);
		if (operateResult.Content.DataCode == 28)
		{
			array2[31] = 0;
			array2[32] = 9;
		}
		else
		{
			array2[31] = 0;
			array2[32] = 3;
		}
		array2[33] = (byte)(array.Length / 256);
		array2[34] = (byte)(array.Length % 256);
		array.CopyTo(array2, 35);
		return OperateResult.Ok(array2);
	}

	#region privates

	private static OperateResult<byte[]> AnalysisReadByte(S7AddressData[] s7Addresses, byte[] content)
	{
		int num = 0;
		for (int i = 0; i < s7Addresses.Length; i++)
		{
			num = (s7Addresses[i].DataCode != 31 && s7Addresses[i].DataCode != 30) ? (num + s7Addresses[i].Length) : (num + s7Addresses[i].Length * 2);
		}

		if (content.Length >= 21 && content[20] == s7Addresses.Length)
		{
			byte[] array = new byte[num];
			int num2 = 0;
			int num3 = 0;
			for (int j = 21; j < content.Length; j++)
			{
				if (j + 1 >= content.Length)
				{
					continue;
				}
				if (content[j] == byte.MaxValue && content[j + 1] == 4)
				{
					Array.Copy(content, j + 4, array, num3, s7Addresses[num2].Length);
					j += s7Addresses[num2].Length + 3;
					num3 += s7Addresses[num2].Length;
					num2++;
				}
				else if (content[j] == byte.MaxValue && content[j + 1] == 9)
				{
					int num4 = content[j + 2] * 256 + content[j + 3];
					if (num4 % 3 == 0)
					{
						for (int k = 0; k < num4 / 3; k++)
						{
							Array.Copy(content, j + 5 + 3 * k, array, num3, 2);
							num3 += 2;
						}
					}
					else
					{
						for (int l = 0; l < num4 / 5; l++)
						{
							Array.Copy(content, j + 7 + 5 * l, array, num3, 2);
							num3 += 2;
						}
					}
					j += num4 + 4;
					num2++;
				}
				else
				{
					if (content[j] == 5 && content[j + 1] == 0)
					{
						return new OperateResult<byte[]>(content[j], ErrorCode.SiemensReadLengthOverPlcAssign.Desc());
					}
					if (content[j] == 6 && content[j + 1] == 0)
					{
						return new OperateResult<byte[]>(content[j], ErrorCode.SiemensError0006.Desc());
					}
					if (content[j] == 10 && content[j + 1] == 0)
					{
						return new OperateResult<byte[]>(content[j], ErrorCode.SiemensError000A.Desc());
					}
				}
			}
			return OperateResult.Ok(array);
		}

		return new OperateResult<byte[]>($"{ErrorCode.SiemensDataLengthCheckFailed.Desc()}, Msg: {SoftBasic.ByteToHexString(content, ' ')}");
	}

	private static OperateResult<byte[]> AnalysisReadBit(byte[] content)
	{
		int num = 1;
		if (content.Length >= 21 && content[20] == 1)
		{
			byte[] array = new byte[num];
			if (22 < content.Length && content[21] == byte.MaxValue && content[22] == 3)
			{
				array[0] = content[25];
			}
			return OperateResult.Ok(array);
		}
		return new OperateResult<byte[]>(ErrorCode.SiemensDataLengthCheckFailed.Desc());
	}

	private static OperateResult AnalysisWrite(byte[] content)
	{
		byte b = content[^1];
		if (b != byte.MaxValue)
		{
			return new OperateResult(b, $"SiemensWriteError {b} Msg: {SoftBasic.ByteToHexString(content, ' ')}");
		}
		return OperateResult.Ok();
	}

	#endregion
}
