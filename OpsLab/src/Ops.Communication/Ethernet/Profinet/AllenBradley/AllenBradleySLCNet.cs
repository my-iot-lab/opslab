using System.Net.Sockets;
using Ops.Communication.Core;
using Ops.Communication.Core.Message;
using Ops.Communication.Core.Net;
using Ops.Communication.Extensions;

namespace Ops.Communication.Ethernet.Profinet.AllenBradley;

/// <summary>
/// AllenBradley品牌的PLC，针对SLC系列的通信的实现，测试PLC为1747。
/// </summary>
/// <remarks>
/// 地址格式如下：
/// <list type="table">
///   <listheader>
///     <term>地址代号</term>
///     <term>字操作</term>
///     <term>位操作</term>
///     <term>备注</term>
///   </listheader>
///   <item>
///     <term>A</term>
///     <term>A9:0</term>
///     <term>A9:0/1 或 A9:0.1</term>
///     <term></term>
///   </item>
///   <item>
///     <term>B</term>
///     <term>B9:0</term>
///     <term>B9:0/1 或 B9:0.1</term>
///     <term></term>
///   </item>
///   <item>
///     <term>N</term>
///     <term>N9:0</term>
///     <term>N9:0/1 或 N9:0.1</term>
///     <term></term>
///   </item>
///   <item>
///     <term>F</term>
///     <term>F9:0</term>
///     <term>F9:0/1 或 F9:0.1</term>
///     <term></term>
///   </item>
///   <item>
///     <term>S</term>
///     <term>S:0</term>
///     <term>S:0/1 或 S:0.1</term>
///     <term>S:0 等同于 S2:0</term>
///   </item>
///   <item>
///     <term>C</term>
///     <term>C9:0</term>
///     <term>C9:0/1 或 C9:0.1</term>
///     <term></term>
///   </item>
///   <item>
///     <term>I</term>
///     <term>I9:0</term>
///     <term>I9:0/1 或 I9:0.1</term>
///     <term></term>
///   </item>
///   <item>
///     <term>O</term>
///     <term>O9:0</term>
///     <term>O9:0/1 或 O9:0.1</term>
///     <term></term>
///   </item>
///   <item>
///     <term>R</term>
///     <term>R9:0</term>
///     <term>R9:0/1 或 R9:0.1</term>
///     <term></term>
///   </item>
///   <item>
///     <term>T</term>
///     <term>T9:0</term>
///     <term>T9:0/1 或 T9:0.1</term>
///     <term></term>
///   </item>
/// </list>
/// 感谢 seedee 的测试支持。
/// </remarks>
public class AllenBradleySLCNet : NetworkDeviceBase
{
	/// <summary>
	/// The current session handle, which is determined by the PLC when communicating with the PLC handshake
	/// </summary>
	public uint SessionHandle { get; protected set; }

	/// <summary>
	/// Instantiate a communication object for a Allenbradley PLC protocol
	/// </summary>
	public AllenBradleySLCNet()
	{
		WordLength = 2;
		ByteTransform = new RegularByteTransform();
	}

	/// <summary>
	/// Instantiate a communication object for a Allenbradley PLC protocol
	/// </summary>
	/// <param name="ipAddress">PLC IpAddress</param>
	/// <param name="port">PLC Port</param>
	public AllenBradleySLCNet(string ipAddress, int port = 44818)
	{
		WordLength = 2;
		IpAddress = ipAddress;
		Port = port;
		ByteTransform = new RegularByteTransform();
	}

	protected override INetMessage GetNewNetMessage()
	{
		return new AllenBradleySLCMessage();
	}

	protected override OperateResult InitializationOnConnect(Socket socket)
	{
		var operateResult = ReadFromCoreServer(socket, "01 01 00 00 00 00 00 00 00 00 00 00 00 04 00 05 00 00 00 00 00 00 00 00 00 00 00 00".ToHexBytes());
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		SessionHandle = ByteTransform.TransUInt32(operateResult.Content, 4);
		return OperateResult.CreateSuccessResult();
	}

	protected override async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
	{
		var read = await ReadFromCoreServerAsync(socket, "01 01 00 00 00 00 00 00 00 00 00 00 00 04 00 05 00 00 00 00 00 00 00 00 00 00 00 00".ToHexBytes());
		if (!read.IsSuccess)
		{
			return read;
		}

		SessionHandle = ByteTransform.TransUInt32(read.Content, 4);
		return OperateResult.CreateSuccessResult();
	}

