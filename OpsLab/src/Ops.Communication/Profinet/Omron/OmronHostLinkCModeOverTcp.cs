using System.Text;
using Ops.Communication.Core;
using Ops.Communication.Core.Net;

namespace Ops.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的HostLink的C-Mode实现形式，当前的类是通过以太网透传实现。地址支持携带站号信息，例如：s=2;D100。
/// </summary>
/// <remarks>
/// 暂时只支持的字数据的读写操作，不支持位的读写操作。
/// </remarks>
public sealed class OmronHostLinkCModeOverTcp : NetworkDeviceBase
{
	public byte UnitNumber { get; set; }

	public OmronHostLinkCModeOverTcp()
	{
		WordLength = 1;
		SleepTime = 20;
		ByteTransform = new ReverseWordTransform
		{
			DataFormat = DataFormat.CDAB,
			IsStringReverseByteWord = true,
		};
	}

	public OmronHostLinkCModeOverTcp(string ipAddress, int port)
		: this()
	{
		IpAddress = ipAddress;
		Port = port;
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		byte unitNumber = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		OperateResult<byte[]> operateResult = OmronHostLinkCMode.BuildReadCommand(address, length, isBit: false);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReadFromCoreServer(OmronHostLinkCMode.PackCommand(operateResult.Content, unitNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult<byte[]> operateResult3 = OmronHostLinkCMode.ResponseValidAnalysis(operateResult2.Content, isRead: true);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return OperateResult.Ok(operateResult3.Content);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		byte unitNumber = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var operateResult = OmronHostLinkCMode.BuildWriteWordCommand(address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		var operateResult2 = ReadFromCoreServer(OmronHostLinkCMode.PackCommand(operateResult.Content, unitNumber));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		var operateResult3 = OmronHostLinkCMode.ResponseValidAnalysis(operateResult2.Content, isRead: false);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var command = OmronHostLinkCMode.BuildReadCommand(address, length, isBit: false);
		if (!command.IsSuccess)
		{
			return command;
		}

		var read = await ReadFromCoreServerAsync(OmronHostLinkCMode.PackCommand(command.Content, station)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return OperateResult.Error<byte[]>(read);
		}

		var valid = OmronHostLinkCMode.ResponseValidAnalysis(read.Content, isRead: true);
		if (!valid.IsSuccess)
		{
			return OperateResult.Error<byte[]>(valid);
		}
		return OperateResult.Ok(valid.Content);
	}

	public override async Task<OperateResult> WriteAsync(string address, byte[] value)
	{
		byte station = (byte)OpsHelper.ExtractParameter(ref address, "s", UnitNumber);
		var command = OmronHostLinkCMode.BuildWriteWordCommand(address, value);
		if (!command.IsSuccess)
		{
			return command;
		}

		var read = await ReadFromCoreServerAsync(OmronHostLinkCMode.PackCommand(command.Content, station)).ConfigureAwait(false);
		if (!read.IsSuccess)
		{
			return read;
		}

		var valid = OmronHostLinkCMode.ResponseValidAnalysis(read.Content, isRead: false);
		if (!valid.IsSuccess)
		{
			return valid;
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 读取PLC的当前的型号信息
	/// </summary>
	/// <returns>型号</returns>
	public OperateResult<string> ReadPlcModel()
	{
		var operateResult = ReadFromCoreServer(OmronHostLinkCMode.PackCommand(Encoding.ASCII.GetBytes("MM"), UnitNumber));
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult);
		}

		int num = Convert.ToInt32(Encoding.ASCII.GetString(operateResult.Content, 5, 2), 16);
		if (num > 0)
		{
			return new OperateResult<string>((int)ErrorCode.UnknownError, "Unknown Error");
		}

		string @string = Encoding.ASCII.GetString(operateResult.Content, 7, 2);
		return OmronHostLinkCMode.GetModelText(@string);
	}

	public override string ToString()
	{
		return $"OmronHostLinkCModeOverTcp[{IpAddress}:{Port}]";
	}
}
