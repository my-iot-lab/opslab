using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;
using Ops.Communication.Extensions;
using Ops.Communication.Profinet.AllenBradley;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Omron;

/// <summary>
/// 基于连接的对象访问的CIP协议的实现，用于对Omron PLC进行标签的数据读写，对数组，多维数组进行读写操作，支持的数据类型请参照API文档手册。
/// </summary>
/// <remarks>
/// 支持普通标签的读写，类型要和标签对应上。如果标签是数组，例如 A 是 INT[0...9] 那么Read("A", 1)，返回的是10个short所有字节数组。
/// 如果需要返回10个长度的short数组，请调用 ReadInt16("A[0], 10"); 地址必须写 "A[0]"，不能写 "A" , 如需要读取结构体，参考 <see cref="ReadStruct``1(System.String)" />
/// </remarks>
/// <example>
/// 首先说明支持的类型地址，在PLC里支持了大量的类型，有些甚至在C#里是不存在的。现在做个统一的声明
/// <list type="table">
///   <listheader>
///     <term>PLC类型</term>
///     <term>含义</term>
///     <term>代号</term>
///     <term>C# 类型</term>
///     <term>备注</term>
///   </listheader>
///   <item>
///     <term>bool</term>
///     <term>位类型数据</term>
///     <term>0xC1</term>
///     <term>bool</term>
///     <term></term>
///   </item>
///   <item>
///     <term>SINT</term>
///     <term>8位的整型</term>
///     <term>0xC2</term>
///     <term>sbyte</term>
///     <term>有符号8位很少用，HSL直接用byte</term>
///   </item>
///   <item>
///     <term>USINT</term>
///     <term>无符号8位的整型</term>
///     <term>0xC6</term>
///     <term>byte</term>
///     <term>如需要，使用<see cref="WriteTag(System.String,System.UInt16,System.Byte[],System.Int32)" />实现</term>
///   </item>
///   <item>
///     <term>BYTE</term>
///     <term>8位字符数据</term>
///     <term>0xD1</term>
///     <term>byte</term>
///     <term>如需要，使用<see cref="WriteTag(System.String,System.UInt16,System.Byte[],System.Int32)" />实现</term>
///   </item>
///   <item>
///     <term>INT</term>
///     <term>16位的整型</term>
///     <term>0xC3</term>
///     <term>short</term>
///     <term></term>
///   </item>
///   <item>
///     <term>UINT</term>
///     <term>无符号的16位整型</term>
///     <term>0xC7</term>
///     <term>ushort</term>
///     <term></term>
///   </item>
///   <item>
///     <term>DINT</term>
///     <term>32位的整型</term>
///     <term>0xC4</term>
///     <term>int</term>
///     <term></term>
///   </item>
///   <item>
///     <term>UDINT</term>
///     <term>无符号的32位整型</term>
///     <term>0xC8</term>
///     <term>uint</term>
///     <term></term>
///   </item>
///   <item>
///     <term>LINT</term>
///     <term>64位的整型</term>
///     <term>0xC5</term>
///     <term>long</term>
///     <term></term>
///   </item>
///   <item>
///     <term>ULINT</term>
///     <term>无符号的64位的整型</term>
///     <term>0xC9</term>
///     <term>ulong</term>
///     <term></term>
///   </item>
///   <item>
///     <term>REAL</term>
///     <term>单精度浮点数</term>
///     <term>0xCA</term>
///     <term>float</term>
///     <term></term>
///   </item>
///   <item>
///     <term>DOUBLE</term>
///     <term>双精度浮点数</term>
///     <term>0xCB</term>
///     <term>double</term>
///     <term></term>
///   </item>
///   <item>
///     <term>STRING</term>
///     <term>字符串数据</term>
///     <term>0xD0</term>
///     <term>string</term>
///     <term>前两个字节为字符长度</term>
///   </item>
///   <item>
///     <term>8bit string BYTE</term>
///     <term>8位的字符串</term>
///     <term>0xD1</term>
///     <term></term>
///     <term>本质是BYTE数组</term>
///   </item>
///   <item>
///     <term>16bit string WORD</term>
///     <term>16位的字符串</term>
///     <term>0xD2</term>
///     <term></term>
///     <term>本质是WORD数组，可存放中文</term>
///   </item>
///   <item>
///     <term>32bit string DWORD</term>
///     <term>32位的字符串</term>
///     <term>0xD2</term>
///     <term></term>
///     <term>本质是DWORD数组，可存放中文</term>
///   </item>
/// </list>
/// 在读写操作之前，先看看怎么实例化和连接PLC<br />
/// 现在来说明以下具体的操作细节。我们假设有如下的变量：<br />
/// CESHI_A       SINT<br />
/// CESHI_B       BYTE<br />
/// CESHI_C       INT<br />
/// CESHI_D       UINT<br />
/// CESHI_E       SINT[0..9]<br />
/// CESHI_F       BYTE[0..9]<br />
/// CESHI_G       INT[0..9]<br />
/// CESHI_H       UINT[0..9]<br />
/// CESHI_I       INT[0..511]<br />
/// CESHI_J       STRING[12]<br />
/// ToPc_ID1      ARRAY[0..99] OF STRING[20]<br />
/// CESHI_O       BOOL<br />
/// CESHI_P       BOOL[0..31]<br />
/// 对 CESHI_A 来说，读写这么操作
/// 对于 CESHI_B 来说，写入的操作有点特殊
/// 对于 CESHI_C, CESHI_D 来说，就是 ReadInt16(string address) , Write( string address, short value ) 和 ReadUInt16(string address) 和 Write( string address, ushort value ) 差别不大。
/// 所以我们着重来看看数组的情况，以 CESHI_G 标签为例子:<br />
/// 情况一，我想一次读取这个标签所有的字节数组（当长度满足的情况下，会一次性返回数据）
/// 情况二，我想读取第3个数，或是第6个数开始，一共5个数
/// 其他的数组情况都是类似的，我们来看看字符串 CESHI_J 变量
/// 对于 bool 变量来说，就是 ReadBool("CESHI_O") 和 Write("CESHI_O", true) 操作，如果是bool数组，就不一样了
/// 最后我们来看看结构体的操作，假设我们有个结构体<br />
/// MyData.Code     STRING(12)<br />
/// MyData.Value1   INT<br />
/// MyData.Value2   INT<br />
/// MyData.Value3   REAL<br />
/// MyData.Value4   INT<br />
/// MyData.Value5   INT<br />
/// MyData.Value6   INT[0..3]<br />
/// 因为bool比较复杂，暂时不考虑。要读取上述的结构体，我们需要定义结构一样的数据
/// 定义好后，我们再来读取就很简单了。
/// </example>
public class OmronConnectedCipNet : NetworkDeviceBase
{
	/// <summary>
	/// O -&gt; T Network Connection ID
	/// </summary>
	private uint OTConnectionId = 0u;

