using System.Text;
using Ops.Communication.Core;
using Ops.Communication.Core.Net;
using Ops.Communication.Extensions;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的HostLink协议的实现，基于Tcp实现，地址支持示例 DM区:D100; CIO区:C100; Work区:W100; Holding区:H100; Auxiliary区: A100。
/// </summary>
/// <remarks>
/// <note type="important">
/// 如果发现串口线和usb同时打开才能通信的情况，需要按照如下的操作：<br />
/// 串口线不是标准的串口线，电脑的串口线的235引脚分别接PLC的329引脚，45线短接，就可以通讯。
/// </note>
/// </remarks>
/// <example>
/// 欧姆龙的地址参考如下：
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
/// </list>
/// </example>
public sealed class OmronHostLinkOverTcp : NetworkDeviceBase
{
	/// <summary>
	/// Specifies whether or not there are network relays. Set “80” (ASCII: 38,30) 
	/// when sending an FINS command to a CPU Unit on a network.Set “00” (ASCII: 30,30) 
	/// when sending to a CPU Unit connected directly to the host computer.
	/// </summary>
	public byte ICF { get; set; } = 0;

	public byte DA2 { get; set; } = 0;

	public byte SA2 { get; set; }

	public byte SID { get; set; } = 0;

	/// <summary>
	/// The response wait time sets the time from when the CPU Unit receives a command block until it starts 
	/// to return a response.It can be set from 0 to F in hexadecimal, in units of 10 ms.
	/// If F(15) is set, the response will begin to be returned 150 ms (15 × 10 ms) after the command block was received.
	/// </summary>
	public byte ResponseWaitTime { get; set; } = 48;

	/// <summary>
	/// PLC设备的站号信息。
	/// </summary>
	public byte UnitNumber { get; set; }

	/// <summary>
	/// 进行字读取的时候对于超长的情况按照本属性进行切割，默认260。
	/// </summary>
	public int ReadSplits { get; set; } = 260;

	public OmronHostLinkOverTcp()
	{
		base.ByteTransform = new ReverseWordTransform();
		base.WordLength = 1;
		base.ByteTransform.DataFormat = DataFormat.CDAB;
		base.SleepTime = 20;
	}

	public OmronHostLinkOverTcp(string ipAddress, int port)
		: this()
	{
		IpAddress = ipAddress;
		Port = port;
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var operateResult = OmronFinsNetHelper.BuildReadCommand(address, length, isBit: false, ReadSplits);
		if (!operateResult.IsSuccess)
		{
			return operateResult.ConvertError<byte[]>();
		}

		var list = new List<byte>();
		for (int i = 0; i < length; i++)
		{
			var operateResult2 = ReadFromCoreServer(PackCommand(station, operateResult.Content[i]));
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult2);
			}

