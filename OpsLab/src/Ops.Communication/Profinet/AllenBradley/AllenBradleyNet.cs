using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;
using Ops.Communication.Extensions;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.AllenBradley;

/// <summary>
/// AB PLC的数据通信类，使用CIP协议实现，适用1756，1769等型号，支持使用标签的形式进行读写操作，
/// 支持标量数据，一维数组，二维数组，三维数组等等。如果是局部变量，那么使用 Program:MainProgram.[变量名]。
/// </summary>
/// <remarks>
/// <br />
/// 默认的地址就是PLC里的TAG名字，比如A，B，C；如果你需要读取的数据是一个数组，那么A就是默认的A[0]，如果想要读取偏移量为10的数据，那么地址为A[10]，
/// 多维数组同理，使用A[10,10,10]的操作。
/// <br />
/// <br />
/// 假设你读取的是局部变量，那么使用 Program:MainProgram.变量名<br />
/// 目前适用的系列为1756 ControlLogix, 1756 GuardLogix, 1769 CompactLogix, 1769 Compact GuardLogix, 1789SoftLogix, 
/// 5069 CompactLogix, 5069 Compact GuardLogix, Studio 5000 Logix Emulate
/// <br />
/// <br />
/// 如果你有个Bool数组要读取，变量名为 A, 那么读第0个位，可以通过 ReadBool("A")，但是第二个位需要使用<br />
/// ReadBoolArray("A[0]")   // 返回32个bool长度，0-31的索引，如果我想读取32-63的位索引，就需要 ReadBoolArray("A[1]") ，以此类推。
/// <br />
/// <br />
/// 地址可以携带站号信息，只要在前面加上slot=2;即可，这就是访问站号2的数据了，例如 slot=2;AAA
/// </remarks>
public class AllenBradleyNet : NetworkDeviceBase
{
	/// <summary>
	/// The current session handle, which is determined by the PLC when communicating with the PLC handshake
	/// </summary>
	public uint SessionHandle { get; protected set; }

	/// <summary>
	/// Gets or sets the slot number information for the current plc, which should be set before connections
	/// </summary>
	public byte Slot { get; set; } = 0;

	/// <summary>
	/// port and slot information
	/// </summary>
	public byte[] PortSlot { get; set; }

	/// <summary>
	/// 获取或设置整个交互指令的控制码，默认为0x6F，通常不需要修改。
	/// </summary>
	public ushort CipCommand { get; set; } = 111;

	/// <summary>
	/// Instantiate a communication object for a Allenbradley PLC protocol
	/// </summary>
	public AllenBradleyNet()
	{
		WordLength = 2;
		ByteTransform = new RegularByteTransform();
	}

	/// <summary>
	/// Instantiate a communication object for a Allenbradley PLC protocol
	/// </summary>
	/// <param name="ipAddress">PLC IpAddress</param>
	/// <param name="port">PLC Port</param>
	public AllenBradleyNet(string ipAddress, int port = 44818)
		: this()
	{
		IpAddress = ipAddress;
		Port = port;
	}

	protected override INetMessage GetNewNetMessage()
	{
		return new AllenBradleyMessage();
	}

	protected override byte[] PackCommandWithHeader(byte[] command)
	{
		return AllenBradleyHelper.PackRequestHeader(CipCommand, SessionHandle, command);
	}

	protected override OperateResult InitializationOnConnect(Socket socket)
	{
		var operateResult = ReadFromCoreServer(socket, AllenBradleyHelper.RegisterSessionHandle(), true, false);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = AllenBradleyHelper.CheckResponse(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		SessionHandle = ByteTransform.TransUInt32(operateResult.Content, 4);
		return OperateResult.Ok();
	}

	protected override OperateResult ExtraOnDisconnect(Socket socket)
	{
		var operateResult = ReadFromCoreServer(socket, AllenBradleyHelper.UnRegisterSessionHandle(SessionHandle), true, false);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}
		return OperateResult.Ok();
	}

