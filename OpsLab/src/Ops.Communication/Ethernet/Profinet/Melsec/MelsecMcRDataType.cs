namespace Ops.Communication.Ethernet.Profinet.Melsec;

/// <summary>
/// 三菱R系列的PLC的数据类型
/// </summary>
public class MelsecMcRDataType
{
	/// <summary>
	/// X输入继电器
	/// </summary>
	public static readonly MelsecMcRDataType X = new(new byte[2] { 156, 0 }, 1, "X***", 16);

	/// <summary>
	/// Y输入继电器
	/// </summary>
	public static readonly MelsecMcRDataType Y = new(new byte[2] { 157, 0 }, 1, "Y***", 16);

	/// <summary>
	/// M内部继电器
	/// </summary>
	public static readonly MelsecMcRDataType M = new(new byte[2] { 144, 0 }, 1, "M***", 10);

	/// <summary>
	/// 特殊继电器
	/// </summary>
	public static readonly MelsecMcRDataType SM = new(new byte[2] { 145, 0 }, 1, "SM**", 10);

	/// <summary>
	/// 锁存继电器
	/// </summary>
	public static readonly MelsecMcRDataType L = new(new byte[2] { 146, 0 }, 1, "L***", 10);

	/// <summary>
	/// 报警器
	/// </summary>
	public static readonly MelsecMcRDataType F = new(new byte[2] { 147, 0 }, 1, "F***", 10);

	/// <summary>
	/// 变址继电器
	/// </summary>
	public static readonly MelsecMcRDataType V = new(new byte[2] { 148, 0 }, 1, "V***", 10);

	/// <summary>
	/// S步进继电器
	/// </summary>
	public static readonly MelsecMcRDataType S = new(new byte[2] { 152, 0 }, 1, "S***", 10);

	/// <summary>
	/// 链接继电器
	/// </summary>
	public static readonly MelsecMcRDataType B = new(new byte[2] { 160, 0 }, 1, "B***", 16);

	/// <summary>
	/// 特殊链接继电器
	/// </summary>
	public static readonly MelsecMcRDataType SB = new(new byte[2] { 161, 0 }, 1, "SB**", 16);

	/// <summary>
	/// 直接访问输入继电器
	/// </summary>
	public static readonly MelsecMcRDataType DX = new(new byte[2] { 162, 0 }, 1, "DX**", 16);

	/// <summary>
	/// 直接访问输出继电器
	/// </summary>
	public static readonly MelsecMcRDataType DY = new(new byte[2] { 163, 0 }, 1, "DY**", 16);

	/// <summary>
	/// 数据寄存器
	/// </summary>
	public static readonly MelsecMcRDataType D = new(new byte[2] { 168, 0 }, 0, "D***", 10);

	/// <summary>
	/// 特殊数据寄存器
	/// </summary>
	public static readonly MelsecMcRDataType SD = new(new byte[2] { 169, 0 }, 0, "SD**", 10);

	/// <summary>
	/// 链接寄存器
	/// </summary>
	public static readonly MelsecMcRDataType W = new(new byte[2] { 180, 0 }, 0, "W***", 16);

	/// <summary>
	/// 特殊链接寄存器
	/// </summary>
	public static readonly MelsecMcRDataType SW = new(new byte[2] { 181, 0 }, 0, "SW**", 16);

	/// <summary>
	/// 文件寄存器
	/// </summary>
	public static readonly MelsecMcRDataType R = new(new byte[2] { 175, 0 }, 0, "R***", 10);

	/// <summary>
	/// 变址寄存器
	/// </summary>
	public static readonly MelsecMcRDataType Z = new(new byte[2] { 204, 0 }, 0, "Z***", 10);

	/// <summary>
	/// 长累计定时器触点
	/// </summary>
	public static readonly MelsecMcRDataType LSTS = new(new byte[2] { 89, 0 }, 1, "LSTS", 10);

	/// <summary>
	/// 长累计定时器线圈
	/// </summary>
	public static readonly MelsecMcRDataType LSTC = new(new byte[2] { 88, 0 }, 1, "LSTC", 10);

