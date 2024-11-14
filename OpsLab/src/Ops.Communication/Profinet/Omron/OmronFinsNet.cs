using System.Net.Sockets;
using System.Text;
using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;

namespace Ops.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙PLC通讯类，采用Fins-Tcp通信协议实现，支持的地址信息参见api文档信息。
/// </summary>
/// <remarks>
/// <note type="important">PLC的IP地址的要求，最后一个整数的范围应该小于250，否则会发生连接不上的情况。</note>
/// <br />
/// <note type="warning">如果在测试的时候报错误码64，经网友 上海-Lex 指点，是因为PLC中产生了报警，如伺服报警，模块错误等产生的，但是数据还是能正常读到的，屏蔽64报警或清除plc错误可解决</note>
/// <br />
/// <note type="warning">如果碰到NX系列连接失败，或是无法读取的，需要使用网口2，配置ip地址，网线连接网口2，配置FINSTCP，把UDP的端口改成9601的，这样就可以读写了。</note><br />
/// 需要特别注意<see cref="ReadSplits" />属性，在超长数据读取时，规定了切割读取的长度，在不是CP1H及扩展模块的时候，可以设置为999，提高一倍的通信速度。
/// </remarks>
/// <example>
/// 地址列表：
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
///     <term>DM Area</term>
///     <term>D</term>
///     <term>D100,D200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>CIO Area</term>
///     <term>C</term>
///     <term>C100,C200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>Work Area</term>
///     <term>W</term>
///     <term>W100,W200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>Holding Bit Area</term>
///     <term>H</term>
///     <term>H100,H200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>Auxiliary Bit Area</term>
///     <term>A</term>
///     <term>A100,A200</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
///   <item>
///     <term>EM Area</term>
///     <term>E</term>
///     <term>E0.0,EF.200,E10.100</term>
///     <term>10</term>
///     <term>√</term>
///     <term>√</term>
///     <term></term>
///   </item>
/// </list>
/// </example>
public sealed class OmronFinsNet : NetworkDeviceBase
{
	private readonly byte[] _handSingle =
    [
        70, 73, 78, 83, 0, 0, 0, 12, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0
	];

	/// <summary>
	/// 信息控制字段，默认0x80。
	/// </summary>
	public byte ICF { get; set; } = 128;

	/// <summary>
	/// 系统使用的内部信息。
	/// </summary>
	public byte RSV { get; private set; } = 0;

	/// <summary>
	/// 网络层信息，默认0x02，如果有八层消息，就设置为0x07。
	/// </summary>
	public byte GCT { get; set; } = 2;

	/// <summary>
	/// PLC的网络号地址，默认0x00。
	/// </summary>
	public byte DNA { get; set; } = 0;

	/// <summary>
	/// PLC的节点地址，这个值在配置了ip地址之后是默认赋值的，默认为Ip地址的最后一位。
	/// </summary>
	/// <remarks>
	/// <note type="important">假如你的PLC的Ip地址为192.168.0.10，那么这个值就是10</note>
	/// </remarks>
	public byte DA1 { get; set; } = 19;

	/// <summary>
	/// PLC的单元号地址，通常都为0。
	/// </summary>
	/// <remarks>
	/// <note type="important">通常都为0</note>
	/// </remarks>
	public byte DA2 { get; set; } = 0;

	/// <summary>
	/// 上位机的网络号地址。
	/// </summary>
	public byte SNA { get; set; } = 0;

	/// <summary>
	/// 上位机的节点地址，默认是0x01，当连接PLC之后，将由PLC来设定当前的值。
	/// </summary>
	/// <remarks>
	/// <note type="important">v9.6.5版本及之前的版本都需要手动设置，如果是多连接，相同的节点是连接不上PLC的。</note>
	/// </remarks>
	public byte SA1 { get; set; } = 1;

	/// <summary>
	/// 上位机的单元号地址<br />
	/// Unit number and address of the computer
	/// </summary>
	public byte SA2 { get; set; }

	/// <summary>
	/// 设备的标识号。
	/// </summary>
	public byte SID { get; set; } = 0;

	/// <summary>
	/// 进行字读取的时候对于超长的情况按照本属性进行切割，默认500，如果不是CP1H及扩展模块的，可以设置为999，可以提高一倍的通信速度。
	/// </summary>
	public int ReadSplits { get; set; } = 500;

	/// <summary>
	/// 实例化一个欧姆龙PLC Fins帧协议的通讯对象。
	/// </summary>
	public OmronFinsNet()
	{
		WordLength = 1;
		ByteTransform = new ReverseWordTransform
		{
			DataFormat = DataFormat.CDAB,
			IsStringReverseByteWord = true,
		};
	}

	/// <summary>
	/// 指定ip地址和端口号来实例化一个欧姆龙PLC Fins帧协议的通讯对象。
	/// </summary>
	/// <param name="ipAddress">PLCd的Ip地址</param>
	/// <param name="port">PLC的端口, 默认 9600</param>
	public OmronFinsNet(string ipAddress, int port = 9600)
		: this()
	{
		IpAddress = ipAddress;
		Port = port;
	}

	protected override INetMessage GetNewNetMessage()
	{
		return new FinsMessage();
	}

