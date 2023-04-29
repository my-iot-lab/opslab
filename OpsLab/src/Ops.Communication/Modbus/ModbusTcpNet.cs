using System.Net.Sockets;
using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;
using Ops.Communication.Utils;

namespace Ops.Communication.Modbus;

/// <summary>
/// Modbus-Tcp协议的客户端通讯类，方便的和服务器进行数据交互，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式。
/// </summary>
/// <remarks>
/// 本客户端支持的标准的modbus协议，Modbus-Tcp及Modbus-Udp内置的消息号会进行自增，地址支持富文本格式，具体参考示例代码。
/// 读取线圈，输入线圈，寄存器，输入寄存器的方法中的读取长度对商业授权用户不限制，内部自动切割读取，结果合并。
/// </remarks>
/// <example>
/// 本客户端支持的标准的modbus协议，Modbus-Tcp及Modbus-Udp内置的消息号会进行自增，比如我们想要控制消息号在0-1000之间自增，不能超过一千，可以写如下的代码：
/// <note type="important">
/// 地址共可以携带3个信息，最完整的表示方式"s=2;x=3;100"，对应的modbus报文是 02 03 00 64 00 01 的前四个字节，站号，功能码，起始地址，下面举例
/// </note>
/// 当读写int, uint, float, double, long, ulong类型的时候，支持动态指定数据格式，也就是 DataFormat 信息，本部分内容为商业授权用户专有，感谢支持。
/// ReadInt32("format=BADC;100") 指示使用BADC的格式来解析byte数组，从而获得int数据，同时支持和站号信息叠加，例如：ReadInt32("format=BADC;s=2;100")
/// <list type="definition">
/// <item>
///     <term>读取线圈</term>
///     <description>ReadCoil("100")表示读取线圈100的值，ReadCoil("s=2;100")表示读取站号为2，线圈地址为100的值</description>
/// </item>
/// <item>
///     <term>读取离散输入</term>
///     <description>ReadDiscrete("100")表示读取离散输入100的值，ReadDiscrete("s=2;100")表示读取站号为2，离散地址为100的值</description>
/// </item>
/// <item>
///     <term>读取寄存器</term>
///     <description>ReadInt16("100")表示读取寄存器100的值，ReadInt16("s=2;100")表示读取站号为2，寄存器100的值</description>
/// </item>
/// <item>
///     <term>读取输入寄存器</term>
///     <description>ReadInt16("x=4;100")表示读取输入寄存器100的值，ReadInt16("s=2;x=4;100")表示读取站号为2，输入寄存器100的值</description>
/// </item>
/// <item>
///     <term>读取寄存器的位</term>
///     <description>ReadBool("100.0")表示读取寄存器100第0位的值，ReadBool("s=2;100.0")表示读取站号为2，寄存器100第0位的值，支持读连续的多个位</description>
/// </item>
/// <item>
///     <term>读取输入寄存器的位</term>
///     <description>ReadBool("x=4;100.0")表示读取输入寄存器100第0位的值，ReadBool("s=2;x=4;100.0")表示读取站号为2，输入寄存器100第0位的值，支持读连续的多个位</description>
/// </item>
/// </list>
/// 对于写入来说也是一致的
/// <list type="definition">
/// <item>
///     <term>写入线圈</term>
///     <description>WriteCoil("100",true)表示读取线圈100的值，WriteCoil("s=2;100",true)表示读取站号为2，线圈地址为100的值</description>
/// </item>
/// <item>
///     <term>写入寄存器</term>
///     <description>Write("100",(short)123)表示写寄存器100的值123，Write("s=2;100",(short)123)表示写入站号为2，寄存器100的值123</description>
/// </item>
/// </list>
/// 特殊说明部分：
///  <list type="definition">
/// <item>
///     <term>01功能码</term>
///     <description>ReadBool("100")</description>
/// </item>
/// <item>
///     <term>02功能码</term>
///     <description>ReadBool("x=2;100")</description>
/// </item>
/// <item>
///     <term>03功能码</term>
///     <description>Read("100")</description>
/// </item>
/// <item>
///     <term>04功能码</term>
///     <description>Read("x=4;100")</description>
/// </item>
/// <item>
///     <term>05功能码</term>
///     <description>Write("100", True)</description>
/// </item>
/// <item>
///     <term>06功能码</term>
///     <description>Write("100", (short)100);Write("100", (ushort)100)</description>
/// </item>
/// <item>
///     <term>0F功能码</term>
///     <description>Write("100", new bool[]{True})   注意：这里和05功能码传递的参数类型不一样</description>
/// </item>
/// <item>
///     <term>10功能码</term>
///     <description>如果写一个short想用10功能码：Write("100", new short[]{100})</description>
/// </item>
/// <item>
///     <term>16功能码</term>
///     <description>Write("100.2", True) 当写入bool值的方法里，地址格式变为字地址时，就使用16功能码，通过掩码的方式来修改寄存器的某一位，
///     需要Modbus服务器支持，对于不支持该功能码的写入无效。</description>
/// </item>
/// </list>
/// </example>
public class ModbusTcpNet : NetworkDeviceBase, IModbus, IReadWriteDevice, IReadWriteNet
{
	private readonly IncrementCount softIncrementCount;