	private IncrementCount incrementCount = new IncrementCount(65535L, 0L);

	public uint SessionHandle { get; protected set; }

	/// <summary>
	/// 当前产品的型号信息。
	/// </summary>
	public string ProductName { get; private set; }

	/// <summary>
	/// 实例化一个默认的对象
	/// </summary>
	public OmronConnectedCipNet()
	{
		base.WordLength = 2;
		base.ByteTransform = new RegularByteTransform();
	}

	/// <summary>
	/// 根据指定的IP及端口来实例化这个连接对象
	/// </summary>
	/// <param name="ipAddress">PLC的Ip地址</param>
	/// <param name="port">PLC的端口号信息</param>
	public OmronConnectedCipNet(string ipAddress, int port = 44818)
	{
		IpAddress = ipAddress;
		Port = port;
		base.WordLength = 2;
		base.ByteTransform = new RegularByteTransform();
	}

	protected override INetMessage GetNewNetMessage()
	{
		return new AllenBradleyMessage();
	}

	protected override byte[] PackCommandWithHeader(byte[] command)
	{
		return AllenBradleyHelper.PackRequestHeader(112, SessionHandle, AllenBradleyHelper.PackCommandSpecificData(GetOTConnectionIdService(), command));
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

		SessionHandle = base.ByteTransform.TransUInt32(operateResult.Content, 4);
		var operateResult3 = ReadFromCoreServer(socket, AllenBradleyHelper.PackRequestHeader(111, SessionHandle, GetLargeForwardOpen()), true, false);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}

