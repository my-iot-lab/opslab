using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Siemens;

/// <summary>
/// 使用了Fetch/Write协议来和西门子进行通讯，该种方法需要在PLC侧进行一些配置<br />
/// Using the Fetch/write protocol to communicate with Siemens, this method requires some configuration on the PLC side
/// </summary>
/// <remarks>
/// 配置的参考文章地址：https://www.cnblogs.com/dathlin/p/8685855.html
/// <br />
/// 与S7协议相比较而言，本协议不支持对单个的点位的读写操作。如果读取M100.0，需要读取M100的值，然后进行提取位数据。
///
/// 如果需要写入位地址的数据，可以读取plc的byte值，然后进行与或非，然后写入到plc之中。
/// 地址支持的列表如下：
/// <list type="table">
///   <listheader>
///     <term>地址名称</term>
///     <term>地址代号</term>
///     <term>示例</term>
///     <term>地址进制</term>
///     <term>字操作</term>
///     <term>位操作</term>
///     <term>备注</term>
///   </listheader>
///   <item>
///     <term>中间寄存器</term>
///     <term>M</term>
///     <term>M100,M200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>输入寄存器</term>
///     <term>I</term>
///     <term>I100,I200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>输出寄存器</term>
///     <term>Q</term>
///     <term>Q100,Q200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>DB块寄存器</term>
///     <term>DB</term>
///     <term>DB1.100,DB1.200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>定时器的值</term>
///     <term>T</term>
///     <term>T100,T200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>计数器的值</term>
///     <term>C</term>
///     <term>C100,C200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
/// </list>
/// </remarks>
public sealed class SiemensFetchWriteNet : NetworkDeviceBase
{
	/// <summary>
	/// 实例化一个西门子的Fetch/Write协议的通讯对象<br />
	/// Instantiate a communication object for a Siemens Fetch/write protocol
	/// </summary>
	public SiemensFetchWriteNet()
	{
		base.WordLength = 2;
		base.ByteTransform = new ReverseBytesTransform();
	}

	/// <summary>
	/// 实例化一个西门子的Fetch/Write协议的通讯对象<br />
	/// Instantiate a communication object for a Siemens Fetch/write protocol
	/// </summary>
	/// <param name="ipAddress">PLC的Ip地址 -&gt; Specify IP Address</param>
	/// <param name="port">PLC的端口 -&gt; Specify IP Port</param>
	public SiemensFetchWriteNet(string ipAddress, int port)
	{
		base.WordLength = 2;
		IpAddress = ipAddress;
		Port = port;
		base.ByteTransform = new ReverseBytesTransform();
	}

	protected override INetMessage GetNewNetMessage()
	{
		return new FetchWriteMessage();
	}