	private bool isAddressStartWithZero = true;

	/// <summary>
	/// 获取或设置起始的地址是否从0开始，默认为True。
	/// </summary>
	/// <remarks>
	/// <note type="warning">因为有些设备的起始地址是从1开始的，就要设置本属性为<c>False</c></note>
	/// </remarks>
	public bool AddressStartWithZero
	{
		get
		{
			return isAddressStartWithZero;
		}
		set
		{
			isAddressStartWithZero = value;
		}
	}

	/// <summary>
	/// 获取或者重新修改服务器的默认站号信息，当然，你可以再读写的时候动态指定，参见备注。
	/// </summary>
	/// <remarks>
	/// 当你调用 ReadCoil("100") 时，对应的站号就是本属性的值，当你调用 ReadCoil("s=2;100") 时，就忽略本属性的值，读写寄存器的时候同理
	/// </remarks>
	public byte Station { get; set; } = 1;

	public DataFormat DataFormat
	{
		get
		{
			return base.ByteTransform.DataFormat;
		}
		set
		{
			base.ByteTransform.DataFormat = value;
		}
	}

	/// <summary>
	/// 字符串数据是否按照字来反转，默认为False
	/// </summary>
	/// <remarks>
	/// 字符串按照2个字节的排列进行颠倒，根据实际情况进行设置
	/// </remarks>
	public bool IsStringReverse
	{
		get
		{
			return base.ByteTransform.IsStringReverseByteWord;
		}
		set
		{
			base.ByteTransform.IsStringReverseByteWord = value;
		}
	}

	/// <summary>
	/// 获取modbus协议自增的消息号，你可以自定义modbus的消息号的规则，详细参见<see cref="ModbusTcpNet" />说明，也可以查找<see cref="IncrementCount" />说明。
	/// </summary>
	public IncrementCount MessageId => softIncrementCount;

	/// <summary>
	/// 实例化一个Modbus-Tcp协议的客户端对象。
	/// </summary>
	public ModbusTcpNet()
	{
		softIncrementCount = new IncrementCount(65535L, 0L);
		base.WordLength = 1;
		Station = 1;
		base.ByteTransform = new ReverseWordTransform();
	}

	/// <summary>
	/// 指定服务器地址，端口号，客户端自己的站号来初始化
	/// </summary>
	/// <param name="ipAddress">服务器的Ip地址</param>
	/// <param name="port">服务器的端口号</param>
	/// <param name="station">客户端自身的站号</param>
	public ModbusTcpNet(string ipAddress, int port = 502, byte station = 1)
	{
		softIncrementCount = new IncrementCount(65535L, 0L);
		IpAddress = ipAddress;
		Port = port;
		base.WordLength = 1;
		this.Station = station;
		base.ByteTransform = new ReverseWordTransform();
	}

	protected override INetMessage GetNewNetMessage()
	{
		return new ModbusTcpMessage();
	}

	protected override OperateResult InitializationOnConnect(Socket socket)
	{
		return base.InitializationOnConnect(socket);
	}

	protected override async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
	{
		return await base.InitializationOnConnectAsync(socket).ConfigureAwait(false);
	}