		if (operateResult3.Content[42] != 0)
		{
			if (base.ByteTransform.TransUInt16(operateResult3.Content, 44) == 256)
			{
				return new OperateResult("Connection in use or duplicate Forward Open");
			}
			return new OperateResult("Forward Open failed, Code: " + base.ByteTransform.TransUInt16(operateResult3.Content, 44));
		}

		OTConnectionId = ByteTransform.TransUInt32(operateResult3.Content, 44);
		incrementCount.ResetCurrentValue();
		var operateResult4 = ReadFromCoreServer(socket, AllenBradleyHelper.PackRequestHeader(111, SessionHandle, GetAttributeAll()), true, false);
		if (!operateResult4.IsSuccess)
		{
			return operateResult4;
		}

		if (operateResult4.Content.Length > 59)
		{
			ProductName = Encoding.UTF8.GetString(operateResult4.Content, 59, operateResult4.Content[58]);
		}
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
		var read1 = await ReadFromCoreServerAsync(socket, AllenBradleyHelper.RegisterSessionHandle(), true, false);
		if (!read1.IsSuccess)
		{
			return read1;
		}

		OperateResult check = AllenBradleyHelper.CheckResponse(read1.Content);
		if (!check.IsSuccess)
		{
			return check;
		}

		SessionHandle = ByteTransform.TransUInt32(read1.Content, 4);
		var read2 = await ReadFromCoreServerAsync(socket, AllenBradleyHelper.PackRequestHeader(111, SessionHandle, GetLargeForwardOpen()), true, false);
		if (!read2.IsSuccess)
		{
			return read2;
		}

		if (read2.Content[42] != 0)
		{
			if (ByteTransform.TransUInt16(read2.Content, 44) == 256)
			{
				return new OperateResult("Connection in use or duplicate Forward Open");
			}
			return new OperateResult("Forward Open failed, Code: " + ByteTransform.TransUInt16(read2.Content, 44));
		}

		OTConnectionId = ByteTransform.TransUInt32(read2.Content, 44);
		incrementCount.ResetCurrentValue();
		var read3 = await ReadFromCoreServerAsync(socket, AllenBradleyHelper.PackRequestHeader(111, SessionHandle, GetAttributeAll()), true, false);
		if (!read3.IsSuccess)
		{
			return read3;
		}

		if (read3.Content.Length > 59)
		{
			ProductName = Encoding.UTF8.GetString(read3.Content, 59, read3.Content[58]);
		}
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

	private byte[] GetOTConnectionIdService()
	{
		byte[] array = new byte[8] { 161, 0, 4, 0, 0, 0, 0, 0 };
		ByteTransform.TransByte(OTConnectionId).CopyTo(array, 4);
		return array;
	}

	private OperateResult<byte[]> BuildReadCommand(string[] address, ushort[] length)
	{
		try
		{
			var list = new List<byte[]>();
			for (int i = 0; i < address.Length; i++)
			{
				list.Add(AllenBradleyHelper.PackRequsetRead(address[i], length[i], isConnectedAddress: true));
			}
			return OperateResult.Ok(PackCommandService(list.ToArray()));
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
		}
	}