	/// <summary>
	/// 长累计定时器当前值
	/// </summary>
	public static readonly MelsecMcRDataType LSTN = new(new byte[2] { 90, 0 }, 0, "LSTN", 10);

	/// <summary>
	/// 累计定时器触点
	/// </summary>
	public static readonly MelsecMcRDataType STS = new(new byte[2] { 199, 0 }, 1, "STS*", 10);

	/// <summary>
	/// 累计定时器线圈
	/// </summary>
	public static readonly MelsecMcRDataType STC = new(new byte[2] { 198, 0 }, 1, "STC*", 10);

	/// <summary>
	/// 累计定时器当前值
	/// </summary>
	public static readonly MelsecMcRDataType STN = new(new byte[2] { 200, 0 }, 0, "STN*", 10);

	/// <summary>
	/// 长定时器触点
	/// </summary>
	public static readonly MelsecMcRDataType LTS = new(new byte[2] { 81, 0 }, 1, "LTS*", 10);

	/// <summary>
	/// 长定时器线圈
	/// </summary>
	public static readonly MelsecMcRDataType LTC = new(new byte[2] { 80, 0 }, 1, "LTC*", 10);

	/// <summary>
	/// 长定时器当前值
	/// </summary>
	public static readonly MelsecMcRDataType LTN = new(new byte[2] { 82, 0 }, 0, "LTN*", 10);

	/// <summary>
	/// 定时器触点
	/// </summary>
	public static readonly MelsecMcRDataType TS = new(new byte[2] { 193, 0 }, 1, "TS**", 10);

	/// <summary>
	/// 定时器线圈
	/// </summary>
	public static readonly MelsecMcRDataType TC = new(new byte[2] { 192, 0 }, 1, "TC**", 10);

	/// <summary>
	/// 定时器当前值
	/// </summary>
	public static readonly MelsecMcRDataType TN = new(new byte[2] { 194, 0 }, 0, "TN**", 10);

	/// <summary>
	/// 长计数器触点
	/// </summary>
	public static readonly MelsecMcRDataType LCS = new(new byte[2] { 85, 0 }, 1, "LCS*", 10);

	/// <summary>
	/// 长计数器线圈
	/// </summary>
	public static readonly MelsecMcRDataType LCC = new(new byte[2] { 84, 0 }, 1, "LCC*", 10);

	/// <summary>
	/// 长计数器当前值
	/// </summary>
	public static readonly MelsecMcRDataType LCN = new(new byte[2] { 86, 0 }, 0, "LCN*", 10);

	/// <summary>
	/// 计数器触点
	/// </summary>
	public static readonly MelsecMcRDataType CS = new(new byte[2] { 196, 0 }, 1, "CS**", 10);

	/// <summary>
	/// 计数器线圈
	/// </summary>
	public static readonly MelsecMcRDataType CC = new(new byte[2] { 195, 0 }, 1, "CC**", 10);

	/// <summary>
	/// 计数器当前值
	/// </summary>
	public static readonly MelsecMcRDataType CN = new(new byte[2] { 197, 0 }, 0, "CN**", 10);

	/// <summary>
	/// 类型的代号值
	/// </summary>
	public byte[] DataCode { get; private set; } = new byte[2];


	/// <summary>
	/// 数据的类型，0代表按字，1代表按位
	/// </summary>
	public byte DataType { get; private set; } = 0;


	/// <summary>
	/// 当以ASCII格式通讯时的类型描述
	/// </summary>
	public string AsciiCode { get; private set; }

	/// <summary>
	/// 指示地址是10进制，还是16进制的
	/// </summary>
	public int FromBase { get; private set; }

	/// <summary>
	/// 如果您清楚类型代号，可以根据值进行扩展
	/// </summary>
	/// <param name="code">数据类型的代号</param>
	/// <param name="type">0或1，默认为0</param>
	/// <param name="asciiCode">ASCII格式的类型信息</param>
	/// <param name="fromBase">指示地址的多少进制的，10或是16</param>
	public MelsecMcRDataType(byte[] code, byte type, string asciiCode, int fromBase)
	{
		DataCode = code;
		AsciiCode = asciiCode;
		FromBase = fromBase;
		if (type < 2)
		{
			DataType = type;
		}
	}
}
