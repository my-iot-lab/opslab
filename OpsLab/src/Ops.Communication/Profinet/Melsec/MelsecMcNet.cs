using System.Text;
using System.Text.RegularExpressions;
using Ops.Communication.Address;
using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;
using Ops.Communication.Extensions;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯类，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为二进制通讯。
/// </summary>
/// <remarks>
/// 支持读写的数据类型详细参考API文档，支持高级的数据读取，例如读取智能模块，缓冲存储器等等。
/// </remarks>
/// <list type="number">
/// 目前组件测试通过的PLC型号列表，有些来自于网友的测试
/// <item>Q06UDV PLC  感谢hwdq0012</item>
/// <item>fx5u PLC  感谢山楂</item>
/// <item>Q02CPU PLC </item>
/// <item>L02CPU PLC </item>
/// </list>
/// 地址的输入的格式支持多种复杂的地址表示方式：
/// <list type="number">
/// <item>[商业授权] 扩展的数据地址: 表示为 ext=1;W100  访问扩展区域为1的W100的地址信息</item>
/// <item>[商业授权] 缓冲存储器地址: 表示为 mem=32  访问地址为32的本站缓冲存储器地址</item>
/// <item>[商业授权] 智能模块地址：表示为 module=3;4106  访问模块号3，偏移地址是4106的数据，偏移地址需要根据模块的详细信息来确认。</item>
/// <item>[商业授权] 基于标签的地址: 表示位 s=AAA  假如标签的名称为AAA，但是标签的读取是有条件的，详细参照<see cref="ReadTags(string,ushort)" /></item>
/// <item>普通的数据地址，参照下面的信息</item>
/// </list>
/// <example><list type="table">
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
///     <term>内部继电器</term>
///     <term>M</term>
///     <term>M100,M200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>输入继电器</term>
///     <term>X</term>
///     <term>X100,X1A0</term>
///     <term>16</term>
///     <term>√</term>
///     <term>√</term>
///     <term>8进制用0开头，X017</term>
///   </item>
///   <item>
///     <term>输出继电器</term>
///     <term>Y</term>
///     <term>Y100,Y1A0</term>
///     <term>16</term>
///     <term>√</term>
///     <term>√</term>
///     <term>8进制用0开头，Y017</term>
///   </item>
///    <item>
///     <term>锁存继电器</term>
///     <term>L</term>
///     <term>L100,L200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>报警器</term>
///     <term>F</term>
///     <term>F100,F200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>边沿继电器</term>
///     <term>V</term>
///     <term>V100,V200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>链接继电器</term>
///     <term>B</term>
///     <term>B100,B1A0</term>
///     <term>16</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>步进继电器</term>
///     <term>S</term>
///     <term>S100,S200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>数据寄存器</term>
///     <term>D</term>
///     <term>D1000,D2000</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>链接寄存器</term>
///     <term>W</term>
///     <term>W100,W1A0</term>
///     <term>16</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>文件寄存器</term>
///     <term>R</term>
///     <term>R100,R200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>ZR文件寄存器</term>
///     <term>ZR</term>
///     <term>ZR100,ZR2A0</term>
///     <term>16</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>变址寄存器</term>
///     <term>Z</term>
///     <term>Z100,Z200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>定时器的触点</term>
///     <term>TS</term>
///     <term>TS100,TS200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>定时器的线圈</term>
///     <term>TC</term>
///     <term>TC100,TC200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>定时器的当前值</term>
///     <term>TN</term>
///     <term>TN100,TN200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>累计定时器的触点</term>
///     <term>SS</term>
///     <term>SS100,SS200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>累计定时器的线圈</term>
///     <term>SC</term>
///     <term>SC100,SC200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>累计定时器的当前值</term>
///     <term>SN</term>
///     <term>SN100,SN200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
///   <item>
///     <term>计数器的触点</term>
///     <term>CS</term>
///     <term>CS100,CS200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>计数器的线圈</term>
///     <term>CC</term>
///     <term>CC100,CC200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>计数器的当前值</term>
///     <term>CN</term>
///     <term>CN100,CN200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>×</term>
///     <term></term>
///   </item>
/// </list>
/// </example>
public class MelsecMcNet : NetworkDeviceBase
{
	/// <summary>
	/// 网络号，通常为0。
	/// </summary>
	/// <remarks>
	/// 依据PLC的配置而配置，如果PLC配置了1，那么此处也填0，如果PLC配置了2，此处就填2，测试不通的话，继续测试0
	/// </remarks>
	public byte NetworkNumber { get; set; } = 0;