	private OperateResult<byte[]> BuildWriteCommand(string address, ushort typeCode, byte[] data, int length = 1)
	{
		try
		{
			return OperateResult.Ok(PackCommandService(AllenBradleyHelper.PackRequestWrite(address, typeCode, data, length, true)));
		}
		catch (Exception ex)
		{
			return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
		}
	}

	private byte[] PackCommandService(params byte[][] cip)
	{
		using var memoryStream = new MemoryStream();
		memoryStream.WriteByte(177);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		long currentValue = incrementCount.GetCurrentValue();
		memoryStream.WriteByte(BitConverter.GetBytes(currentValue)[0]);
		memoryStream.WriteByte(BitConverter.GetBytes(currentValue)[1]);
		if (cip.Length == 1)
		{
			memoryStream.Write(cip[0], 0, cip[0].Length);
		}
		else
		{
			memoryStream.Write(new byte[6] { 10, 2, 32, 2, 36, 1 }, 0, 6);
			memoryStream.WriteByte(BitConverter.GetBytes(cip.Length)[0]);
			memoryStream.WriteByte(BitConverter.GetBytes(cip.Length)[1]);
			int num = 2 + cip.Length * 2;
			for (int i = 0; i < cip.Length; i++)
			{
				memoryStream.WriteByte(BitConverter.GetBytes(num)[0]);
				memoryStream.WriteByte(BitConverter.GetBytes(num)[1]);
				num += cip[i].Length;
			}
			for (int j = 0; j < cip.Length; j++)
			{
				memoryStream.Write(cip[j], 0, cip[j].Length);
			}
		}

		byte[] array = memoryStream.ToArray();
		BitConverter.GetBytes((ushort)(array.Length - 4)).CopyTo(array, 2);
		return array;
	}