	/// <summary>
	/// 将普通的指令打包成完整的指令
	/// </summary>
	/// <param name="cmd">FINS的核心指令</param>
	/// <returns>完整的可用于发送PLC的命令</returns>
	private byte[] PackCommand(byte[] cmd)
	{
		byte[] array = new byte[26 + cmd.Length];
		Array.Copy(_handSingle, 0, array, 0, 4);
		byte[] bytes = BitConverter.GetBytes(array.Length - 8);
		Array.Reverse((Array)bytes);
		bytes.CopyTo(array, 4);
		array[11] = 2;
		array[16] = ICF;
		array[17] = RSV;
		array[18] = GCT;
		array[19] = DNA;
		array[20] = DA1;
		array[21] = DA2;
		array[22] = SNA;
		array[23] = SA1;
		array[24] = SA2;
		array[25] = SID;
		cmd.CopyTo(array, 26);
		return array;
	}

	protected override OperateResult InitializationOnConnect(Socket socket)
	{
		OperateResult<byte[]> operateResult = ReadFromCoreServer(socket, _handSingle, hasResponseData: true, usePackAndUnpack: false);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		int num = BitConverter.ToInt32(
        [
            operateResult.Content[15],
			operateResult.Content[14],
			operateResult.Content[13],
			operateResult.Content[12]
		], 0);

		if (num != 0)
		{
			var errCode = OmronFinsNetHelper.GetErrorCode(num);
            return new OperateResult((int)errCode, errCode.Desc());
		}

		if (operateResult.Content.Length >= 20)
		{
			SA1 = operateResult.Content[19];
		}

		if (operateResult.Content.Length >= 24)
		{
			DA1 = operateResult.Content[23];
		}
		return OperateResult.Ok();
	}

	protected override async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
	{
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(socket, _handSingle, hasResponseData: true, usePackAndUnpack: false).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		int status = BitConverter.ToInt32(
        [
            read.Content[15],
			read.Content[14],
			read.Content[13],
			read.Content[12]
		], 0);

		if (status != 0)
		{
			var errCode = OmronFinsNetHelper.GetErrorCode(status);
            return new OperateResult((int)errCode, errCode.Desc());
		}

		if (read.Content.Length >= 20)
		{
			SA1 = read.Content[19];
		}
		if (read.Content.Length >= 24)
		{
			DA1 = read.Content[23];
		}
		return OperateResult.Ok();
	}

	protected override byte[] PackCommandWithHeader(byte[] command)
	{
		return PackCommand(command);
	}

	protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
	{
		return OmronFinsNetHelper.ResponseValidAnalysis(response);
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		return OmronFinsNetHelper.Read(this, address, length, ReadSplits);
	}

    public OperateResult<byte[]> Read(string[] address)
    {
        return OmronFinsNetHelper.Read(this, address);
    }

    public override OperateResult Write(string address, byte[] value)
	{
		return OmronFinsNetHelper.Write(this, address, value);
	}

	public override OperateResult<string> ReadString(string address, ushort length)
	{
		return ReadString(address, length, Encoding.UTF8);
	}

	public override OperateResult Write(string address, string value)
	{
		return Write(address, value, Encoding.UTF8);
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		return await OmronFinsNetHelper.ReadAsync(this, address, length, ReadSplits).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		return await OmronFinsNetHelper.WriteAsync(this, address, value).ConfigureAwait(false);
	}

	public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
	{
		return await ReadStringAsync(address, length, Encoding.UTF8).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, string value)
	{
		return await WriteAsync(address, value, Encoding.UTF8).ConfigureAwait(false);
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		return OmronFinsNetHelper.ReadBool(this, address, length, ReadSplits);
	}

	public override OperateResult Write(string address, bool[] values)
	{
		return OmronFinsNetHelper.Write(this, address, values);
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		return await OmronFinsNetHelper.ReadBoolAsync(this, address, length, ReadSplits).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool[] values)
	{
		return await OmronFinsNetHelper.WriteAsync(this, address, values).ConfigureAwait(false);
	}

	public OperateResult Run()
	{
		return OmronFinsNetHelper.Run(this);
	}

	public OperateResult Stop()
	{
		return OmronFinsNetHelper.Stop(this);
	}

	public OperateResult<OmronCpuUnitData> ReadCpuUnitData()
	{
		return OmronFinsNetHelper.ReadCpuUnitData(this);
	}

	public OperateResult<OmronCpuUnitStatus> ReadCpuUnitStatus()
	{
		return OmronFinsNetHelper.ReadCpuUnitStatus(this);
	}

	public async Task<OperateResult> RunAsync()
	{
		return await OmronFinsNetHelper.RunAsync(this).ConfigureAwait(false);
	}

	public async Task<OperateResult> StopAsync()
	{
		return await OmronFinsNetHelper.StopAsync(this).ConfigureAwait(false);
	}

	public async Task<OperateResult<OmronCpuUnitData>> ReadCpuUnitDataAsync()
	{
		return await OmronFinsNetHelper.ReadCpuUnitDataAsync(this).ConfigureAwait(false);
	}

	public async Task<OperateResult<OmronCpuUnitStatus>> ReadCpuUnitStatusAsync()
	{
		return await OmronFinsNetHelper.ReadCpuUnitStatusAsync(this).ConfigureAwait(false);
	}

	public override string ToString()
	{
		return $"OmronFinsNet[{IpAddress}:{Port}]";
	}
}
