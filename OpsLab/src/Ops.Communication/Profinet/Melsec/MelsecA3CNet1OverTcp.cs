using System.Text;
using Ops.Communication.Address;
using Ops.Communication.Core;
using Ops.Communication.Core.Net;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Melsec;

/// <summary>
/// 基于Qna 兼容3C帧的格式一的通讯，具体的地址需要参照三菱的基本地址，本类是基于tcp通讯的实现。
/// </summary>
/// <remarks>
/// 地址可以携带站号信息，例如：s=2;D100
/// </remarks>
public sealed class MelsecA3CNet1OverTcp : NetworkDeviceBase
{
    public byte Station { get; set; } = 0;

    /// <summary>
    /// 实例化默认的对象。
    /// </summary>
    public MelsecA3CNet1OverTcp()
	{
		base.WordLength = 1;
		base.ByteTransform = new RegularByteTransform();
		base.SleepTime = 20;
	}

	/// <summary>
	/// 指定ip地址和端口号来实例化对象。
	/// </summary>
	/// <param name="ipAddress">Ip地址信息</param>
	/// <param name="port">端口号信息</param>
	public MelsecA3CNet1OverTcp(string ipAddress, int port)
		: this()
	{
		IpAddress = ipAddress;
		Port = port;
	}

	private OperateResult<byte[]> ReadWithPackCommand(byte[] command, byte station)
	{
		return ReadFromCoreServer(PackCommand(command, station));
	}

	private async Task<OperateResult<byte[]>> ReadWithPackCommandAsync(byte[] command)
	{
		return await ReadFromCoreServerAsync(PackCommand(command, Station));
	}