	private OperateResult<byte[], ushort, bool> ReadWithType(string[] address, ushort[] length)
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
		return ExtractActualData(operateResult2.Content, isRead: true);
	}

	public OperateResult<byte[]> ReadCipFromServer(params byte[][] cips)
	{
		byte[] send = PackCommandService(cips.ToArray());
		OperateResult<byte[]> operateResult = ReadFromCoreServer(send);
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
	/// <b>[商业授权]</b> 读取一个结构体的对象，需要事先根据实际的数据点位定义好结构体，然后使用本方法进行读取，当结构体定义不对时，本方法将会读取失败。
	/// </summary>
	/// <remarks>
	/// 本方法需要商业授权支持，具体的使用方法，参考API文档的示例代码
	/// </remarks>
	/// <example>
	/// 我们来看看结构体的操作，假设我们有个结构体<br />
	/// MyData.Code     STRING(12)<br />
	/// MyData.Value1   INT<br />
	/// MyData.Value2   INT<br />
	/// MyData.Value3   REAL<br />
	/// MyData.Value4   INT<br />
	/// MyData.Value5   INT<br />
	/// MyData.Value6   INT[0..3]<br />
	/// 因为bool比较复杂，暂时不考虑。要读取上述的结构体，我们需要定义结构一样的数据
	/// 定义好后，我们再来读取就很简单了。
	/// </example>
	/// <typeparam name="T">结构体的类型</typeparam>
	/// <param name="address">结构体对象的地址</param>
	/// <returns>是否读取成功的对象</returns>
	public OperateResult<T> ReadStruct<T>(string address) where T : struct
	{
		OperateResult<byte[]> operateResult = Read(address, 1);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<T>(operateResult);
		}
		return OpsHelper.ByteArrayToStruct<T>(operateResult.Content.RemoveBegin(2));
	}

	private async Task<OperateResult<byte[], ushort, bool>> ReadWithTypeAsync(string[] address, ushort[] length)
	{
		OperateResult<byte[]> command = BuildReadCommand(address, length);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<byte[], ushort, bool>(command);
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[], ushort, bool>(read);
		}

		OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[], ushort, bool>(check);
		}
		return ExtractActualData(read.Content, isRead: true);
	}

	public async Task<OperateResult<byte[]>> ReadCipFromServerAsync(params byte[][] cips)
	{
		byte[] command = PackCommandService(cips.ToArray());
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(command);
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

	public async Task<OperateResult<T>> ReadStructAsync<T>(string address) where T : struct
	{
		OperateResult<byte[]> read = await ReadAsync(address, 1);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<T>(read);
		}
		return OpsHelper.ByteArrayToStruct<T>(read.Content.RemoveBegin(2));
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		OperateResult<byte[], ushort, bool> operateResult = ReadWithType(new string[1] { address }, new ushort[1] { length });
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}
		return OperateResult.Ok(operateResult.Content1);
	}

	public OperateResult<byte[]> Read(string[] address, ushort[] length)
	{
		OperateResult<byte[], ushort, bool> operateResult = ReadWithType(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}
		return OperateResult.Ok(operateResult.Content1);
	}

	/// <summary>
	/// 读取bool数据信息，如果读取的是单bool变量，就直接写变量名，如果是 bool 数组，就在开头使用 "i=" 访问，如 "i=A[0]"。
	/// </summary>
	/// <param name="address">节点的名称</param>
	/// <param name="length">读取的数组长度信息</param>
	/// <returns>带有结果对象的结果数据</returns>
	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		if (length == 1 && !Regex.IsMatch(address, "\\[[0-9]+\\]$"))
		{
			OperateResult<byte[]> operateResult = Read(address, length);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult);
			}
			return OperateResult.Ok(SoftBasic.ByteToBoolArray(operateResult.Content));
		}

		OperateResult<byte[]> operateResult2 = Read(address, length);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult2);
		}
		return OperateResult.Ok(operateResult2.Content.Select((byte m) => m != 0).Take(length).ToArray());
	}

	/// <summary>
	/// 读取PLC的byte类型的数据。
	/// </summary>
	/// <param name="address">节点的名称</param>
	/// <returns>带有结果对象的结果数据 </returns>
	public OperateResult<byte> ReadByte(string address)
	{
		return ByteTransformHelper.GetResultFromArray(Read(address, 1));
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		if (length == 1 && !Regex.IsMatch(address, "\\[[0-9]+\\]$"))
		{
			OperateResult<byte[]> read = await ReadAsync(address, length);
			if (!read.IsSuccess)
			{
				return OperateResult.Error<bool[]>(read);
			}
			return OperateResult.Ok(SoftBasic.ByteToBoolArray(read.Content));
		}

		OperateResult<byte[]> read2 = await ReadAsync(address, length);
		if (!read2.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read2);
		}
		return OperateResult.Ok(read2.Content.Select((byte m) => m != 0).Take(length).ToArray());
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		OperateResult<byte[], ushort, bool> read = await ReadWithTypeAsync(new string[1] { address }, new ushort[1] { length });
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}
		return OperateResult.Ok(read.Content1);
	}

	public async Task<OperateResult<byte[]>> ReadAsync(string[] address, ushort[] length)
	{
		OperateResult<byte[], ushort, bool> read = await ReadWithTypeAsync(address, length);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}
		return OperateResult.Ok(read.Content1);
	}

	public async Task<OperateResult<byte>> ReadByteAsync(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1));
	}

	/// <summary>
	/// 当前的PLC不支持该功能，需要调用 <see cref="WriteTag(string,ushort,byte[],int)" /> 方法来实现。
	/// </summary>
	/// <param name="address">地址</param>
	/// <param name="value">值</param>
	/// <returns>写入结果值</returns>
	public override OperateResult Write(string address, byte[] value)
	{
		return new OperateResult($"{ErrorCode.NotSupportedFunction.Desc()} Please refer to use WriteTag instead ");
	}

	/// <summary>
	/// 使用指定的类型写入指定的节点数据。
	/// </summary>
	/// <param name="address">节点的名称</param>
	/// <param name="typeCode">类型代码，详细参见<see cref="AllenBradleyHelper" />上的常用字段</param>
	/// <param name="value">实际的数据值 -&gt; The actual data value </param>
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
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), (byte[] m) => ByteTransform.TransUInt64(m, 0, length));
	}

	public override OperateResult<double[]> ReadDouble(string address, ushort length)
	{
		return ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransDouble(m, 0, length));
	}

	public OperateResult<string> ReadString(string address)
	{
		return ReadString(address, 1, Encoding.UTF8);
	}

	/// <summary>
	/// 读取字符串数据，默认为UTF-8编码。
	/// </summary>
	/// <param name="address">起始地址</param>
	/// <param name="length">数据长度</param>
	/// <returns>带有成功标识的string数据</returns>
	public override OperateResult<string> ReadString(string address, ushort length)
	{
		return ReadString(address, length, Encoding.UTF8);
	}

	public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
	{
		OperateResult<byte[]> operateResult = Read(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult);
		}

		if (operateResult.Content.Length >= 2)
		{
			int count = ByteTransform.TransUInt16(operateResult.Content, 0);
			return OperateResult.Ok(encoding.GetString(operateResult.Content, 2, count));
		}

		return OperateResult.Ok(encoding.GetString(operateResult.Content));
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
		return await ReadStringAsync(address, 1, Encoding.UTF8);
	}

	public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
	{
		return await ReadStringAsync(address, length, Encoding.UTF8);
	}

	public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
	{
		OperateResult<byte[]> read = await ReadAsync(address, length);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<string>(read);
		}
		if (read.Content.Length >= 2)
		{
			return OperateResult.Ok(encoding.GetString(count: ByteTransform.TransUInt16(read.Content, 0), bytes: read.Content, index: 2));
		}
		return OperateResult.Ok(encoding.GetString(read.Content));
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
		return WriteTag(address, 201, base.ByteTransform.TransByte(values), values.Length);
	}

	public override OperateResult Write(string address, double[] values)
	{
		return WriteTag(address, 203, base.ByteTransform.TransByte(values), values.Length);
	}

	public override OperateResult Write(string address, string value)
	{
		byte[] array = (string.IsNullOrEmpty(value) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(value));
		return WriteTag(address, 208, SoftBasic.SpliceArray(BitConverter.GetBytes((ushort)array.Length), array));
	}

	public override OperateResult Write(string address, bool value)
	{
		return WriteTag(address, 193, (!value) ? new byte[2] : new byte[2] { 255, 255 });
	}

	public OperateResult Write(string address, byte value)
	{
		return WriteTag(address, 194, new byte[1] { value });
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
		byte[] buffer = string.IsNullOrEmpty(value) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(value);
		return await WriteTagAsync(address, 208, SoftBasic.SpliceArray(BitConverter.GetBytes((ushort)buffer.Length), buffer));
	}

	public override async Task<OperateResult> WriteAsync(string address, bool value)
	{
		return await WriteTagAsync(address, 193, !value ? new byte[2] : new byte[2] { 255, 255 });
	}

	public async Task<OperateResult> WriteAsync(string address, byte value)
	{
		return await WriteTagAsync(address, 194, new byte[1] { value });
	}

	private byte[] GetLargeForwardOpen()
	{
		return "00 00 00 00 00 00 02 00 00 00 00 00 b2 00 34 00\r\n5b 02 20 06 24 01 06 9c 02 00 00 80 01 00 fe 80\r\n02 00 1b 05 30 a7 2b 03 02 00 00 00 80 84 1e 00\r\ncc 07 00 42 80 84 1e 00 cc 07 00 42 a3 03 20 02\r\n24 01 2c 01".ToHexBytes();
	}

	private byte[] GetAttributeAll()
	{
		return "00 00 00 00 00 00 02 00 00 00 00 00 b2 00 06 00 01 02 20 01 24 01".ToHexBytes();
	}

	/// <summary>
	/// 从PLC反馈的数据解析
	/// </summary>
	/// <param name="response">PLC的反馈数据</param>
	/// <param name="isRead">是否是返回的操作</param>
	/// <returns>带有结果标识的最终数据</returns>
	public static OperateResult<byte[], ushort, bool> ExtractActualData(byte[] response, bool isRead)
	{
		var list = new List<byte>();
		int num = 42;
		bool value = false;
		ushort value2 = 0;
		ushort num2 = BitConverter.ToUInt16(response, num);
		if (BitConverter.ToInt32(response, 46) == 138)
		{
			num = 50;
			int num3 = BitConverter.ToUInt16(response, num);
			for (int i = 0; i < num3; i++)
			{
				int num4 = BitConverter.ToUInt16(response, num + 2 + i * 2) + num;
				int num5 = ((i == num3 - 1) ? response.Length : (BitConverter.ToUInt16(response, num + 4 + i * 2) + num));
				ushort num6 = BitConverter.ToUInt16(response, num4 + 2);
				switch (num6)
				{
					case 4:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = ErrorCode.AllenBradley04.Desc(),
						};
					case 5:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = ErrorCode.AllenBradley05.Desc(),
						};
					case 6:
						if (response[num + 2] == 210 || response[num + 2] == 204)
						{
							return new OperateResult<byte[], ushort, bool>
							{
								ErrorCode = num6,
								Message = ErrorCode.AllenBradley06.Desc(),
							};
						}
						break;
					case 10:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = ErrorCode.AllenBradley0A.Desc(),
						};
					case 19:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = ErrorCode.AllenBradley13.Desc(),
						};
					case 28:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = ErrorCode.AllenBradley1C.Desc(),
						};
					case 30:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = ErrorCode.AllenBradley1E.Desc(),
						};
					case 38:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = ErrorCode.AllenBradley26.Desc(),
						};
					default:
						return new OperateResult<byte[], ushort, bool>
						{
							ErrorCode = num6,
							Message = ErrorCode.UnknownError.Desc(),
						};
					case 0:
						break;
				}
				if (isRead)
				{
					for (int j = num4 + 6; j < num5; j++)
					{
						list.Add(response[j]);
					}
				}
			}
		}
		else
		{
			byte b = response[num + 6];
			switch (b)
			{
				case 4:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = ErrorCode.AllenBradley04.Desc(),
					};
				case 5:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = ErrorCode.AllenBradley05.Desc(),
					};
				case 6:
					value = true;
					break;
				case 10:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = ErrorCode.AllenBradley0A.Desc(),
					};
				case 19:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = ErrorCode.AllenBradley13.Desc(),
					};
				case 28:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = ErrorCode.AllenBradley1C.Desc(),
					};
				case 30:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = ErrorCode.AllenBradley1E.Desc(),
					};
				case 38:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = ErrorCode.AllenBradley26.Desc(),
					};
				default:
					return new OperateResult<byte[], ushort, bool>
					{
						ErrorCode = b,
						Message = ErrorCode.UnknownError.Desc(),
					};
				case 0:
					break;
			}

			if (response[num + 4] == 205 || response[num + 4] == 211)
			{
				return OperateResult.Ok(list.ToArray(), value2, value);
			}

			if (response[num + 4] == 204 || response[num + 4] == 210)
			{
				for (int k = num + 10; k < num + 2 + num2; k++)
				{
					list.Add(response[k]);
				}
				value2 = BitConverter.ToUInt16(response, num + 8);
			}
			else if (response[num + 4] == 213)
			{
				for (int l = num + 8; l < num + 2 + num2; l++)
				{
					list.Add(response[l]);
				}
			}
		}

		return OperateResult.Ok(list.ToArray(), value2, value);
	}
}