	/// <summary>
	/// 从PLC读取数据，地址格式为I100，Q100，DB20.100，M100，T100，C100，以字节为单位<br />
	/// Read data from PLC, address format I100,Q100,DB20.100,M100,T100,C100, in bytes
	/// </summary>
	/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100，T100，C100 -&gt;
	/// Starting address, formatted as I100,M100,Q100,DB20.100,T100,C100
	/// </param>
	/// <param name="length">读取的数量，以字节为单位 -&gt; The number of reads, in bytes</param>
	/// <returns>带有成功标志的字节信息 -&gt; Byte information with a success flag</returns>
	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		var operateResult = BuildReadCommand(address, length);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return OperateResult.Ok(SoftBasic.ArrayRemoveBegin(operateResult2.Content, 16));
	}

	/// <summary>
	/// 将数据写入到PLC数据，地址格式为I100，Q100，DB20.100，M100，以字节为单位<br />
	/// Writes data to the PLC data, in the address format i100,q100,db20.100,m100, in bytes
	/// </summary>
	/// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100 -&gt; Starting address, formatted as M100,I100,Q100,DB1.100</param>
	/// <param name="value">要写入的实际数据 -&gt; The actual data to write</param>
	/// <returns>是否写入成功的结果对象 -&gt; Whether to write a successful result object</returns>
	public override OperateResult Write(string address, byte[] value)
	{
		var operateResult = BuildWriteCommand(address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReadFromCoreServer(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		var command = BuildReadCommand(address, length);
		if (!command.IsSuccess)
		{
			return command;
		}

		var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return OperateResult.Ok(SoftBasic.ArrayRemoveBegin(read.Content, 16));
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		var command = BuildWriteCommand(address, value);
		if (!command.IsSuccess)
		{
			return command;
		}

		var write = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
		if (!write.IsSuccess)
		{
			return write;
		}

		OperateResult check = CheckResponseContent(write.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 读取指定地址的byte数据<br />
	/// Reads the byte data for the specified address
	/// </summary>
	/// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100 -&gt; Starting address, formatted as M100,I100,Q100,DB1.100</param>
	/// <returns>byte类型的结果对象 -&gt; Result object of type Byte</returns>
	/// <remarks>
	/// <note type="warning">
	/// 不适用于DB块，定时器，计数器的数据读取，会提示相应的错误，读取长度必须为偶数
	/// </note>
	/// </remarks>
	public OperateResult<byte> ReadByte(string address)
	{
		return ByteTransformHelper.GetResultFromArray(Read(address, 1));
	}

	/// <summary>
	/// 向PLC中写入byte数据，返回是否写入成功<br />
	/// Writes byte data to the PLC and returns whether the write succeeded
	/// </summary>
	/// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100 -&gt; Starting address, formatted as M100,I100,Q100,DB1.100</param>
	/// <param name="value">要写入的实际数据 -&gt; The actual data to write</param>
	/// <returns>是否写入成功的结果对象 -&gt; Whether to write a successful result object</returns>
	public OperateResult Write(string address, byte value)
	{
		return Write(address, new byte[1] { value });
	}

	public async Task<OperateResult<byte>> ReadByteAsync(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1).ConfigureAwait(false));
	}

	public async Task<OperateResult> WriteAsync(string address, byte value)
	{
		return await WriteAsync(address, new byte[1] { value }).ConfigureAwait(false);
	}

	public override string ToString()
	{
		return $"SiemensFetchWriteNet[{IpAddress}:{Port}]";
	}

	/// <summary>
	/// 计算特殊的地址信息<br />
	/// Calculate special address information
	/// </summary>
	/// <param name="address">字符串信息</param>
	/// <returns>实际值</returns>
	private static int CalculateAddressStarted(string address)
	{
		if (address.IndexOf('.') < 0)
		{
			return Convert.ToInt32(address);
		}
		string[] array = address.Split(['.']);
		return Convert.ToInt32(array[0]);
	}

	private static OperateResult CheckResponseContent(byte[] content)
	{
		if (content[8] != 0)
		{
			return new OperateResult(content[8], "SiemensWriteError" + content[8]);
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 解析数据地址，解析出地址类型，起始地址，DB块的地址<br />
	/// Parse data address, parse out address type, start address, db block address
	/// </summary>
	/// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100 -&gt; Starting address, formatted as M100,I100,Q100,DB1.100</param>
	/// <returns>解析出地址类型，起始地址，DB块的地址 -&gt; Resolves address type, start address, db block address</returns>
	private static OperateResult<byte, int, ushort> AnalysisAddress(string address)
	{
		OperateResult<byte, int, ushort> operateResult = new();
		try
		{
			operateResult.Content3 = 0;
			if (address[0] == 'I')
			{
				operateResult.Content1 = 3;
				operateResult.Content2 = CalculateAddressStarted(address[1..]);
			}
			else if (address[0] == 'Q')
			{
				operateResult.Content1 = 4;
				operateResult.Content2 = CalculateAddressStarted(address[1..]);
			}
			else if (address[0] == 'M')
			{
				operateResult.Content1 = 2;
				operateResult.Content2 = CalculateAddressStarted(address[1..]);
			}
			else if (address[0] == 'D' || address[..2] == "DB")
			{
				operateResult.Content1 = 1;
				string[] array = address.Split(['.']);
				if (address[1] == 'B')
				{
					operateResult.Content3 = Convert.ToUInt16(array[0][2..]);
				}
				else
				{
					operateResult.Content3 = Convert.ToUInt16(array[0][1..]);
				}
				if (operateResult.Content3 > 255)
				{
					operateResult.Message = "SiemensDBAddressNotAllowedLargerThan255";
					return operateResult;
				}
				operateResult.Content2 = CalculateAddressStarted(address[(address.IndexOf('.') + 1)..]);
			}
			else if (address[0] == 'T')
			{
				operateResult.Content1 = 7;
				operateResult.Content2 = CalculateAddressStarted(address[1..]);
			}
			else
			{
				if (address[0] != 'C')
				{
					operateResult.Message = "NotSupportedDataType";
					operateResult.Content1 = 0;
					operateResult.Content2 = 0;
					operateResult.Content3 = 0;
					return operateResult;
				}
				operateResult.Content1 = 6;
				operateResult.Content2 = CalculateAddressStarted(address[1..]);
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
	/// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100 -&gt; Starting address, formatted as M100,I100,Q100,DB1.100</param>
	/// <param name="count">读取数据个数 -&gt; Number of Read data</param>
	/// <returns>带结果对象的报文数据 -&gt; Message data with a result object</returns>
	public static OperateResult<byte[]> BuildReadCommand(string address, ushort count)
	{
		OperateResult<byte, int, ushort> operateResult = AnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		byte[] array =
        [
            83,
			53,
			16,
			1,
			3,
			5,
			3,
			8,
			operateResult.Content1,
			(byte)operateResult.Content3,
			(byte)(operateResult.Content2 / 256),
			(byte)(operateResult.Content2 % 256),
			0,
			0,
			0,
			0
		];
		if (operateResult.Content1 == 1 || operateResult.Content1 == 6 || operateResult.Content1 == 7)
		{
			if (count % 2 != 0)
			{
				return new OperateResult<byte[]>("SiemensReadLengthMustBeEvenNumber");
			}
			array[12] = BitConverter.GetBytes((int)count / 2)[1];
			array[13] = BitConverter.GetBytes((int)count / 2)[0];
		}
		else
		{
			array[12] = BitConverter.GetBytes(count)[1];
			array[13] = BitConverter.GetBytes(count)[0];
		}
		array[14] = byte.MaxValue;
		array[15] = 2;
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 生成一个写入字节数据的指令<br />
	/// Generate an instruction to write byte data
	/// </summary>
	/// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100 -&gt; Starting address, formatted as M100,I100,Q100,DB1.100</param>
	/// <param name="data">实际的写入的内容 -&gt; The actual content of the write</param>
	/// <returns>带结果对象的报文数据 -&gt; Message data with a result object</returns>
	public static OperateResult<byte[]> BuildWriteCommand(string address, byte[] data)
	{
		data ??= [];
		OperateResult<byte, int, ushort> operateResult = AnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}
		byte[] array = new byte[16 + data.Length];
		array[0] = 83;
		array[1] = 53;
		array[2] = 16;
		array[3] = 1;
		array[4] = 3;
		array[5] = 3;
		array[6] = 3;
		array[7] = 8;
		array[8] = operateResult.Content1;
		array[9] = (byte)operateResult.Content3;
		array[10] = (byte)(operateResult.Content2 / 256);
		array[11] = (byte)(operateResult.Content2 % 256);
		if (operateResult.Content1 == 1 || operateResult.Content1 == 6 || operateResult.Content1 == 7)
		{
			if (data.Length % 2 != 0)
			{
				return new OperateResult<byte[]>("SiemensReadLengthMustBeEvenNumber");
			}
			array[12] = BitConverter.GetBytes(data.Length / 2)[1];
			array[13] = BitConverter.GetBytes(data.Length / 2)[0];
		}
		else
		{
			array[12] = BitConverter.GetBytes(data.Length)[1];
			array[13] = BitConverter.GetBytes(data.Length)[0];
		}
		array[14] = byte.MaxValue;
		array[15] = 2;
		Array.Copy(data, 0, array, 16, data.Length);
		return OperateResult.Ok(array);
	}
}