	protected override async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
	{
		var read = await ReadFromCoreServerAsync(socket, AllenBradleyHelper.RegisterSessionHandle(), true, false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
		if (!check.IsSuccess)
		{
			return check;
		}

		SessionHandle = ByteTransform.TransUInt32(read.Content, 4);
		return OperateResult.Ok();
	}

	protected override async Task<OperateResult> ExtraOnDisconnectAsync(Socket socket)
	{
		var read = await ReadFromCoreServerAsync(socket, AllenBradleyHelper.UnRegisterSessionHandle(SessionHandle), true, false);
		if (!read.IsSuccess)
		{
			return read;
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 创建一个读取标签的报文指定，标签地址可以手动动态指定slot编号，例如 slot=2;AAA。
	/// </summary>
	/// <param name="address">the address of the tag name</param>
	/// <param name="length">Array information, if not arrays, is 1 </param>
	/// <returns>Message information that contains the result object </returns>
	public virtual OperateResult<byte[]> BuildReadCommand(string[] address, int[] length)
	{
		if (address == null || length == null)
		{
			return new OperateResult<byte[]>("address or length is null");
		}

		if (address.Length != length.Length)
		{
			return new OperateResult<byte[]>("address and length is not same array");
		}

		try
		{
			byte b = Slot;
			var list = new List<byte[]>();
			for (int i = 0; i < address.Length; i++)
			{
				b = (byte)OpsHelper.ExtractParameter(ref address[i], "slot", Slot);
				list.Add(AllenBradleyHelper.PackRequsetRead(address[i], length[i]));
			}

			byte[] value = AllenBradleyHelper.PackCommandSpecificData(new byte[4], PackCommandService(PortSlot ?? new byte[2] { 1, b }, list.ToArray()));
			return OperateResult.Ok(value);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
		}
	}

	/// <summary>
	/// 创建一个读取多标签的报文。
	/// </summary>
	/// <param name="address">The address of the tag name </param>
	/// <returns>Message information that contains the result object </returns>
	public OperateResult<byte[]> BuildReadCommand(string[] address)
	{
		if (address == null)
		{
			return new OperateResult<byte[]>("address or length is null");
		}

		int[] array = new int[address.Length];
		for (int i = 0; i < address.Length; i++)
		{
			array[i] = 1;
		}
		return BuildReadCommand(address, array);
	}

	/// <summary>
	/// Create a written message instruction
	/// </summary>
	/// <param name="address">The address of the tag name </param>
	/// <param name="typeCode">Data type</param>
	/// <param name="data">Source Data </param>
	/// <param name="length">In the case of arrays, the length of the array </param>
	/// <returns>Message information that contains the result object</returns>
	public OperateResult<byte[]> BuildWriteCommand(string address, ushort typeCode, byte[] data, int length = 1)
	{
		try
		{
			byte b = (byte)OpsHelper.ExtractParameter(ref address, "slot", Slot);
			byte[] array = AllenBradleyHelper.PackRequestWrite(address, typeCode, data, length);
			byte[] value = AllenBradleyHelper.PackCommandSpecificData(new byte[4], PackCommandService(PortSlot ?? new byte[2] { 1, b }, array));
			return OperateResult.Ok(value);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
		}
	}

	/// <summary>
	/// Create a written message instruction
	/// </summary>
	/// <param name="address">The address of the tag name </param>
	/// <param name="data">Bool Data </param>
	/// <returns>Message information that contains the result object</returns>
	public OperateResult<byte[]> BuildWriteCommand(string address, bool data)
	{
		try
		{
			byte b = (byte)OpsHelper.ExtractParameter(ref address, "slot", Slot);
			byte[] array = AllenBradleyHelper.PackRequestWrite(address, data);
			byte[] value = AllenBradleyHelper.PackCommandSpecificData(new byte[4], PackCommandService(PortSlot ?? new byte[2] { 1, b }, array));
			return OperateResult.Ok(value);
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
		}
	}

	/// <summary>
	/// Read data information, data length for read array length information
	/// </summary>
	/// <param name="address">Address format of the node</param>
	/// <param name="length">In the case of arrays, the length of the array </param>
	/// <returns>Result data with result object </returns>
	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		if (length > 1)
		{
			return ReadSegment(address, 0, length);
		}
		return Read(new string[1] { address }, new int[1] { length });
	}

	/// <summary>
	/// Bulk read Data information
	/// </summary>
	/// <param name="address">Name of the node </param>
	/// <returns>Result data with result object </returns>
	public OperateResult<byte[]> Read(string[] address)
	{
		if (address == null)
		{
			return new OperateResult<byte[]>("address can not be null");
		}

		int[] array = new int[address.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = 1;
		}
		return Read(address, array);
	}

	/// <summary>
	/// <b>[商业授权]</b> 批量读取多地址的数据信息，例如我可以读取两个标签的数据 "A","B[0]"， 长度为 [1, 5]，返回的是一整个的字节数组，需要自行解析。
	/// </summary>
	/// <param name="address">节点的名称</param>
	/// <param name="length">如果是数组，就为数组长度</param>
	/// <returns>带有结果对象的结果数据</returns>
	public OperateResult<byte[]> Read(string[] address, int[] length)
	{
		OperateResult<byte[], ushort, bool> operateResult = ReadWithType(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}
		return OperateResult.Ok(operateResult.Content1);
	}

	private OperateResult<byte[], ushort, bool> ReadWithType(string[] address, int[] length)
	{
		OperateResult<byte[]> operateResult = BuildReadCommand(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[], ushort, bool>(operateResult);
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[], ushort, bool>(operateResult2);
		}

		OperateResult operateResult3 = AllenBradleyHelper.CheckResponse(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[], ushort, bool>(operateResult3);
		}
		return AllenBradleyHelper.ExtractActualData(operateResult2.Content, isRead: true);
	}

	/// <summary>
	/// Read Segment Data Array form plc, use address tag name
	/// </summary>
	/// <param name="address">Tag name in plc</param>
	/// <param name="startIndex">array start index, uint byte index</param>
	/// <param name="length">array length, data item length</param>
	/// <returns>Results Bytes</returns>
	public OperateResult<byte[]> ReadSegment(string address, int startIndex, int length)
	{
		try
		{
			var list = new List<byte>();
			OperateResult<byte[], ushort, bool> operateResult2;
			do
			{
				OperateResult<byte[]> operateResult = ReadCipFromServer(AllenBradleyHelper.PackRequestReadSegment(address, startIndex, length));
				if (!operateResult.IsSuccess)
				{
					return operateResult;
				}

				operateResult2 = AllenBradleyHelper.ExtractActualData(operateResult.Content, isRead: true);
				if (!operateResult2.IsSuccess)
				{
					return OperateResult.Error<byte[]>(operateResult2);
				}

				startIndex += operateResult2.Content1.Length;
				list.AddRange(operateResult2.Content1);
			}
			while (operateResult2.Content3);

			return OperateResult.Ok(list.ToArray());
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>($"Address Wrong: {ex.Message}");
		}
	}

	private OperateResult<byte[]> ReadByCips(params byte[][] cips)
	{
		OperateResult<byte[]> operateResult = ReadCipFromServer(cips);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = AllenBradleyHelper.ExtractActualData(operateResult.Content, isRead: true);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}
		return OperateResult.Ok(operateResult2.Content1);
	}

	/// <summary>
	/// 使用CIP报文和服务器进行核心的数据交换
	/// </summary>
	/// <param name="cips">Cip commands</param>
	/// <returns>Results Bytes</returns>
	public OperateResult<byte[]> ReadCipFromServer(params byte[][] cips)
	{
		byte[] send = AllenBradleyHelper.PackCommandSpecificData(new byte[4], PackCommandService(PortSlot ?? new byte[2] { 1, Slot }, cips.ToArray()));
		var operateResult = ReadFromCoreServer(send);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = AllenBradleyHelper.CheckResponse(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}
		return OperateResult.Ok(operateResult.Content);
	}

	/// <summary>
	/// 使用EIP报文和服务器进行核心的数据交换
	/// </summary>
	/// <param name="eip">eip commands</param>
	/// <returns>Results Bytes</returns>
	public OperateResult<byte[]> ReadEipFromServer(params byte[][] eip)
	{
		byte[] send = AllenBradleyHelper.PackCommandSpecificData(eip);
		var operateResult = ReadFromCoreServer(send);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = AllenBradleyHelper.CheckResponse(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}
		return OperateResult.Ok(operateResult.Content);
	}

	/// <summary>
	/// 读取单个的bool数据信息，如果读取的是单bool变量，就直接写变量名，如果是由int组成的bool数组的一个值，一律带"i="开头访问，例如"i=A[0]"。
	/// </summary>
	/// <param name="address">节点的名称</param>
	/// <returns>带有结果对象的结果数据</returns>
	public override OperateResult<bool> ReadBool(string address)
	{
		if (address.StartsWith("i="))
		{
			address = address[2..];
			address = AllenBradleyHelper.AnalysisArrayIndex(address, out var arrayIndex);
			var operateResult = ReadBoolArray(address + $"[{arrayIndex / 32}]");
			if (!operateResult.IsSuccess)
			{
				return OperateResult.Error<bool>(operateResult);
			}
			return OperateResult.Ok(operateResult.Content[arrayIndex % 32]);
		}

		var operateResult2 = Read(address, 1);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<bool>(operateResult2);
		}
		return OperateResult.Ok(base.ByteTransform.TransBool(operateResult2.Content, 0));
	}