			var operateResult3 = ResponseValidAnalysis(operateResult2.Content, isRead: true);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult3);
			}
			list.AddRange(operateResult3.Content);
		}
		return OperateResult.Ok(list.ToArray());
	}

	public override OperateResult Write(string address, byte[] value)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var operateResult = OmronFinsNetHelper.BuildWriteWordCommand(address, value, isBit: false);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReadFromCoreServer(PackCommand(station, operateResult.Content));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		var operateResult3 = ResponseValidAnalysis(operateResult2.Content, isRead: false);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var command = OmronFinsNetHelper.BuildReadCommand(address, length, isBit: false, ReadSplits);
		if (!command.IsSuccess)
		{
			return command.ConvertError<byte[]>();
		}

		var contentArray = new List<byte>();
		for (int i = 0; i < length; i++)
		{
			var read = await ReadFromCoreServerAsync(PackCommand(station, command.Content[i])).ConfigureAwait(false);
			if (!read.IsSuccess)
			{
				return OperateResult.Error<byte[]>(read);
			}

			var valid = ResponseValidAnalysis(read.Content, isRead: true);
			if (!valid.IsSuccess)
			{
				return OperateResult.Error<byte[]>(valid);
			}
			contentArray.AddRange(valid.Content);
		}
		return OperateResult.Ok(contentArray.ToArray());
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var command = OmronFinsNetHelper.BuildWriteWordCommand(address, value, isBit: false);
		if (!command.IsSuccess)
		{
			return command;
		}

		var read = await ReadFromCoreServerAsync(PackCommand(station, command.Content)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		var valid = ResponseValidAnalysis(read.Content, isRead: false);
		if (!valid.IsSuccess)
		{
			return valid;
		}
		return OperateResult.Ok();
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var operateResult = OmronFinsNetHelper.BuildReadCommand(address, length, isBit: true);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}

		var list = new List<bool>();
		for (int i = 0; i < operateResult.Content.Count; i++)
		{
			var operateResult2 = ReadFromCoreServer(PackCommand(station, operateResult.Content[i]));
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult2);
			}

			var operateResult3 = ResponseValidAnalysis(operateResult2.Content, isRead: true);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult3);
			}
			list.AddRange(operateResult3.Content.Select((byte m) => (m != 0)));
		}
		return OperateResult.Ok(list.ToArray());
	}

	public override OperateResult Write(string address, bool[] values)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var operateResult = OmronFinsNetHelper.BuildWriteWordCommand(address, values.Select((bool m) => (byte)(m ? 1 : 0)).ToArray(), isBit: true);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReadFromCoreServer(PackCommand(station, operateResult.Content));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		var operateResult3 = ResponseValidAnalysis(operateResult2.Content, isRead: false);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var command = OmronFinsNetHelper.BuildReadCommand(address, length, isBit: true);
		if (!command.IsSuccess)
		{
			return OperateResult.Error<bool[]>(command);
		}

		var contentArray = new List<bool>();
		for (int i = 0; i < command.Content.Count; i++)
		{
			var read = await ReadFromCoreServerAsync(PackCommand(station, command.Content[i])).ConfigureAwait(false);
			if (!read.IsSuccess)
			{
				return OperateResult.Error<bool[]>(read);
			}

			var valid = ResponseValidAnalysis(read.Content, isRead: true);
			if (!valid.IsSuccess)
			{
				return OperateResult.Error<bool[]>(valid);
			}
			contentArray.AddRange(valid.Content.Select((byte m) => (m != 0)));
		}
		return OperateResult.Ok(contentArray.ToArray());
	}

	public override async Task<OperateResult> WriteAsync(string address, bool[] values)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var command = OmronFinsNetHelper.BuildWriteWordCommand(address, values.Select((bool m) => (byte)(m ? 1 : 0)).ToArray(), isBit: true);
		if (!command.IsSuccess)
		{
			return command;
		}

		var read = await ReadFromCoreServerAsync(PackCommand(station, command.Content)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		var valid = ResponseValidAnalysis(read.Content, isRead: false);
		if (!valid.IsSuccess)
		{
			return valid;
		}
		return OperateResult.Ok();
	}

	public override string ToString()
	{
		return $"OmronHostLinkOverTcp[{IpAddress}:{Port}]";
	}

	/// <summary>
	/// 将普通的指令打包成完整的指令
	/// </summary>
	/// <param name="station">PLC的站号信息</param>
	/// <param name="cmd">fins指令</param>
	/// <returns>完整的质量</returns>
	private byte[] PackCommand(byte station, byte[] cmd)
	{
		cmd = SoftBasic.BytesToAsciiBytes(cmd);
		byte[] array = new byte[18 + cmd.Length];
		array[0] = 64;
		array[1] = SoftBasic.BuildAsciiBytesFrom(station)[0];
		array[2] = SoftBasic.BuildAsciiBytesFrom(station)[1];
		array[3] = 70;
		array[4] = 65;
		array[5] = ResponseWaitTime;
		array[6] = SoftBasic.BuildAsciiBytesFrom(ICF)[0];
		array[7] = SoftBasic.BuildAsciiBytesFrom(ICF)[1];
		array[8] = SoftBasic.BuildAsciiBytesFrom(DA2)[0];
		array[9] = SoftBasic.BuildAsciiBytesFrom(DA2)[1];
		array[10] = SoftBasic.BuildAsciiBytesFrom(SA2)[0];
		array[11] = SoftBasic.BuildAsciiBytesFrom(SA2)[1];
		array[12] = SoftBasic.BuildAsciiBytesFrom(SID)[0];
		array[13] = SoftBasic.BuildAsciiBytesFrom(SID)[1];
		array[^2] = 42;
		array[^1] = 13;
		cmd.CopyTo(array, 14);
		int num = array[0];

		for (int i = 1; i < array.Length - 4; i++)
		{
			num ^= array[i];
		}

		array[^4] = SoftBasic.BuildAsciiBytesFrom((byte)num)[0];
		array[^3] = SoftBasic.BuildAsciiBytesFrom((byte)num)[1];
		string @string = Encoding.ASCII.GetString(array);
		Console.WriteLine(@string);
		return array;
	}

	/// <summary>
	/// 验证欧姆龙的Fins-TCP返回的数据是否正确的数据，如果正确的话，并返回所有的数据内容
	/// </summary>
	/// <param name="response">来自欧姆龙返回的数据内容</param>
	/// <param name="isRead">是否读取</param>
	/// <returns>带有是否成功的结果对象</returns>
	public static OperateResult<byte[]> ResponseValidAnalysis(byte[] response, bool isRead)
	{
		if (response.Length >= 27)
		{
			if (int.TryParse(Encoding.ASCII.GetString(response, 19, 4), out var result))
			{
				byte[] array = null;
				if (response.Length > 27)
				{
					array = SoftBasic.HexStringToBytes(Encoding.ASCII.GetString(response, 23, response.Length - 27));
				}

				if (result > 0)
				{
					return new OperateResult<byte[]>
					{
						ErrorCode = result,
						Content = array
					};
				}
				return OperateResult.Ok(array);
			}
			return new OperateResult<byte[]>($"Parse error code failed, [{Encoding.ASCII.GetString(response, 19, 4)}]{Environment.NewLine} Source Data: {response.ToHexString(' ')}");
		}

		return new OperateResult<byte[]>($"{ErrorCode.OmronReceiveDataError.Desc()} Source Data: {response.ToHexString(' ')}");
	}
}
