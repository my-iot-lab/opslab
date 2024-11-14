using System.Text;
using Ops.Communication.Core;
using Ops.Communication.Serial;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的HostLink的C-Mode实现形式，地址支持携带站号信息，例如：s=2;D100。
/// </summary>
/// <remarks>
/// 暂时只支持的字数据的读写操作，不支持位的读写操作。
/// </remarks>
public sealed class OmronHostLinkCMode : SerialDeviceBase
{
	public byte UnitNumber { get; set; }

	public OmronHostLinkCMode()
	{
		WordLength = 1;
		ByteTransform = new ReverseWordTransform
		{
			DataFormat = DataFormat.CDAB,
			IsStringReverseByteWord = true,
		};
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		byte unitNumber = (byte)ConnHelper.ExtractParameter(ref address, "s", UnitNumber);
		OperateResult<byte[]> operateResult = BuildReadCommand(address, length, isBit: false);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackCommand(operateResult.Content, unitNumber));
		if (!operateResult2.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult2);
		}

		OperateResult<byte[]> operateResult3 = ResponseValidAnalysis(operateResult2.Content, isRead: true);
		if (!operateResult3.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult3);
		}
		return OperateResult.Ok(operateResult3.Content);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		byte unitNumber = (byte)ConnHelper.ExtractParameter(ref address, "s", UnitNumber);
		OperateResult<byte[]> operateResult = BuildWriteWordCommand(address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackCommand(operateResult.Content, unitNumber));
		if (!operateResult2.IsSuccess)
		{
			return operateResult2;
		}

		OperateResult<byte[]> operateResult3 = ResponseValidAnalysis(operateResult2.Content, isRead: false);
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}
		return OperateResult.Ok();
	}

	public OperateResult<string> ReadPlcModel()
	{
		OperateResult<byte[]> operateResult = ReadFromCoreServer(PackCommand(Encoding.ASCII.GetBytes("MM"), UnitNumber));
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<string>(operateResult);
		}

		int num = Convert.ToInt32(Encoding.ASCII.GetString(operateResult.Content, 5, 2), 16);
		if (num > 0)
		{
			return new OperateResult<string>((int)ConnErrorCode.UnknownError, "Unknown Error");
		}

		string @string = Encoding.ASCII.GetString(operateResult.Content, 7, 2);
		return GetModelText(@string);
	}

	public override string ToString()
	{
		return $"OmronHostLinkCMode[{PortName}:{BaudRate}]";
	}

	/// <summary>
	/// 解析欧姆龙的数据地址，参考来源是Omron手册第188页，比如D100， E1.100。
	/// </summary>
	/// <param name="address">数据地址</param>
	/// <param name="isBit">是否是位地址</param>
	/// <param name="isRead">是否读取</param>
	/// <returns>解析后的结果地址对象</returns>
	public static OperateResult<string, string> AnalysisAddress(string address, bool isBit, bool isRead)
	{
		var operateResult = new OperateResult<string, string>();
		try
		{
			switch (address[0])
			{
				case 'D' or 'd':
					operateResult.Content1 = isRead ? "RD" : "WD";
					break;
				case 'C' or 'c':
					operateResult.Content1 = isRead ? "RR" : "WR";
					break;
				case 'H' or 'h':
					operateResult.Content1 = isRead ? "RH" : "WH";
					break;
				case 'A' or 'a':
					operateResult.Content1 = isRead ? "RJ" : "WJ";
					break;
				case 'E' or 'e':
					string[] array = address.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
					int num = Convert.ToInt32(array[0][1..], 16);
					operateResult.Content1 = (isRead ? "RE" : "WE") + Encoding.ASCII.GetString(SoftBasic.BuildAsciiBytesFrom((byte)num));
					break;
				default:
					throw new Exception(ConnErrorCode.NotSupportedDataType.Desc());
			}

			if (address[0] == 'E' || address[0] == 'e')
			{
				string[] array2 = address.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
				if (!isBit)
				{
					operateResult.Content2 = ushort.Parse(array2[1]).ToString("D4");
				}
			}
			else if (!isBit)
			{
				operateResult.Content2 = ushort.Parse(address[1..]).ToString("D4");
			}
		}
		catch (Exception ex)
		{
			operateResult.ErrorCode = (int)ConnErrorCode.NotSupportedDataType;
            operateResult.Message = ex.Message;
			return operateResult;
		}

		operateResult.IsSuccess = true;
		return operateResult;
	}

	/// <summary>
	/// 根据读取的地址，长度，是否位读取创建Fins协议的核心报文。
	/// </summary>
	/// <param name="address">地址，具体格式请参照示例说明</param>
	/// <param name="length">读取的数据长度</param>
	/// <param name="isBit">是否使用位读取</param>
	/// <returns>带有成功标识的Fins核心报文</returns>
	public static OperateResult<byte[]> BuildReadCommand(string address, ushort length, bool isBit)
	{
		OperateResult<string, string> operateResult = AnalysisAddress(address, isBit, isRead: true);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		var stringBuilder = new StringBuilder();
		stringBuilder.Append(operateResult.Content1);
		stringBuilder.Append(operateResult.Content2);
		stringBuilder.Append(length.ToString("D4"));
		return OperateResult.Ok(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
	}

	/// <summary>
	/// 根据读取的地址，长度，是否位读取创建Fins协议的核心报文。
	/// </summary>
	/// <param name="address">地址，具体格式请参照示例说明</param>
	/// <param name="value">等待写入的数据</param>
	/// <returns>带有成功标识的Fins核心报文</returns>
	public static OperateResult<byte[]> BuildWriteWordCommand(string address, byte[] value)
	{
		OperateResult<string, string> operateResult = AnalysisAddress(address, isBit: false, isRead: false);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<byte[]>(operateResult);
		}

		var stringBuilder = new StringBuilder();
		stringBuilder.Append(operateResult.Content1);
		stringBuilder.Append(operateResult.Content2);
		for (int i = 0; i < value.Length / 2; i++)
		{
			stringBuilder.Append(BitConverter.ToUInt16(value, i * 2).ToString("X4"));
		}
		return OperateResult.Ok(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
	}

	/// <summary>
	/// 验证欧姆龙的Fins-TCP返回的数据是否正确的数据，如果正确的话，并返回所有的数据内容。
	/// </summary>
	/// <param name="response">来自欧姆龙返回的数据内容</param>
	/// <param name="isRead">是否读取</param>
	/// <returns>带有是否成功的结果对象</returns>
	public static OperateResult<byte[]> ResponseValidAnalysis(byte[] response, bool isRead)
	{
		if (response.Length >= 11)
		{
			int num = Convert.ToInt32(Encoding.ASCII.GetString(response, 5, 2), 16);
			byte[] array = null;
			if (response.Length > 11)
			{
				byte[] array2 = new byte[(response.Length - 11) / 2];
				for (int i = 0; i < array2.Length / 2; i++)
				{
					BitConverter.GetBytes(Convert.ToUInt16(Encoding.ASCII.GetString(response, 7 + 4 * i, 4), 16)).CopyTo(array2, i * 2);
				}
				array = array2;
			}

			if (num > 0)
			{
				return new OperateResult<byte[]>
				{
					ErrorCode = (int)ConnErrorCode.OmronReceiveDataError,
					Content = array
				};
			}

			return OperateResult.Ok(array);
		}

		return new OperateResult<byte[]>((int)ConnErrorCode.OmronReceiveDataError, ConnErrorCode.OmronReceiveDataError.Desc());
	}

	/// <summary>
	/// 将普通的指令打包成完整的指令
	/// </summary>
	/// <param name="cmd">fins指令</param>
	/// <param name="unitNumber">站号信息</param>
	/// <returns>完整的质量</returns>
	public static byte[] PackCommand(byte[] cmd, byte unitNumber)
	{
		byte[] array = new byte[7 + cmd.Length];
		array[0] = 64;
		array[1] = SoftBasic.BuildAsciiBytesFrom(unitNumber)[0];
		array[2] = SoftBasic.BuildAsciiBytesFrom(unitNumber)[1];
		array[^2] = 42;
		array[^1] = 13;
		cmd.CopyTo(array, 3);
		int num = array[0];
		for (int i = 1; i < array.Length - 4; i++)
		{
			num ^= array[i];
		}

		array[^4] = SoftBasic.BuildAsciiBytesFrom((byte)num)[0];
		array[^3] = SoftBasic.BuildAsciiBytesFrom((byte)num)[1];
		return array;
	}

	/// <summary>
	/// 获取model的字符串描述信息
	/// </summary>
	/// <param name="model">型号代码</param>
	/// <returns>是否解析成功</returns>
	public static OperateResult<string> GetModelText(string model)
	{
		return model switch
		{
			"30" => OperateResult.Ok("CS/CJ"),
			"01" => OperateResult.Ok("C250"),
			"02" => OperateResult.Ok("C500"),
			"03" => OperateResult.Ok("C120/C50"),
			"09" => OperateResult.Ok("C250F"),
			"0A" => OperateResult.Ok("C500F"),
			"0B" => OperateResult.Ok("C120F"),
			"0E" => OperateResult.Ok("C2000"),
			"10" => OperateResult.Ok("C1000H"),
			"11" => OperateResult.Ok("C2000H/CQM1/CPM1"),
			"12" => OperateResult.Ok("C20H/C28H/C40H, C200H, C200HS, C200HX/HG/HE (-ZE)"),
			"20" => OperateResult.Ok("CV500"),
			"21" => OperateResult.Ok("CV1000"),
			"22" => OperateResult.Ok("CV2000"),
			"40" => OperateResult.Ok("CVM1-CPU01-E"),
			"41" => OperateResult.Ok("CVM1-CPU11-E"),
			"42" => OperateResult.Ok("CVM1-CPU21-E"),
			_ => new OperateResult<string>($"Unknown model, model code: {model}"),
		};
	}
}