	/// <summary>
	/// 批量读取PLC的数据，以字为单位，支持读取X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认。
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="length">数据长度</param>
	/// <returns>读取结果信息</returns>
	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		return ReadHelper(address, length, b, ReadWithPackCommand);
	}

	/// <summary>
	/// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认。
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="value">数据值</param>
	/// <returns>是否写入成功</returns>
	public override OperateResult Write(string address, byte[] value)
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		return WriteHelper(address, value, b, ReadWithPackCommand);
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		byte stat = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address, length);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(addressResult);
		}

		byte[] command = MelsecHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, isBit: false);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command, stat)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		if (read.Content[0] != 2)
		{
			return new OperateResult<byte[]>((int)ConnErrorCode.MelsecReadFailed, "Read Failed:" + Encoding.ASCII.GetString(read.Content, 1, read.Content.Length - 1));
		}

		byte[] content = new byte[length * 2];
		for (int i = 0; i < content.Length / 2; i++)
		{
			ushort tmp = Convert.ToUInt16(Encoding.ASCII.GetString(read.Content, i * 4 + 11, 4), 16);
			BitConverter.GetBytes(tmp).CopyTo(content, i * 2);
		}
		return OperateResult.Ok(content);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		byte stat = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address, 0);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(addressResult);
		}

		byte[] command = MelsecHelper.BuildAsciiWriteWordCoreCommand(addressResult.Content, value);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command, stat)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		if (read.Content[0] != 6)
		{
			return new OperateResult((int)ConnErrorCode.MelsecWriteFailed, "Write Failed:" + Encoding.ASCII.GetString(read.Content, 1, read.Content.Length - 1));
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 批量读取bool类型数据，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型。
	/// </summary>
	/// <param name="address">地址信息，比如X10,Y17，注意X，Y的地址是8进制的</param>
	/// <param name="length">读取的长度</param>
	/// <returns>读取结果信息</returns>
	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		return ReadBoolHelper(address, length, b, ReadWithPackCommand);
	}

	/// <summary>
	/// 批量写入bool类型的数组，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型。
	/// </summary>
	/// <param name="address">PLC的地址信息</param>
	/// <param name="value">数据信息</param>
	/// <returns>是否写入成功</returns>
	public override OperateResult Write(string address, bool[] value)
	{
		byte b = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		return WriteHelper(address, value, b, ReadWithPackCommand);
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		byte stat = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address, length);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(addressResult);
		}

		byte[] command = MelsecHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, isBit: true);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command, stat)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<bool[]>(read);
		}

		if (read.Content[0] != 2)
		{
			return new OperateResult<bool[]>((int)ConnErrorCode.MelsecReadFailed, "Read Failed:" + Encoding.ASCII.GetString(read.Content, 1, read.Content.Length - 1));
		}

		byte[] buffer = new byte[length];
		Array.Copy(read.Content, 11, buffer, 0, length);
		return OperateResult.Ok(buffer.Select((byte m) => m == 49).ToArray());
	}

	public override async Task<OperateResult> WriteAsync(string address, bool[] value)
	{
		byte stat = (byte)ConnHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address, 0);
		if (!addressResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(addressResult);
		}

		byte[] command = MelsecHelper.BuildAsciiWriteBitCoreCommand(addressResult.Content, value);
		OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command, stat)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		if (read.Content[0] != 6)
		{
			return new OperateResult((int)ConnErrorCode.MelsecWriteFailed, "Write Failed:" + Encoding.ASCII.GetString(read.Content, 1, read.Content.Length - 1));
		}
		return OperateResult.Ok();
	}

	public OperateResult RemoteRun()
	{
		return RemoteRunHelper(Station, ReadWithPackCommand);
	}

	public OperateResult RemoteStop()
	{
		return RemoteStopHelper(Station, ReadWithPackCommand);
	}

	public OperateResult<string> ReadPlcType()
	{
		return ReadPlcTypeHelper(Station, ReadWithPackCommand);
	}

	public async Task<OperateResult> RemoteRunAsync()
	{
		OperateResult<byte[]> read = await ReadWithPackCommandAsync(Encoding.ASCII.GetBytes("1001000000010000"));
		if (!read.IsSuccess)
		{
			return read;
		}

		if (read.Content[0] != 6 && read.Content[0] != 2)
		{
			return new OperateResult((int)ConnErrorCode.MelsecStartPLCFailed, "Failed:" + Encoding.ASCII.GetString(read.Content, 1, read.Content.Length - 1));
		}
		return OperateResult.Ok();
	}

	public async Task<OperateResult> RemoteStopAsync()
	{
		OperateResult<byte[]> read = await ReadWithPackCommandAsync(Encoding.ASCII.GetBytes("100200000001")).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		if (read.Content[0] != 6 && read.Content[0] != 2)
		{
			return new OperateResult((int)ConnErrorCode.MelsecStopPLCFailed, "Failed:" + Encoding.ASCII.GetString(read.Content, 1, read.Content.Length - 1));
		}
		return OperateResult.Ok();
	}

	public async Task<OperateResult<string>> ReadPlcTypeAsync()
	{
		OperateResult<byte[]> read = await ReadWithPackCommandAsync(Encoding.ASCII.GetBytes("01010000")).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<string>(read);
		}

		if (read.Content[0] != 6 && read.Content[0] != 2)
		{
			return new OperateResult<string>((int)ConnErrorCode.MelsecReadPlcTypeError, "Failed:" + Encoding.ASCII.GetString(read.Content, 1, read.Content.Length - 1));
		}
		return OperateResult.Ok(Encoding.ASCII.GetString(read.Content, 11, 16).TrimEnd(Array.Empty<char>()));
	}

	public override string ToString()
	{
		return $"MelsecA3CNet1OverTcp[{IpAddress}:{Port}]";
	}

	/// <summary>
	/// 批量读取PLC的数据，以字为单位，支持读取X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="length">数据长度</param>
	/// <param name="station">当前PLC的站号信息</param>
	/// <param name="readCore">通信的载体信息</param>
	/// <returns>读取结果信息</returns>
	public static OperateResult<byte[]> ReadHelper(string address, ushort length, byte station, Func<byte[], byte, OperateResult<byte[]>> readCore)
	{
		OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		byte[] arg = MelsecHelper.BuildAsciiReadMcCoreCommand(operateResult.Content, isBit: false);
		OperateResult<byte[]> operateResult2 = readCore(arg, station);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		if (operateResult2.Content[0] != 2)
		{
			return new OperateResult<byte[]>((int)ConnErrorCode.MelsecReadFailed, "Read Failed:" + Encoding.ASCII.GetString(operateResult2.Content, 1, operateResult2.Content.Length - 1));
		}

		byte[] array = new byte[length * 2];
		for (int i = 0; i < array.Length / 2; i++)
		{
			ushort value = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult2.Content, i * 4 + 11, 4), 16);
			BitConverter.GetBytes(value).CopyTo(array, i * 2);
		}
		return OperateResult.Ok(array);
	}

	/// <summary>
	/// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="value">数据值</param>
	/// <param name="station">当前PLC的站号信息</param>
	/// <param name="readCore">通信的载体信息</param>
	/// <returns>是否写入成功</returns>
	public static OperateResult WriteHelper(string address, byte[] value, byte station, Func<byte[], byte, OperateResult<byte[]>> readCore)
	{
		OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address, 0);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		byte[] arg = MelsecHelper.BuildAsciiWriteWordCoreCommand(operateResult.Content, value);
		OperateResult<byte[]> operateResult2 = readCore(arg, station);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		if (operateResult2.Content[0] != 6)
		{
			return new OperateResult((int)ConnErrorCode.MelsecWriteFailed, "Write Failed:" + Encoding.ASCII.GetString(operateResult2.Content, 1, operateResult2.Content.Length - 1));
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 批量读取bool类型数据，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
	/// </summary>
	/// <param name="address">地址信息，比如X10,Y17，注意X，Y的地址是8进制的</param>
	/// <param name="length">读取的长度</param>
	/// <param name="station">当前PLC的站号信息</param>
	/// <param name="readCore">通信的载体信息</param>
	/// <returns>读取结果信息</returns>
	public static OperateResult<bool[]> ReadBoolHelper(string address, ushort length, byte station, Func<byte[], byte, OperateResult<byte[]>> readCore)
	{
		OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address, length);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		byte[] arg = MelsecHelper.BuildAsciiReadMcCoreCommand(operateResult.Content, isBit: true);
		OperateResult<byte[]> operateResult2 = readCore(arg, station);
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult2);
		}

		if (operateResult2.Content[0] != 2)
		{
			return new OperateResult<bool[]>((int)ConnErrorCode.MelsecReadFailed, "Read Faild:" + Encoding.ASCII.GetString(operateResult2.Content, 1, operateResult2.Content.Length - 1));
		}
		byte[] array = new byte[length];
		Array.Copy(operateResult2.Content, 11, array, 0, length);
		return OperateResult.Ok(array.Select((byte m) => m == 49).ToArray());
	}

	/// <summary>
	/// 批量写入bool类型的数组，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
	/// </summary>
	/// <param name="address">PLC的地址信息</param>
	/// <param name="value">数据信息</param>
	/// <param name="station">当前PLC的站号信息</param>
	/// <param name="readCore">通信的载体信息</param>
	/// <returns>是否写入成功</returns>
	public static OperateResult WriteHelper(string address, bool[] value, byte station, Func<byte[], byte, OperateResult<byte[]>> readCore)
	{
		OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address, 0);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		byte[] arg = MelsecHelper.BuildAsciiWriteBitCoreCommand(operateResult.Content, value);
		OperateResult<byte[]> operateResult2 = readCore(arg, station);
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		if (operateResult2.Content[0] != 6)
		{
			return new OperateResult((int)ConnErrorCode.MelsecWriteFailed, "Write Failed:" + Encoding.ASCII.GetString(operateResult2.Content, 1, operateResult2.Content.Length - 1));
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 远程Run操作
	/// </summary>
	/// <param name="station">当前PLC的站号信息</param>
	/// <param name="readCore">通信的载体信息</param>
	/// <returns>是否成功</returns>
	public static OperateResult RemoteRunHelper(byte station, Func<byte[], byte, OperateResult<byte[]>> readCore)
	{
		OperateResult<byte[]> operateResult = readCore(Encoding.ASCII.GetBytes("1001000000010000"), station);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		if (operateResult.Content[0] != 6 && operateResult.Content[0] != 2)
		{
			return new OperateResult((int)ConnErrorCode.MelsecStartPLCFailed, "Failed:" + Encoding.ASCII.GetString(operateResult.Content, 1, operateResult.Content.Length - 1));
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 远程Stop操作
	/// </summary>
	/// <param name="station">当前PLC的站号信息</param>
	/// <param name="readCore">通信的载体信息</param>
	/// <returns>是否成功</returns>
	public static OperateResult RemoteStopHelper(byte station, Func<byte[], byte, OperateResult<byte[]>> readCore)
	{
		OperateResult<byte[]> operateResult = readCore(Encoding.ASCII.GetBytes("100200000001"), station);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		if (operateResult.Content[0] != 6 && operateResult.Content[0] != 2)
		{
			return new OperateResult((int)ConnErrorCode.MelsecStopPLCFailed, "Failed:" + Encoding.ASCII.GetString(operateResult.Content, 1, operateResult.Content.Length - 1));
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 读取PLC的型号信息
	/// </summary>
	/// <param name="station">当前PLC的站号信息</param>
	/// <param name="readCore">通信的载体信息</param>
	/// <returns>返回型号的结果对象</returns>
	public static OperateResult<string> ReadPlcTypeHelper(byte station, Func<byte[], byte, OperateResult<byte[]>> readCore)
	{
		OperateResult<byte[]> operateResult = readCore(Encoding.ASCII.GetBytes("01010000"), station);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult);
		}

		if (operateResult.Content[0] != 6 && operateResult.Content[0] != 2)
		{
			return new OperateResult<string>((int)ConnErrorCode.MelsecReadPlcTypeError, "Failed:" + Encoding.ASCII.GetString(operateResult.Content, 1, operateResult.Content.Length - 1));
		}
		return OperateResult.Ok(Encoding.ASCII.GetString(operateResult.Content, 11, 16).TrimEnd(Array.Empty<char>()));
	}

	/// <summary>
	/// 将命令进行打包传送
	/// </summary>
	/// <param name="mcCommand">mc协议的命令</param>
	/// <param name="station">PLC的站号</param>
	/// <returns>最终的原始报文信息</returns>
	public static byte[] PackCommand(byte[] mcCommand, byte station = 0)
	{
		byte[] array = new byte[13 + mcCommand.Length];
		array[0] = 5;
		array[1] = 70;
		array[2] = 57;
		array[3] = SoftBasic.BuildAsciiBytesFrom(station)[0];
		array[4] = SoftBasic.BuildAsciiBytesFrom(station)[1];
		array[5] = 48;
		array[6] = 48;
		array[7] = 70;
		array[8] = 70;
		array[9] = 48;
		array[10] = 48;
		mcCommand.CopyTo(array, 11);
		int num = 0;
		for (int i = 1; i < array.Length - 3; i++)
		{
			num += array[i];
		}
		array[^2] = SoftBasic.BuildAsciiBytesFrom((byte)num)[0];
		array[^1] = SoftBasic.BuildAsciiBytesFrom((byte)num)[1];
		return array;
	}
}