	/// <summary>
	/// Read data information, data length for read array length information
	/// </summary>
	/// <param name="address">Address format of the node</param>
	/// <param name="length">In the case of arrays, the length of the array </param>
	/// <returns>Result data with result object </returns>
	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		OperateResult<byte[]> operateResult = BuildReadCommand(address, length);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackCommand(operateResult.Content));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult<byte[]> operateResult3 = ExtraActualContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.CreateSuccessResult(operateResult3.Content);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		OperateResult<byte[]> operateResult = BuildWriteCommand(address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackCommand(operateResult.Content));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult<byte[]> operateResult3 = ExtraActualContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.CreateSuccessResult(operateResult3.Content);
	}

	public override OperateResult<bool> ReadBool(string address)
	{
		address = AnalysisBitIndex(address, out var bitIndex);
		OperateResult<byte[]> operateResult = Read(address, 1);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.CreateFailedResult<bool>(operateResult);
		}

		return OperateResult.CreateSuccessResult(operateResult.Content.ToBoolArray()[bitIndex]);
	}

	public override OperateResult Write(string address, bool value)
	{
		OperateResult<byte[]> operateResult = BuildWriteCommand(address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackCommand(operateResult.Content));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult<byte[]> operateResult3 = ExtraActualContent(operateResult2.Content);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.CreateSuccessResult(operateResult3.Content);
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		OperateResult<byte[]> command = BuildReadCommand(address, length);
		if (!command.IsSuccess)
		{
			return command;
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command.Content));
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult<byte[]> extra = ExtraActualContent(read.Content);
		if (!extra.IsSuccess)
		{
			return extra;
		}

		return OperateResult.CreateSuccessResult(extra.Content);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		OperateResult<byte[]> command = BuildWriteCommand(address, value);
		if (!command.IsSuccess)
		{
			return command;
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command.Content));
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult<byte[]> extra = ExtraActualContent(read.Content);
		if (!extra.IsSuccess)
		{
			return extra;
		}
		return OperateResult.CreateSuccessResult(extra.Content);
	}

	public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
	{
		address = AnalysisBitIndex(address, out var bitIndex);
		OperateResult<byte[]> read = await ReadAsync(address, 1);
		if (!read.IsSuccess)
		{
			return OperateResult.CreateFailedResult<bool>(read);
		}
		return OperateResult.CreateSuccessResult(read.Content.ToBoolArray()[bitIndex]);
	}

	public override async Task<OperateResult> WriteAsync(string address, bool value)
	{
		OperateResult<byte[]> command = BuildWriteCommand(address, value);
		if (!command.IsSuccess)
		{
			return command;
		}

		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command.Content));
		if (!read.IsSuccess)
		{
			return read;
		}

		OperateResult<byte[]> extra = ExtraActualContent(read.Content);
		if (!extra.IsSuccess)
		{
			return extra;
		}
		return OperateResult.CreateSuccessResult(extra.Content);
	}

	private byte[] PackCommand(byte[] coreCmd)
	{
		byte[] array = new byte[28 + coreCmd.Length];
		array[0] = 1;
		array[1] = 7;
		array[2] = (byte)(coreCmd.Length / 256);
		array[3] = (byte)(coreCmd.Length % 256);
		BitConverter.GetBytes(SessionHandle).CopyTo(array, 4);
		coreCmd.CopyTo(array, 28);
		return array;
	}

	/// <summary>
	/// 分析地址数据信息里的位索引的信息
	/// </summary>
	/// <param name="address">数据地址</param>
	/// <param name="bitIndex">位索引</param>
	/// <returns>地址信息</returns>
	public static string AnalysisBitIndex(string address, out int bitIndex)
	{
		bitIndex = 0;
		int num = address.IndexOf('/');
		if (num < 0)
		{
			num = address.IndexOf('.');
		}

		if (num > 0)
		{
			bitIndex = int.Parse(address[(num + 1)..]);
			address = address[..num];
		}

		return address;
	}

	/// <summary>
	/// 分析当前的地址信息，返回类型代号，区块号，起始地址。
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <returns>结果内容对象</returns>
	public static OperateResult<byte, byte, ushort> AnalysisAddress(string address)
	{
		if (!address.Contains(':'))
		{
			return new OperateResult<byte, byte, ushort>("Address can't find ':', example : A9:0");
		}

		string[] array = address.Split(new char[1] { ':' });
		try
		{
			var operateResult = new OperateResult<byte, byte, ushort>
			{
				Content1 = array[0][0] switch
				{
					'A' => 142,
					'B' => 133,
					'N' => 137,
					'F' => 138,
					'S' => 132,
					'C' => 135,
					'I' => 131,
					'O' => 130,
					'R' => 136,
					'T' => 134,
					_ => throw new Exception("Address code wrong, must be A,B,N,F,S,C,I,O,R,T"),
				},
			};

			if (operateResult.Content1 == 132)
			{
				operateResult.Content2 = 2;
			}
			else
			{
				operateResult.Content2 = byte.Parse(array[0].Substring(1));
			}

			operateResult.Content3 = ushort.Parse(array[1]);
			operateResult.IsSuccess = true;
			operateResult.Message = "Success";
			return operateResult;
		}
		catch (Exception ex)
		{
			return new OperateResult<byte, byte, ushort>($"Wrong Address formate: {ex.Message}");
		}
	}

	/// <summary>
	/// 构建读取的指令信息
	/// </summary>
	/// <param name="address">地址信息，举例：A9:0</param>
	/// <param name="length">读取的长度信息</param>
	/// <returns>是否成功</returns>
	public static OperateResult<byte[]> BuildReadCommand(string address, ushort length)
	{
		OperateResult<byte, byte, ushort> operateResult = AnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.CreateFailedResult<byte[]>(operateResult);
		}

		if (length < 2)
		{
			length = 2;
		}

		if (operateResult.Content1 == 142)
		{
			operateResult.Content3 /= 2;
		}

		byte[] array = new byte[14]
		{
			0,
			5,
			0,
			0,
			15,
			0,
			0,
			1,
			162,
			(byte)length,
			operateResult.Content2,
			operateResult.Content1,
			0,
			0,
		};
		BitConverter.GetBytes(operateResult.Content3).CopyTo(array, 12);
		return OperateResult.CreateSuccessResult(array);
	}

	/// <summary>
	/// 构建写入的报文内容，变成实际的数据
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="value">数据值</param>
	/// <returns>是否成功的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteCommand(string address, byte[] value)
	{
		OperateResult<byte, byte, ushort> operateResult = AnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.CreateFailedResult<byte[]>(operateResult);
		}

		if (operateResult.Content1 == 142)
		{
			operateResult.Content3 /= 2;
		}

		byte[] array = new byte[18 + value.Length];
		array[0] = 0;
		array[1] = 5;
		array[2] = 0;
		array[3] = 0;
		array[4] = 15;
		array[5] = 0;
		array[6] = 0;
		array[7] = 1;
		array[8] = 171;
		array[9] = byte.MaxValue;
		array[10] = BitConverter.GetBytes(value.Length)[0];
		array[11] = BitConverter.GetBytes(value.Length)[1];
		array[12] = operateResult.Content2;
		array[13] = operateResult.Content1;
		BitConverter.GetBytes(operateResult.Content3).CopyTo(array, 14);
		array[16] = byte.MaxValue;
		array[17] = byte.MaxValue;
		value.CopyTo(array, 18);
		return OperateResult.CreateSuccessResult(array);
	}

	/// <summary>
	/// 构建写入的报文内容，变成实际的数据
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="value">数据值</param>
	/// <returns>是否成功的结果对象</returns>
	public static OperateResult<byte[]> BuildWriteCommand(string address, bool value)
	{
		address = AnalysisBitIndex(address, out var bitIndex);
		OperateResult<byte, byte, ushort> operateResult = AnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.CreateFailedResult<byte[]>(operateResult);
		}

		if (operateResult.Content1 == 142)
		{
			operateResult.Content3 /= 2;
		}
		int value2 = 1 << bitIndex;
		byte[] array = new byte[20]
		{
			0, 5, 0, 0, 15, 0, 0, 1, 171, 255,
			2, 0, operateResult.Content2, operateResult.Content1, 0, 0, 0, 0, 0, 0
		};
		BitConverter.GetBytes(operateResult.Content3).CopyTo(array, 14);
		array[16] = BitConverter.GetBytes(value2)[0];
		array[17] = BitConverter.GetBytes(value2)[1];
		if (value)
		{
			array[18] = BitConverter.GetBytes(value2)[0];
			array[19] = BitConverter.GetBytes(value2)[1];
		}
		return OperateResult.CreateSuccessResult(array);
	}

	/// <summary>
	/// 解析当前的实际报文内容，变成数据内容
	/// </summary>
	/// <param name="content">报文内容</param>
	/// <returns>是否成功</returns>
	public static OperateResult<byte[]> ExtraActualContent(byte[] content)
	{
		if (content.Length < 36)
		{
			return new OperateResult<byte[]>($"{ErrorCode.ReceiveDataLengthTooShort.Desc()} {content.ToHexString(' ')}");
		}
		return OperateResult.CreateSuccessResult(content.RemoveBegin(36));
	}
}
