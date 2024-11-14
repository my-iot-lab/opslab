namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 三菱R系列的PLC的数据类型
/// </summary>
public sealed class MelsecMcRDataType
{
	/// <summary>
	/// X输入继电器
	/// </summary>
	public static readonly MelsecMcRDataType X = new([156, 0], 1, "X***", 16);

	/// <summary>
	/// Y输入继电器
	/// </summary>
	public static readonly MelsecMcRDataType Y = new([157, 0], 1, "Y***", 16);

	/// <summary>
	/// M内部继电器
	/// </summary>
	public static readonly MelsecMcRDataType M = new([144, 0], 1, "M***", 10);

	/// <summary>
	/// 特殊继电器
	/// </summary>
	public static readonly MelsecMcRDataType SM = new([145, 0], 1, "SM**", 10);

	/// <summary>
	/// 锁存继电器
	/// </summary>
	public static readonly MelsecMcRDataType L = new([146, 0], 1, "L***", 10);

	/// <summary>
	/// 报警器
	/// </summary>
	public static readonly MelsecMcRDataType F = new([147, 0], 1, "F***", 10);

	/// <summary>
	/// 变址继电器
	/// </summary>
	public static readonly MelsecMcRDataType V = new([148, 0], 1, "V***", 10);

	/// <summary>
	/// S步进继电器
	/// </summary>
	public static readonly MelsecMcRDataType S = new([152, 0], 1, "S***", 10);

	/// <summary>
	/// 链接继电器
	/// </summary>
	public static readonly MelsecMcRDataType B = new([160, 0], 1, "B***", 16);

	/// <summary>
	/// 特殊链接继电器
	/// </summary>
	public static readonly MelsecMcRDataType SB = new([161, 0], 1, "SB**", 16);

	/// <summary>
	/// 直接访问输入继电器
	/// </summary>
	public static readonly MelsecMcRDataType DX = new([162, 0], 1, "DX**", 16);

	/// <summary>
	/// 直接访问输出继电器
	/// </summary>
	public static readonly MelsecMcRDataType DY = new([163, 0], 1, "DY**", 16);

	/// <summary>
	/// 数据寄存器
	/// </summary>
	public static readonly MelsecMcRDataType D = new([168, 0], 0, "D***", 10);

	/// <summary>
	/// 特殊数据寄存器
	/// </summary>
	public static readonly MelsecMcRDataType SD = new([169, 0], 0, "SD**", 10);

	/// <summary>
	/// 链接寄存器
	/// </summary>
	public static readonly MelsecMcRDataType W = new([180, 0], 0, "W***", 16);

	/// <summary>
	/// 特殊链接寄存器
	/// </summary>
	public static readonly MelsecMcRDataType SW = new([181, 0], 0, "SW**", 16);

	/// <summary>
	/// 文件寄存器
	/// </summary>
	public static readonly MelsecMcRDataType R = new([175, 0], 0, "R***", 10);

	/// <summary>
	/// 变址寄存器
	/// </summary>
	public static readonly MelsecMcRDataType Z = new([204, 0], 0, "Z***", 10);

	/// <summary>
	/// 长累计定时器触点
	/// </summary>
	public static readonly MelsecMcRDataType LSTS = new([89, 0], 1, "LSTS", 10);

	/// <summary>
	/// 长累计定时器线圈
	/// </summary>
	public static readonly MelsecMcRDataType LSTC = new([88, 0], 1, "LSTC", 10);

	/// <summary>
	/// 长累计定时器当前值
	/// </summary>
	public static readonly MelsecMcRDataType LSTN = new([90, 0], 0, "LSTN", 10);

	/// <summary>
	/// 累计定时器触点
	/// </summary>
	public static readonly MelsecMcRDataType STS = new([199, 0], 1, "STS*", 10);

	/// <summary>
	/// 累计定时器线圈
	/// </summary>
	public static readonly MelsecMcRDataType STC = new([198, 0], 1, "STC*", 10);

	/// <summary>
	/// 累计定时器当前值
	/// </summary>
	public static readonly MelsecMcRDataType STN = new([200, 0], 0, "STN*", 10);

	/// <summary>
	/// 长定时器触点
	/// </summary>
	public static readonly MelsecMcRDataType LTS = new([81, 0], 1, "LTS*", 10);

	/// <summary>
	/// 长定时器线圈
	/// </summary>
	public static readonly MelsecMcRDataType LTC = new([80, 0], 1, "LTC*", 10);

	/// <summary>
	/// 长定时器当前值
	/// </summary>
	public static readonly MelsecMcRDataType LTN = new([82, 0], 0, "LTN*", 10);

	/// <summary>
	/// 定时器触点
	/// </summary>
	public static readonly MelsecMcRDataType TS = new([193, 0], 1, "TS**", 10);

	/// <summary>
	/// 定时器线圈
	/// </summary>
	public static readonly MelsecMcRDataType TC = new([192, 0], 1, "TC**", 10);

	/// <summary>
	/// 定时器当前值
	/// </summary>
	public static readonly MelsecMcRDataType TN = new([194, 0], 0, "TN**", 10);

	/// <summary>
	/// 长计数器触点
	/// </summary>
	public static readonly MelsecMcRDataType LCS = new([85, 0], 1, "LCS*", 10);

	/// <summary>
	/// 长计数器线圈
	/// </summary>
	public static readonly MelsecMcRDataType LCC = new([84, 0], 1, "LCC*", 10);

	/// <summary>
	/// 长计数器当前值
	/// </summary>
	public static readonly MelsecMcRDataType LCN = new([86, 0], 0, "LCN*", 10);

	/// <summary>
	/// 计数器触点
	/// </summary>
	public static readonly MelsecMcRDataType CS = new([196, 0], 1, "CS**", 10);

	/// <summary>
	/// 计数器线圈
	/// </summary>
	public static readonly MelsecMcRDataType CC = new([195, 0], 1, "CC**", 10);

	/// <summary>
	/// 计数器当前值
	/// </summary>
	public static readonly MelsecMcRDataType CN = new([197, 0], 0, "CN**", 10);

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