	/// <summary>
	/// 批量读取的bool数组信息，如果你有个Bool数组变量名为 A, 那么读第0个位，可以通过 ReadBool("A")，但是第二个位需要使用 
	/// ReadBoolArray("A[0]")   // 返回32个bool长度，0-31的索引，如果我想读取32-63的位索引，就需要 ReadBoolArray("A[1]") ，以此类推。
	/// </summary>
	/// <param name="address">节点的名称</param>
	/// <returns>带有结果对象的结果数据</returns>
	public OperateResult<bool[]> ReadBoolArray(string address)
	{
		var operateResult = Read(address, 1);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}
		return OperateResult.Ok(base.ByteTransform.TransBool(operateResult.Content, 0, operateResult.Content.Length));
	}

	/// <summary>
	/// 读取PLC的byte类型的数据。
	/// </summary>
	/// <param name="address">节点的名称</param>
	/// <returns>带有结果对象的结果数据</returns>
	public OperateResult<byte> ReadByte(string address)
	{
		return ByteTransformHelper.GetResultFromArray(Read(address, 1));
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		if (length > 1)
		{
			return await ReadSegmentAsync(address, 0, length);
		}
		return await ReadAsync(new string[1] { address }, new int[1] { length });
	}

	public async Task<OperateResult<byte[]>> ReadAsync(string[] address)
	{
		if (address == null)
		{
			return new OperateResult<byte[]>("address can not be null");
		}

		int[] length = new int[address.Length];
		for (int i = 0; i < length.Length; i++)
		{
			length[i] = 1;
		}
		return await ReadAsync(address, length);
	}

	public async Task<OperateResult<byte[]>> ReadAsync(string[] address, int[] length)
	{
		var read = await ReadWithTypeAsync(address, length);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}
		return OperateResult.Ok(read.Content1);
	}

	private async Task<OperateResult<byte[], ushort, bool>> ReadWithTypeAsync(string[] address, int[] length)
	{
		var command = BuildReadCommand(address, length);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<byte[], ushort, bool>(command);
		}

		var read = await ReadFromCoreServerAsync(command.Content);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[], ushort, bool>(read);
		}

		OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[], ushort, bool>(check);
		}
		return AllenBradleyHelper.ExtractActualData(read.Content, isRead: true);
	}

	public async Task<OperateResult<byte[]>> ReadSegmentAsync(string address, int startIndex, int length)
	{
		try
		{
			var bytesContent = new List<byte>();
			OperateResult<byte[], ushort, bool> analysis;
			do
			{
				var read = await ReadCipFromServerAsync(AllenBradleyHelper.PackRequestReadSegment(address, startIndex, length));
				if (!read.IsSuccess)
				{
					return read;
				}

				analysis = AllenBradleyHelper.ExtractActualData(read.Content, isRead: true);
				if (!analysis.IsSuccess)
				{
					return OperateResult.Error<byte[]>(analysis);
				}

				startIndex += analysis.Content1.Length;
				bytesContent.AddRange(analysis.Content1);
			}
			while (analysis.Content3);

			return OperateResult.Ok(bytesContent.ToArray());
		}
		catch (Exception ex2)
		{
			Exception ex = ex2;
			return new OperateResult<byte[]>($"Address Wrong: {ex.Message}");
		}
	}

	public async Task<OperateResult<byte[]>> ReadCipFromServerAsync(params byte[][] cips)
	{
		byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData(new byte[4], PackCommandService(PortSlot ?? new byte[2] { 1, Slot }, cips.ToArray()));
		var read = await ReadFromCoreServerAsync(commandSpecificData);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return OperateResult.Ok(read.Content);
	}

	public async Task<OperateResult<byte[]>> ReadEipFromServerAsync(params byte[][] eip)
	{
		byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData(eip);
		var read = await ReadFromCoreServerAsync(commandSpecificData);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return OperateResult.Ok(read.Content);
	}

	public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
	{
		if (address.StartsWith("i="))
		{
			address = address[2..];
			address = AllenBradleyHelper.AnalysisArrayIndex(address, out var bitIndex);
			var read2 = await ReadBoolArrayAsync(address + $"[{bitIndex / 32}]");
			if (!read2.IsSuccess)
			{
				return OperateResult.Error<bool>(read2);
			}
			return OperateResult.Ok(read2.Content[bitIndex % 32]);
		}

		var read = await ReadAsync(address, 1);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<bool>(read);
		}
		return OperateResult.Ok(base.ByteTransform.TransBool(read.Content, 0));
	}

	public async Task<OperateResult<bool[]>> ReadBoolArrayAsync(string address)
	{
		var read = await ReadAsync(address, 1);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read);
		}

		return OperateResult.Ok(ByteTransform.TransBool(read.Content, 0, read.Content.Length));
	}

	public async Task<OperateResult<byte>> ReadByteAsync(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1));
	}

	/// <summary>
	/// 枚举当前的所有的变量名字，包含结构体信息，除去系统自带的名称数据信息。
	/// </summary>
	/// <returns>结果对象</returns>
	public OperateResult<AbTagItem[]> TagEnumerator()
	{
		var list = new List<AbTagItem>();
		ushort startInstance = 0;
		while (true)
		{
			OperateResult<byte[]> operateResult = ReadCipFromServer(AllenBradleyHelper.GetEnumeratorCommand(startInstance));
			if (!operateResult.IsSuccess)
			{
				return OperateResult.Error<AbTagItem[]>(operateResult);
			}

			OperateResult<byte[], ushort, bool> operateResult2 = AllenBradleyHelper.ExtractActualData(operateResult.Content, isRead: true);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.Error<AbTagItem[]>(operateResult2);
			}

			if (operateResult.Content.Length < 43 || BitConverter.ToUInt16(operateResult.Content, 40) != 213)
			{
				break;
			}

			int num = 44;
			while (num < operateResult.Content.Length)
			{
				var abTagItem = new AbTagItem();
				abTagItem.InstanceID = BitConverter.ToUInt32(operateResult.Content, num);
				startInstance = (ushort)(abTagItem.InstanceID + 1);
				num += 4;
				ushort num2 = BitConverter.ToUInt16(operateResult.Content, num);
				num += 2;
				abTagItem.Name = Encoding.ASCII.GetString(operateResult.Content, num, num2);
				num += num2;
				abTagItem.SymbolType = BitConverter.ToUInt16(operateResult.Content, num);
				num += 2;
				if ((abTagItem.SymbolType & 0x1000) != 4096 && !abTagItem.Name.StartsWith("__"))
				{
					list.Add(abTagItem);
				}
			}

			if (!operateResult2.Content3)
			{
				return OperateResult.Ok(list.ToArray());
			}
		}
		return new OperateResult<AbTagItem[]>(ErrorCode.UnknownError.Desc());
	}

	public async Task<OperateResult<AbTagItem[]>> TagEnumeratorAsync()
	{
		var lists = new List<AbTagItem>();
		ushort instansAddress = 0;
		while (true)
		{
			var readCip = await ReadCipFromServerAsync(AllenBradleyHelper.GetEnumeratorCommand(instansAddress));
			if (!readCip.IsSuccess)
			{
				return OperateResult.Error<AbTagItem[]>(readCip);
			}

			var analysis = AllenBradleyHelper.ExtractActualData(readCip.Content, isRead: true);
			if (!analysis.IsSuccess)
			{
				return OperateResult.Error<AbTagItem[]>(analysis);
			}

			if (readCip.Content.Length < 43 || BitConverter.ToUInt16(readCip.Content, 40) != 213)
			{
				break;
			}

			int index4 = 44;
			while (index4 < readCip.Content.Length)
			{
				AbTagItem td = new AbTagItem
				{
					InstanceID = BitConverter.ToUInt32(readCip.Content, index4)
				};
				instansAddress = (ushort)(td.InstanceID + 1);
				index4 += 4;
				ushort nameLen = BitConverter.ToUInt16(readCip.Content, index4);
				index4 += 2;
				td.Name = Encoding.ASCII.GetString(readCip.Content, index4, nameLen);
				index4 += nameLen;
				td.SymbolType = BitConverter.ToUInt16(readCip.Content, index4);
				index4 += 2;
				if ((td.SymbolType & 0x1000) != 4096 && !td.Name.StartsWith("__"))
				{
					lists.Add(td);
				}
			}
			if (!analysis.Content3)
			{
				return OperateResult.Ok(lists.ToArray());
			}
		}
		return new OperateResult<AbTagItem[]>(ErrorCode.UnknownError.Desc());
	}

	private OperateResult<AbStructHandle> ReadTagStructHandle(AbTagItem structTag)
	{
		var operateResult = ReadByCips(AllenBradleyHelper.GetStructHandleCommand(structTag.SymbolType));
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<AbStructHandle>(operateResult);
		}

		if (operateResult.Content.Length >= 43 && BitConverter.ToInt32(operateResult.Content, 40) == 131)
		{
			var abStructHandle = new AbStructHandle
			{
				Count = BitConverter.ToUInt16(operateResult.Content, 44),
				TemplateObjectDefinitionSize = BitConverter.ToUInt32(operateResult.Content, 50),
				TemplateStructureSize = BitConverter.ToUInt32(operateResult.Content, 58),
				MemberCount = BitConverter.ToUInt16(operateResult.Content, 66),
				StructureHandle = BitConverter.ToUInt16(operateResult.Content, 72)
			};
			return OperateResult.Ok(abStructHandle);
		}
		return new OperateResult<AbStructHandle>(ErrorCode.UnknownError.Desc());
	}

	private List<AbTagItem> EnumSysStructItemType(byte[] Struct_Item_Type_buff, AbStructHandle structHandle)
	{
		var list = new List<AbTagItem>();
		if (Struct_Item_Type_buff.Length > 41 && Struct_Item_Type_buff[40] == 204 && Struct_Item_Type_buff[41] == 0 && Struct_Item_Type_buff[42] == 0)
		{
			int num = Struct_Item_Type_buff.Length - 40;
			byte[] array = new byte[num - 4];
			Array.Copy(Struct_Item_Type_buff, 44, array, 0, num - 4);
			byte[] array2 = new byte[structHandle.MemberCount * 8];
			Array.Copy(array, 0, array2, 0, structHandle.MemberCount * 8);
			byte[] array3 = new byte[array.Length - array2.Length + 1];
			Array.Copy(array, array2.Length - 1, array3, 0, array.Length - array2.Length + 1);
			ushort memberCount = structHandle.MemberCount;
			for (int i = 0; i < memberCount; i++)
			{
				AbTagItem abTagItem = new AbTagItem();
				int num2;
				abTagItem.SymbolType = BitConverter.ToUInt16(array2, num2 = 8 * i + 2);
				list.Add(abTagItem);
			}

			var list2 = new List<int>();
			for (int j = 0; j < array3.Length; j++)
			{
				if (array3[j] == 0)
				{
					list2.Add(j);
				}
			}

			list2.Add(array3.Length);
			for (int k = 0; k < list2.Count; k++)
			{
				if (k != 0)
				{
					int num3 = (k + 1 < list2.Count) ? (list2[k + 1] - list2[k] - 1) : 0;
					if (num3 > 0)
					{
						list[k - 1].Name = Encoding.ASCII.GetString(array3, list2[k] + 1, num3);
					}
				}
			}
		}

		return list;
	}

	private List<AbTagItem> EnumUserStructItemType(byte[] Struct_Item_Type_buff, AbStructHandle structHandle)
	{
		var list = new List<AbTagItem>();
		bool flag = false;
		int num = 0;
		if ((Struct_Item_Type_buff.Length > 41) & (Struct_Item_Type_buff[40] == 204) & (Struct_Item_Type_buff[41] == 0) & (Struct_Item_Type_buff[42] == 0))
		{
			int num2 = Struct_Item_Type_buff.Length - 40;
			byte[] array = new byte[num2 - 4];
			Array.ConstrainedCopy(Struct_Item_Type_buff, 44, array, 0, num2 - 4);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == 0 && !flag)
				{
					num = i;
				}

				if (array[i] != 59 || array[i + 1] != 110)
				{
					continue;
				}

				flag = true;
				int num3 = i - num - 1;
				byte[] destinationArray = new byte[num3];
				Array.Copy(array, num + 1, destinationArray, 0, num3);
				byte[] array2 = new byte[i + 1];
				Array.Copy(array, 0, array2, 0, i + 1);
				byte[] array3 = new byte[array.Length - i - 1];
				Array.Copy(array, i + 1, array3, 0, array.Length - i - 1);
				if ((num + 1) % 8 != 0)
				{
					break;
				}

				int num4 = (num + 1) / 8 - 1;
				for (int j = 0; j <= num4; j++)
				{
					var abTagItem = new AbTagItem();
					abTagItem.SymbolType = BitConverter.ToUInt16(array2, 8 * j + 2);
					list.Add(abTagItem);
				}

				var list2 = new List<int>();
				for (int k = 0; k < array3.Length; k++)
				{
					if (array3[k] == 0)
					{
						list2.Add(k);
					}
				}

				list2.Add(array3.Length);
				for (int l = 0; l < list2.Count; l++)
				{
					int num6 = ((l + 1 < list2.Count) ? (list2[l + 1] - list2[l] - 1) : 0);
					if (num6 > 0)
					{
						list[l].Name = Encoding.ASCII.GetString(array3, list2[l] + 1, num6);
					}
				}
				break;
			}
		}
		return list;
	}

	public override OperateResult<short[]> ReadInt16(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransInt16(m, 0, length));
	}

	public override OperateResult<ushort[]> ReadUInt16(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransUInt16(m, 0, length));
	}

	public override OperateResult<int[]> ReadInt32(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransInt32(m, 0, length));
	}

	public override OperateResult<uint[]> ReadUInt32(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransUInt32(m, 0, length));
	}

	public override OperateResult<float[]> ReadFloat(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransSingle(m, 0, length));
	}

	public override OperateResult<long[]> ReadInt64(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransInt64(m, 0, length));
	}

	public override OperateResult<ulong[]> ReadUInt64(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransUInt64(m, 0, length));
	}

	public override OperateResult<double[]> ReadDouble(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransDouble(m, 0, length));
	}

	public OperateResult<string> ReadString(string address)
	{
		return ReadString(address, 1, Encoding.ASCII);
	}

	public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
	{
		OperateResult<byte[]> operateResult = Read(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult);
		}

		try
		{
			if (operateResult.Content.Length >= 6)
			{
				int count = base.ByteTransform.TransInt32(operateResult.Content, 2);
				return OperateResult.Ok(encoding.GetString(operateResult.Content, 6, count));
			}
			return OperateResult.Ok(encoding.GetString(operateResult.Content));
		}
		catch (Exception ex)
		{
			return new OperateResult<string>($"{ex.Message} Source: {operateResult.Content.ToHexString(' ')}");
		}
	}

	public override async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransInt16(m, 0, length));
	}

	public override async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransUInt16(m, 0, length));
	}

	public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransInt32(m, 0, length));
	}

	public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransUInt32(m, 0, length));
	}

	public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransSingle(m, 0, length));
	}

	public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransInt64(m, 0, length));
	}

	public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransUInt64(m, 0, length));
	}

	public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransDouble(m, 0, length));
	}

	public async Task<OperateResult<string>> ReadStringAsync(string address)
	{
		return await ReadStringAsync(address, 1, Encoding.ASCII);
	}

	public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
	{
		OperateResult<byte[]> read = await ReadAsync(address, length);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<string>(read);
		}

		if (read.Content.Length >= 6)
		{
			return OperateResult.Ok(encoding.GetString(count: ByteTransform.TransInt32(read.Content, 2), bytes: read.Content, index: 6));
		}
		return OperateResult.Ok(encoding.GetString(read.Content));
	}

	/// <summary>
	/// 当前的PLC不支持该功能，需要调用 <see cref="WriteTag(string,ushort,byte[],int)" /> 方法来实现。
	/// </summary>
	/// <param name="address">地址</param>
	/// <param name="value">值</param>
	/// <returns>写入结果值</returns>
	public override OperateResult Write(string address, byte[] value)
	{
		return new OperateResult("NotSupportedFunction, Please refer to use WriteTag instead ");
	}

	/// <summary>
	/// 使用指定的类型写入指定的节点数据。
	/// </summary>
	/// <param name="address">节点的名称</param>
	/// <param name="typeCode">类型代码，详细参见<see cref="T:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper" />上的常用字段</param>
	/// <param name="value">实际的数据值</param>
	/// <param name="length">如果节点是数组，就是数组长度</param>
	/// <returns>是否写入成功</returns>
	public virtual OperateResult WriteTag(string address, ushort typeCode, byte[] value, int length = 1)
	{
		OperateResult<byte[]> operateResult = BuildWriteCommand(address, typeCode, value, length);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult operateResult3 = AllenBradleyHelper.CheckResponse(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return AllenBradleyHelper.ExtractActualData(operateResult2.Content, isRead: false);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		return await Task.Run(() => Write(address, value));
	}

	public virtual async Task<OperateResult> WriteTagAsync(string address, ushort typeCode, byte[] value, int length = 1)
	{
		OperateResult<byte[]> command = BuildWriteCommand(address, typeCode, value, length);
		if (!command.IsSuccess)
		{
			return command;
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return AllenBradleyHelper.ExtractActualData(read.Content, isRead: false);
	}

	public override OperateResult Write(string address, short[] values)
	{
		return WriteTag(address, 195, ByteTransform.TransByte(values), values.Length);
	}

	public override OperateResult Write(string address, ushort[] values)
	{
		return WriteTag(address, 199, ByteTransform.TransByte(values), values.Length);
	}

	public override OperateResult Write(string address, int[] values)
	{
		return WriteTag(address, 196, ByteTransform.TransByte(values), values.Length);
	}

	public override OperateResult Write(string address, uint[] values)
	{
		return WriteTag(address, 200, ByteTransform.TransByte(values), values.Length);
	}

	public override OperateResult Write(string address, float[] values)
	{
		return WriteTag(address, 202, ByteTransform.TransByte(values), values.Length);
	}

	public override OperateResult Write(string address, long[] values)
	{
		return WriteTag(address, 197, ByteTransform.TransByte(values), values.Length);
	}

	public override OperateResult Write(string address, ulong[] values)
	{
		return WriteTag(address, 201, ByteTransform.TransByte(values), values.Length);
	}

	public override OperateResult Write(string address, double[] values)
	{
		return WriteTag(address, 203, ByteTransform.TransByte(values), values.Length);
	}

	public override OperateResult Write(string address, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			value = string.Empty;
		}

		byte[] bytes = Encoding.ASCII.GetBytes(value);
		OperateResult operateResult = Write($"{address}.LEN", bytes.Length);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		byte[] value2 = SoftBasic.ArrayExpandToLengthEven(bytes);
		return WriteTag($"{address}.DATA[0]", 194, value2, bytes.Length);
	}

	/// <summary>
	/// 写入单个Bool的数据信息。如果读取的是单bool变量，就直接写变量名，如果是bool数组的一个值，一律带下标访问，例如a[0]。
	/// </summary>
	/// <param name="address">标签的地址数据</param>
	/// <param name="value">bool数据值</param>
	/// <returns>是否写入成功</returns>
	public override OperateResult Write(string address, bool value)
	{
		if (Regex.IsMatch(address, "\\[[0-9]+\\]$"))
		{
			OperateResult<byte[]> operateResult = BuildWriteCommand(address, value);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}

			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}

			OperateResult operateResult3 = AllenBradleyHelper.CheckResponse(operateResult2.Content);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult3);
			}
			return AllenBradleyHelper.ExtractActualData(operateResult2.Content, isRead: false);
		}
		return WriteTag(address, 193, (!value) ? new byte[2] : new byte[2] { 255, 255 });
	}

	/// <summary>
	/// 写入Byte数据，返回是否写入成功。
	/// </summary>
	/// <param name="address">标签的地址数据</param>
	/// <param name="value">Byte数据</param>
	/// <returns>是否写入成功</returns>
	public virtual OperateResult Write(string address, byte value)
	{
		return WriteTag(address, 194, new byte[2] { value, 0 });
	}

	public override async Task<OperateResult> WriteAsync(string address, short[] values)
	{
		return await WriteTagAsync(address, 195, ByteTransform.TransByte(values), values.Length);
	}

	public override async Task<OperateResult> WriteAsync(string address, ushort[] values)
	{
		return await WriteTagAsync(address, 199, ByteTransform.TransByte(values), values.Length);
	}

	public override async Task<OperateResult> WriteAsync(string address, int[] values)
	{
		return await WriteTagAsync(address, 196, ByteTransform.TransByte(values), values.Length);
	}

	public override async Task<OperateResult> WriteAsync(string address, uint[] values)
	{
		return await WriteTagAsync(address, 200, ByteTransform.TransByte(values), values.Length);
	}

	public override async Task<OperateResult> WriteAsync(string address, float[] values)
	{
		return await WriteTagAsync(address, 202, ByteTransform.TransByte(values), values.Length);
	}

	public override async Task<OperateResult> WriteAsync(string address, long[] values)
	{
		return await WriteTagAsync(address, 197, ByteTransform.TransByte(values), values.Length);
	}

	public override async Task<OperateResult> WriteAsync(string address, ulong[] values)
	{
		return await WriteTagAsync(address, 201, ByteTransform.TransByte(values), values.Length);
	}

	public override async Task<OperateResult> WriteAsync(string address, double[] values)
	{
		return await WriteTagAsync(address, 203, ByteTransform.TransByte(values), values.Length);
	}

	public override async Task<OperateResult> WriteAsync(string address, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			value = string.Empty;
		}

		byte[] data = Encoding.ASCII.GetBytes(value);
		OperateResult write = await WriteAsync($"{address}.LEN", data.Length);
		if (!write.IsSuccess)
		{
			return write;
		}
		return await WriteTagAsync($"{address}.DATA[0]", 194, SoftBasic.ArrayExpandToLengthEven(data), data.Length);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool value)
	{
		if (Regex.IsMatch(address, "\\[[0-9]+\\]$"))
		{
			OperateResult<byte[]> command = BuildWriteCommand(address, value);
			if (!command.IsSuccess)
			{
				return command;
			}

			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess)
			{
				return read;
			}

			OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
			if (!check.IsSuccess)
			{
				return OperateResult.Error<byte[]>(check);
			}
			return AllenBradleyHelper.ExtractActualData(read.Content, isRead: false);
		}

		return await WriteTagAsync(address, 193, (!value) ? new byte[2] : new byte[2] { 255, 255 });
	}

	public virtual async Task<OperateResult> WriteAsync(string address, byte value)
	{
		return await WriteTagAsync(address, 194, new byte[2] { value, 0 });
	}

	protected virtual byte[] PackCommandService(byte[] portSlot, params byte[][] cips)
	{
		return AllenBradleyHelper.PackCommandService(portSlot, cips);
	}

	public override string ToString()
	{
		return $"AllenBradleyNet[{IpAddress}:{Port}]";
	}
}