	/// <summary>
	/// 网络站号，通常为0。
	/// </summary>
	/// <remarks>
	/// 依据PLC的配置而配置，如果PLC配置了1，那么此处也填0，如果PLC配置了2，此处就填2，测试不通的话，继续测试0
	/// </remarks>
	public byte NetworkStationNumber { get; set; } = 0;


	/// <summary>
	/// 实例化三菱的Qna兼容3E帧协议的通讯对象。
	/// </summary>
	public MelsecMcNet()
	{
		base.WordLength = 1;
		base.ByteTransform = new RegularByteTransform();
	}

	/// <summary>
	/// 指定ip地址和端口号来实例化一个默认的对象。
	/// </summary>
	/// <param name="ipAddress">PLC的Ip地址</param>
	/// <param name="port">PLC的端口</param>
	public MelsecMcNet(string ipAddress, int port)
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

	/// <summary>
	/// 当前MC协议的分析地址的方法，对传入的字符串格式的地址进行数据解析。
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="length">数据长度</param>
	/// <returns>解析后的数据信息</returns>
	protected virtual OperateResult<McAddressData> McAnalysisAddress(string address, ushort length)
	{
		return McAddressData.ParseMelsecFrom(address, length);
	}

	/// <summary>
	/// 检查MC协议返回的数据报文是否合法，如果有错误代码返回，同时返回当前错误代码的具体文本信息。
	/// </summary>
	/// <param name="response">MC协议返回的原始报文信息</param>
	/// <returns>是否成功的结果对象</returns>
	protected virtual OperateResult CheckResponseContent(byte[] response)
	{
		return CheckResponseContentHelper(response);
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		if (address.StartsWith("s=") || address.StartsWith("S="))
		{
			return ReadTags(address[2..], length);
		}

		if (Regex.IsMatch(address, "ext=[0-9]+;", RegexOptions.IgnoreCase))
		{
			string value = Regex.Match(address, "ext=[0-9]+;").Value;
			ushort extend = ushort.Parse(Regex.Match(value, "[0-9]+").Value);
			return ReadExtend(extend, address[value.Length..], length);
		}

		if (Regex.IsMatch(address, "mem=", RegexOptions.IgnoreCase))
		{
			return ReadMemory(address[4..], length);
		}

		if (Regex.IsMatch(address, "module=[0-9]+;", RegexOptions.IgnoreCase))
		{
			string value2 = Regex.Match(address, "module=[0-9]+;").Value;
			ushort module = ushort.Parse(Regex.Match(value2, "[0-9]+").Value);
			return ReadSmartModule(module, address[value2.Length..], (ushort)(length * 2));
		}

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
		return ExtractActualData(operateResult.Content.RemoveBegin(11), isBit: false);
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
		OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult operateResult2 = CheckResponseContent(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		if (address.StartsWith("s="))
		{
			return await ReadTagsAsync(address[2..], length);
		}

		if (Regex.IsMatch(address, "ext=[0-9]+;"))
		{
			string extStr = Regex.Match(address, "ext=[0-9]+;").Value;
			ushort ext = ushort.Parse(Regex.Match(extStr, "[0-9]+").Value);
			return await ReadExtendAsync(ext, address[extStr.Length..], length);
		}

		if (Regex.IsMatch(address, "mem=", RegexOptions.IgnoreCase))
		{
			return await ReadMemoryAsync(address[4..], length);
		}

		OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
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
			OperateResult<byte[]> read = await ReadAddressDataAsync(addressResult.Content);
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
		byte[] coreResult = MelsecHelper.BuildReadMcCoreCommand(addressData, isBit: false);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content.RemoveBegin(11), isBit: false);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(addressResult);
		}
		return await WriteAddressDataAsync(addressResult.Content, value);
	}

	private async Task<OperateResult> WriteAddressDataAsync(McAddressData addressData, byte[] value)
	{
		byte[] coreResult = MelsecHelper.BuildWriteWordCoreCommand(addressData, value);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，需要自行解析数据。
	/// </summary>
	/// <param name="address">所有的地址的集合</param>
	/// <remarks>
	/// 访问安装有 Q 系列 C24/E71 的站 QCPU 上位站 经由 Q 系列兼容网络系统 MELSECNET/H MELSECNET/10 Ethernet 的 QCPU 其他站 时
	/// 访问点数········1≦ 字访问点数 双字访问点数 ≦192
	/// <br />
	/// 访问 QnACPU 其他站 经由 QnA 系列兼容网络系统 MELSECNET/10 Ethernet 的 Q/QnACPU 其他站 时访问点数········1≦ 字访问点数 双字访问点数 ≦96
	/// <br />
	/// 访问上述以外的 PLC CPU 其他站 时访问点数········1≦字访问点数≦10
	/// </remarks>
	/// <returns>结果</returns>
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
		return ExtractActualData(operateResult2.Content.RemoveBegin(11), isBit: false);
	}

	/// <summary>
	/// 随机读取PLC的数据信息，可以跨地址，跨类型组合，每个地址是任意的长度。收到结果后，需要自行解析数据，目前只支持字地址，比如D区，W区，R区，不支持X，Y，M，B，L等等。
	/// </summary>
	/// <param name="address">所有的地址的集合</param>
	/// <param name="length">每个地址的长度信息</param>
	/// <remarks>
	/// 实际测试不一定所有的plc都可以读取成功，具体情况需要具体分析
	/// <br />
	/// 1 块数按照下列要求指定 120 ≧ 字软元件块数 + 位软元件块数
	/// <br />
	/// 2 各软元件点数按照下列要求指定 960 ≧ 字软元件各块的合计点数 + 位软元件各块的合计点数
	/// </remarks>
	/// <returns>结果</returns>
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

		byte[] mcCore = MelsecHelper.BuildReadRandomCommand(array);
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
		return ExtractActualData(operateResult2.Content.RemoveBegin(11), isBit: false);
	}

	/// <summary>
	/// 随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，自动转换为了short类型的数组。
	/// </summary>
	/// <param name="address">所有的地址的集合</param>
	/// <remarks>
	/// 访问安装有 Q 系列 C24/E71 的站 QCPU 上位站 经由 Q 系列兼容网络系统 MELSECNET/H MELSECNET/10 Ethernet 的 QCPU 其他站 时
	/// 访问点数········1≦ 字访问点数 双字访问点数 ≦192
	///
	/// 访问 QnACPU 其他站 经由 QnA 系列兼容网络系统 MELSECNET/10 Ethernet 的 Q/QnACPU 其他站 时访问点数········1≦ 字访问点数 双字访问点数 ≦96
	///
	/// 访问上述以外的 PLC CPU 其他站 时访问点数········1≦字访问点数≦10
	/// </remarks>
	/// <returns>结果</returns>
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

		byte[] coreResult = MelsecHelper.BuildReadRandomWordCommand(mcAddressDatas);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content.RemoveBegin(11), isBit: false);
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

		byte[] coreResult = MelsecHelper.BuildReadRandomCommand(mcAddressDatas);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content.RemoveBegin(11), isBit: false);
	}

	public async Task<OperateResult<short[]>> ReadRandomInt16Async(string[] address)
	{
		OperateResult<byte[]> read = await ReadRandomAsync(address);
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

		byte[] mcCore = MelsecHelper.BuildReadMcCoreCommand(operateResult.Content, isBit: true);
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

		OperateResult<byte[]> operateResult4 = ExtractActualData(operateResult2.Content.RemoveBegin(11), isBit: true);
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

		byte[] coreResult = MelsecHelper.BuildReadMcCoreCommand(addressResult.Content, isBit: true);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
		if (!read.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<bool[]>(check);
		}

		OperateResult<byte[]> extract = ExtractActualData(read.Content.RemoveBegin(11), isBit: true);
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

		byte[] coreResult = MelsecHelper.BuildWriteBitCoreCommand(addressResult.Content, values);
		var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
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

	/// <summary>
	/// <b>[商业授权]</b> 读取PLC的标签信息，需要传入标签的名称，读取的字长度，标签举例：A; label[1]; bbb[10,10,10]。
	/// </summary>
	/// <param name="tag">标签名</param>
	/// <param name="length">读取长度</param>
	/// <returns>是否成功</returns>
	/// <remarks>
	///  不可以访问局部标签。<br />
	///  不可以访问通过GX Works2设置的全局标签。<br />
	///  为了访问全局标签，需要通过GX Works3的全局标签设置编辑器将“来自于外部设备的访问”的设置项目置为有效。(默认为无效。)<br />
	///  以ASCII代码进行数据通信时，由于需要从UTF-16将标签名转换为ASCII代码，因此报文容量将增加
	/// </remarks>
	public OperateResult<byte[]> ReadTags(string tag, ushort length)
	{
		return ReadTags(new string[1] { tag }, new ushort[1] { length });
	}

	public OperateResult<byte[]> ReadTags(string[] tags, ushort[] length)
	{
		byte[] mcCore = MelsecHelper.BuildReadTag(tags, length);
		var operateResult = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		OperateResult operateResult2 = CheckResponseContent(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		var operateResult3 = ExtractActualData(operateResult.Content.RemoveBegin(11), isBit: false);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return MelsecHelper.ExtraTagData(operateResult3.Content);
	}

	public async Task<OperateResult<byte[]>> ReadTagsAsync(string tag, ushort length)
	{
		return await ReadTagsAsync(new string[1] { tag }, new ushort[1] { length });
	}

	public async Task<OperateResult<byte[]>> ReadTagsAsync(string[] tags, ushort[] length)
	{
		byte[] coreResult = MelsecHelper.BuildReadTag(tags, length);
		var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}

		OperateResult<byte[]> extract = ExtractActualData(read.Content.RemoveBegin(11), isBit: false);
		if (!extract.IsSuccess)
		{
			return extract;
		}
		return MelsecHelper.ExtraTagData(extract.Content);
	}

	/// <summary>
	/// <b>[商业授权]</b> 读取扩展的数据信息，需要在原有的地址，长度信息之外，输入扩展值信息。
	/// </summary>
	/// <param name="extend">扩展信息</param>
	/// <param name="address">地址</param>
	/// <param name="length">数据长度</param>
	/// <returns>返回结果</returns>
	public OperateResult<byte[]> ReadExtend(ushort extend, string address, ushort length)
	{
		OperateResult<McAddressData> operateResult = McAnalysisAddress(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		byte[] mcCore = MelsecHelper.BuildReadMcCoreExtendCommand(operateResult.Content, extend, isBit: false);
		var operateResult2 = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}

		var operateResult4 = ExtractActualData(operateResult2.Content.RemoveBegin(11), isBit: false);
		if (!operateResult4.IsSuccess)
		{
			return operateResult4;
		}
		return MelsecHelper.ExtraTagData(operateResult4.Content);
	}

	public async Task<OperateResult<byte[]>> ReadExtendAsync(ushort extend, string address, ushort length)
	{
		OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(addressResult);
		}

		byte[] coreResult = MelsecHelper.BuildReadMcCoreExtendCommand(addressResult.Content, extend, isBit: false);
		var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}

		OperateResult<byte[]> extract = ExtractActualData(read.Content.RemoveBegin(11), isBit: false);
		if (!extract.IsSuccess)
		{
			return extract;
		}
		return MelsecHelper.ExtraTagData(extract.Content);
	}

	/// <summary>
	/// <b>[商业授权]</b> 读取缓冲寄存器的数据信息，地址直接为偏移地址。
	/// </summary>
	/// <remarks>
	/// 本指令不可以访问下述缓冲存储器:<br />
	/// 1. 本站(SLMP对应设备)上安装的智能功能模块<br />
	/// 2. 其它站缓冲存储器<br />
	/// </remarks>
	/// <param name="address">偏移地址</param>
	/// <param name="length">读取长度</param>
	/// <returns>读取的内容</returns>
	public OperateResult<byte[]> ReadMemory(string address, ushort length)
	{
		var operateResult = MelsecHelper.BuildReadMemoryCommand(address, length);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReadFromCoreServer(PackMcCommand(operateResult.Content, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return ExtractActualData(operateResult2.Content.RemoveBegin(11), isBit: false);
	}

	public async Task<OperateResult<byte[]>> ReadMemoryAsync(string address, ushort length)
	{
		var coreResult = MelsecHelper.BuildReadMemoryCommand(address, length);
		if (!coreResult.IsSuccess)
		{
			return coreResult;
		}

		var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult.Content, NetworkNumber, NetworkStationNumber));
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content.RemoveBegin(11), isBit: false);
	}

	/// <summary>
	/// <b>[商业授权]</b> 读取智能模块的数据信息，需要指定模块地址，偏移地址，读取的字节长度。
	/// </summary>
	/// <param name="module">模块地址</param>
	/// <param name="address">地址</param>
	/// <param name="length">数据长度</param>
	/// <returns>返回结果</returns>
	public OperateResult<byte[]> ReadSmartModule(ushort module, string address, ushort length)
	{
		var operateResult = MelsecHelper.BuildReadSmartModule(module, address, length);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReadFromCoreServer(PackMcCommand(operateResult.Content, NetworkNumber, NetworkStationNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult operateResult3 = CheckResponseContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return ExtractActualData(operateResult2.Content.RemoveBegin(11), isBit: false);
	}

	public async Task<OperateResult<byte[]>> ReadSmartModuleAsync(ushort module, string address, ushort length)
	{
		var coreResult = MelsecHelper.BuildReadSmartModule(module, address, length);
		if (!coreResult.IsSuccess)
		{
			return coreResult;
		}

		var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult.Content, NetworkNumber, NetworkStationNumber));
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<byte[]>(check);
		}
		return ExtractActualData(read.Content.RemoveBegin(11), isBit: false);
	}

	/// <summary>
	/// 远程Run操作。
	/// </summary>
	/// <param name="force">是否强制执行</param>
	/// <returns>是否成功</returns>
	public OperateResult RemoteRun(bool force = false)
	{
		var operateResult = ReadFromCoreServer(PackMcCommand(new byte[8] { 1, 16, 0, 0, 1, 0, 0, 0 }, NetworkNumber, NetworkStationNumber));
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

	/// <summary>
	/// 远程Stop操作。
	/// </summary>
	/// <returns>是否成功</returns>
	public OperateResult RemoteStop()
	{
		var operateResult = ReadFromCoreServer(PackMcCommand(new byte[6] { 2, 16, 0, 0, 1, 0 }, NetworkNumber, NetworkStationNumber));
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

	/// <summary>
	/// 远程Reset操作。
	/// </summary>
	/// <returns>是否成功</returns>
	public OperateResult RemoteReset()
	{
		var operateResult = ReadFromCoreServer(PackMcCommand(new byte[6] { 6, 16, 0, 0, 1, 0 }, NetworkNumber, NetworkStationNumber));
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

	/// <summary>
	/// 读取PLC的型号信息，例如 Q02HCPU。
	/// </summary>
	/// <returns>返回型号的结果对象</returns>
	public OperateResult<string> ReadPlcType()
	{
		var operateResult = ReadFromCoreServer(PackMcCommand(new byte[4] { 1, 1, 0, 0 }, NetworkNumber, NetworkStationNumber));
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult);
		}

		OperateResult operateResult2 = CheckResponseContent(operateResult.Content);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult2);
		}
		return OperateResult.Ok(Encoding.ASCII.GetString(operateResult.Content, 11, 16).TrimEnd(Array.Empty<char>()));
	}

	/// <summary>
	/// LED 熄灭 出错代码初始化。
	/// </summary>
	/// <returns>是否成功</returns>
	public OperateResult ErrorStateReset()
	{
		var operateResult = ReadFromCoreServer(PackMcCommand(new byte[4] { 23, 22, 0, 0 }, NetworkNumber, NetworkStationNumber));
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
		var read = await ReadFromCoreServerAsync(PackMcCommand(new byte[8] { 1, 16, 0, 0, 1, 0, 0, 0 }, NetworkNumber, NetworkStationNumber));
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
		var read = await ReadFromCoreServerAsync(PackMcCommand(new byte[6] { 2, 16, 0, 0, 1, 0 }, NetworkNumber, NetworkStationNumber));
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
		var read = await ReadFromCoreServerAsync(PackMcCommand(new byte[6] { 6, 16, 0, 0, 1, 0 }, NetworkNumber, NetworkStationNumber));
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
		var read = await ReadFromCoreServerAsync(PackMcCommand(new byte[4] { 1, 1, 0, 0 }, NetworkNumber, NetworkStationNumber));
		if (!read.IsSuccess)
		{
			return OperateResult.Error<string>(read);
		}

		OperateResult check = CheckResponseContent(read.Content);
		if (!check.IsSuccess)
		{
			return OperateResult.Error<string>(check);
		}
		return OperateResult.Ok(Encoding.ASCII.GetString(read.Content, 11, 16).TrimEnd(Array.Empty<char>()));
	}

	public async Task<OperateResult> ErrorStateResetAsync()
	{
		var read = await ReadFromCoreServerAsync(PackMcCommand(new byte[4] { 23, 22, 0, 0 }, NetworkNumber, NetworkStationNumber));
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
		return $"MelsecMcNet[{IpAddress}:{Port}]";
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
		byte[] array = new byte[11 + mcCore.Length];
		array[0] = 80;
		array[1] = 0;
		array[2] = networkNumber;
		array[3] = byte.MaxValue;
		array[4] = byte.MaxValue;
		array[5] = 3;
		array[6] = networkStationNumber;
		array[7] = (byte)((array.Length - 9) % 256);
		array[8] = (byte)((array.Length - 9) / 256);
		array[9] = 10;
		array[10] = 0;
		mcCore.CopyTo(array, 11);
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
			byte[] array = new byte[response.Length * 2];
			for (int i = 0; i < response.Length; i++)
			{
				if ((response[i] & 0x10) == 16)
				{
					array[i * 2] = 1;
				}
				if ((response[i] & 1) == 1)
				{
					array[i * 2 + 1] = 1;
				}
			}
			return OperateResult.Ok(array);
		}
		return OperateResult.Ok(response);
	}

	/// <summary>
	/// 检查从MC返回的数据是否是合法的。
	/// </summary>
	/// <param name="content">数据内容</param>
	/// <returns>是否合法</returns>
	public static OperateResult CheckResponseContentHelper(byte[] content)
	{
		ushort num = BitConverter.ToUInt16(content, 9);
		if (num != 0)
		{
			return new OperateResult<byte[]>(num, MelsecHelper.GetErrorDescription(num));
		}
		return OperateResult.Ok();
	}
}