	public override OperateResult<int[]> ReadInt32(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransInt32(m, 0, length));
	}

	public override OperateResult<uint[]> ReadUInt32(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransUInt32(m, 0, length));
	}

	public override OperateResult<float[]> ReadFloat(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransSingle(m, 0, length));
	}

	public override OperateResult<long[]> ReadInt64(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransInt64(m, 0, length));
	}

	public override OperateResult<ulong[]> ReadUInt64(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransUInt64(m, 0, length));
	}

	public override OperateResult<double[]> ReadDouble(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransDouble(m, 0, length));
	}

	public override OperateResult Write(string address, int[] values)
	{
		IByteTransform byteTransform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override OperateResult Write(string address, uint[] values)
	{
		IByteTransform byteTransform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override OperateResult Write(string address, float[] values)
	{
		IByteTransform byteTransform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override OperateResult Write(string address, long[] values)
	{
		IByteTransform byteTransform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override OperateResult Write(string address, ulong[] values)
	{
		IByteTransform byteTransform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override OperateResult Write(string address, double[] values)
	{
		IByteTransform byteTransform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return Write(address, byteTransform.TransByte(values));
	}

	public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 2)).ConfigureAwait(false), (byte[] m) => transform.TransInt32(m, 0, length));
	}

	public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 2)).ConfigureAwait(false), (byte[] m) => transform.TransUInt32(m, 0, length));
	}

	public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 2)).ConfigureAwait(false), (byte[] m) => transform.TransSingle(m, 0, length));
	}

	public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 4)).ConfigureAwait(false), (byte[] m) => transform.TransInt64(m, 0, length));
	}

	public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 4)).ConfigureAwait(false), (byte[] m) => transform.TransUInt64(m, 0, length));
	}

	public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
	{
		IByteTransform transform = OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform);
		return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 4)).ConfigureAwait(false), (byte[] m) => transform.TransDouble(m, 0, length));
	}

	public override async Task<OperateResult> WriteAsync(string address, int[] values)
	{
		return await WriteAsync(value: OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, uint[] values)
	{
		return await WriteAsync(value: OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, float[] values)
	{
		return await WriteAsync(value: OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, long[] values)
	{
		return await WriteAsync(value: OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, ulong[] values)
	{
		return await WriteAsync(value: OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, double[] values)
	{
		return await WriteAsync(value: OpsHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
	}

	public virtual OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
	{
		return OperateResult.Ok(address);
	}

	protected override byte[] PackCommandWithHeader(byte[] command)
	{
		return ModbusInfo.PackCommandToTcp(command, (ushort)softIncrementCount.GetCurrentValue());
	}

	protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
	{
		return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(response));
	}

	/// <summary>
	/// 读取线圈，需要指定起始地址，如果富文本地址不指定，默认使用的功能码是 0x01
	/// </summary>
	/// <param name="address">起始地址，格式为"1234"</param>
	/// <returns>带有成功标志的bool对象</returns>
	public OperateResult<bool> ReadCoil(string address)
	{
		return ReadBool(address);
	}

	/// <summary>
	/// 批量的读取线圈，需要指定起始地址，读取长度，如果富文本地址不指定，默认使用的功能码是 0x01
	/// </summary>
	/// <param name="address">起始地址，格式为"1234"</param>
	/// <param name="length">读取长度</param>
	/// <returns>带有成功标志的bool数组对象</returns>
	public OperateResult<bool[]> ReadCoil(string address, ushort length)
	{
		return ReadBool(address, length);
	}

	/// <summary>
	/// 读取输入线圈，需要指定起始地址，如果富文本地址不指定，默认使用的功能码是 0x02
	/// </summary>
	/// <param name="address">起始地址，格式为"1234"</param>
	/// <returns>带有成功标志的bool对象</returns>
	public OperateResult<bool> ReadDiscrete(string address)
	{
		return ByteTransformHelper.GetResultFromArray(ReadDiscrete(address, 1));
	}

	/// <summary>
	/// 批量的读取输入点，需要指定起始地址，读取长度，如果富文本地址不指定，默认使用的功能码是 0x02
	/// </summary>
	/// <param name="address">起始地址，格式为"1234"</param>
	/// <param name="length">读取长度</param>
	/// <returns>带有成功标志的bool数组对象</returns>
	public OperateResult<bool[]> ReadDiscrete(string address, ushort length)
	{
		return ModbusHelper.ReadBoolHelper(this, address, length, 2);
	}

	/// <summary>
	/// 从Modbus服务器批量读取寄存器的信息，需要指定起始地址，读取长度，如果富文本地址不指定，默认使用的功能码是 0x03，如果需要使用04功能码，那么地址就写成 x=4;100
	/// </summary>
	/// <param name="address">起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
	/// <param name="length">读取的数量</param>
	/// <returns>带有成功标志的字节信息</returns>
	/// <remarks>
	/// 富地址格式，支持携带站号信息，功能码信息，具体参照类的示例代码
	/// </remarks>
	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		return ModbusHelper.Read(this, address, length);
	}

	/// <summary>
	/// 将数据写入到Modbus的寄存器上去，需要指定起始地址和数据内容，如果富文本地址不指定，默认使用的功能码是 0x10
	/// </summary>
	/// <param name="address">起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
	/// <param name="value">写入的数据，长度根据data的长度来指示</param>
	/// <returns>返回写入结果</returns>
	/// <remarks>
	/// 富地址格式，支持携带站号信息，功能码信息，具体参照类的示例代码
	/// </remarks>
	public override OperateResult Write(string address, byte[] value)
	{
		return ModbusHelper.Write(this, address, value);
	}

	/// <summary>
	/// 将数据写入到Modbus的单个寄存器上去，需要指定起始地址和数据值，如果富文本地址不指定，默认使用的功能码是 0x06
	/// </summary>
	/// <param name="address">起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
	/// <param name="value">写入的short数据</param>
	/// <returns>是否写入成功</returns>
	public override OperateResult Write(string address, short value)
	{
		return ModbusHelper.Write(this, address, value);
	}

	/// <summary>
	/// 将数据写入到Modbus的单个寄存器上去，需要指定起始地址和数据值，如果富文本地址不指定，默认使用的功能码是 0x06
	/// </summary>
	/// <param name="address">起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
	/// <param name="value">写入的ushort数据</param>
	/// <returns>是否写入成功</returns>
	public override OperateResult Write(string address, ushort value)
	{
		return ModbusHelper.Write(this, address, value);
	}

	/// <summary>
	/// 向设备写入掩码数据，使用0x16功能码，需要确认对方是否支持相关的操作，掩码数据的操作主要针对寄存器。
	/// </summary>
	/// <param name="address">起始地址，起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
	/// <param name="andMask">等待与操作的掩码数据</param>
	/// <param name="orMask">等待或操作的掩码数据</param>
	/// <returns>是否写入成功</returns>
	public OperateResult WriteMask(string address, ushort andMask, ushort orMask)
	{
		return ModbusHelper.WriteMask(this, address, andMask, orMask);
	}

	public OperateResult WriteOneRegister(string address, short value)
	{
		return Write(address, value);
	}

	public OperateResult WriteOneRegister(string address, ushort value)
	{
		return Write(address, value);
	}

	public async Task<OperateResult<bool>> ReadCoilAsync(string address)
	{
		return await ReadBoolAsync(address).ConfigureAwait(false);
	}

	public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length)
	{
		return await ReadBoolAsync(address, length).ConfigureAwait(false);
	}

	public async Task<OperateResult<bool>> ReadDiscreteAsync(string address)
	{
		return ByteTransformHelper.GetResultFromArray(await ReadDiscreteAsync(address, 1).ConfigureAwait(false));
	}

	public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length)
	{
		return await ReadBoolHelperAsync(address, length, 2).ConfigureAwait(false);
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		return await ModbusHelper.ReadAsync(this, address, length).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, short value)
	{
		return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, ushort value)
	{
		return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
	}

	public async Task<OperateResult> WriteMaskAsync(string address, ushort andMask, ushort orMask)
	{
		return await ModbusHelper.WriteMaskAsync(this, address, andMask, orMask).ConfigureAwait(false);
	}

	public virtual async Task<OperateResult> WriteOneRegisterAsync(string address, short value)
	{
		return await WriteAsync(address, value).ConfigureAwait(false);
	}

	public virtual async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value)
	{
		return await WriteAsync(address, value).ConfigureAwait(false);
	}

	/// <summary>
	/// 批量读取线圈或是离散的数据信息，需要指定地址和长度，具体的结果取决于实现，如果富文本地址不指定，默认使用的功能码是 0x01
	/// </summary>
	/// <param name="address">数据地址，比如 "1234" </param>
	/// <param name="length">数据长度</param>
	/// <returns>带有成功标识的bool[]数组</returns>
	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		return ModbusHelper.ReadBoolHelper(this, address, length, 1);
	}

	/// <summary>
	/// 向线圈中写入bool数组，返回是否写入成功，如果富文本地址不指定，默认使用的功能码是 0x0F
	/// </summary>
	/// <param name="address">要写入的数据地址，比如"1234"</param>
	/// <param name="values">要写入的实际数组</param>
	/// <returns>返回写入结果</returns>
	public override OperateResult Write(string address, bool[] values)
	{
		return ModbusHelper.Write(this, address, values);
	}

	/// <summary>
	/// 向线圈中写入bool数值，返回是否写入成功，如果富文本地址不指定，默认使用的功能码是 0x05，
	/// 如果你的地址为字地址，例如100.2，那么将使用0x16的功能码，通过掩码的方式来修改寄存器的某一位，需要Modbus服务器支持，否则写入无效。
	/// </summary>
	/// <param name="address">要写入的数据地址，比如"12345"</param>
	/// <param name="value">要写入的实际数据</param>
	/// <returns>返回写入结果</returns>
	public override OperateResult Write(string address, bool value)
	{
		return ModbusHelper.Write(this, address, value);
	}

	private async Task<OperateResult<bool[]>> ReadBoolHelperAsync(string address, ushort length, byte function)
	{
		return await ModbusHelper.ReadBoolHelperAsync(this, address, length, function).ConfigureAwait(false);
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		return await ReadBoolHelperAsync(address, length, 1).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool[] values)
	{
		return await ModbusHelper.WriteAsync(this, address, values).ConfigureAwait(false);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool value)
	{
		return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
	}

	public override string ToString()
	{
		return $"ModbusTcpNet[{IpAddress}:{Port}]";
	}
}
