using Ops.Communication.Profinet.Melsec;

namespace Ops.Communication.Address;

/// <summary>
/// 三菱的数据地址表示形式。
/// </summary>
public class McAddressData : DeviceAddressDataBase
{
	/// <summary>
	/// 三菱的数据类型及地址信息
	/// </summary>
	public MelsecMcDataType McDataType { get; set; }

	/// <summary>
	/// 实例化一个默认的对象
	/// </summary>
	public McAddressData()
	{
		McDataType = MelsecMcDataType.D;
	}

	/// <summary>
	/// 从指定的地址信息解析成真正的设备地址信息，默认是三菱的地址
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="length">数据长度</param>
	public override void Parse(string address, ushort length)
	{
		OperateResult<McAddressData> operateResult = ParseMelsecFrom(address, length);
		if (operateResult.IsSuccess)
		{
                AddressStart = operateResult.Content.AddressStart;
                Length = operateResult.Content.Length;
			McDataType = operateResult.Content.McDataType;
		}
	}

	public override string ToString()
	{
		return McDataType.AsciiCode.Replace("*", "") + Convert.ToString(AddressStart, McDataType.FromBase);
	}

	/// <summary>
	/// 从实际三菱的地址里面解析出我们需要的地址类型。
	/// </summary>
	/// <param name="address">三菱的地址数据信息</param>
	/// <param name="length">读取的数据长度</param>
	/// <returns>是否成功的结果对象</returns>
	public static OperateResult<McAddressData> ParseMelsecFrom(string address, ushort length)
	{
		var mcAddressData = new McAddressData
		{
			Length = length
		};

		try
		{
			switch (address[0])
			{
				case 'M' or 'm':
					mcAddressData.McDataType = MelsecMcDataType.M;
					mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.M.FromBase);
					break;
				case 'X' or 'x':
					mcAddressData.McDataType = MelsecMcDataType.X;
					address = address[1..];
					if (address.StartsWith("0"))
					{
						mcAddressData.AddressStart = Convert.ToInt32(address, 8);
					}
					else
					{
						mcAddressData.AddressStart = Convert.ToInt32(address, MelsecMcDataType.X.FromBase);
					}
					break;
				case 'Y' or 'y':
					mcAddressData.McDataType = MelsecMcDataType.Y;
					address = address[1..];
					if (address.StartsWith("0"))
					{
						mcAddressData.AddressStart = Convert.ToInt32(address, 8);
					}
					else
					{
						mcAddressData.AddressStart = Convert.ToInt32(address, MelsecMcDataType.Y.FromBase);
					}
					break;
				case 'D' or 'd':
					if (address[1] == 'X' || address[1] == 'x')
					{
						mcAddressData.McDataType = MelsecMcDataType.DX;
						address = address[2..];
						if (address.StartsWith("0"))
						{
							mcAddressData.AddressStart = Convert.ToInt32(address, 8);
						}
						else
						{
							mcAddressData.AddressStart = Convert.ToInt32(address, MelsecMcDataType.DX.FromBase);
						}
					}
					else if (address[1] == 'Y' || address[1] == 's')
					{
						mcAddressData.McDataType = MelsecMcDataType.DY;
						address = address[2..];
						if (address.StartsWith("0"))
						{
							mcAddressData.AddressStart = Convert.ToInt32(address, 8);
						}
						else
						{
							mcAddressData.AddressStart = Convert.ToInt32(address, MelsecMcDataType.DY.FromBase);
						}
					}
					else
					{
						mcAddressData.McDataType = MelsecMcDataType.D;
						mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.D.FromBase);
					}
					break;
				case 'W' or 'w':
					mcAddressData.McDataType = MelsecMcDataType.W;
					mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.W.FromBase);
					break;
				case 'L' or 'l':
					mcAddressData.McDataType = MelsecMcDataType.L;
					mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.L.FromBase);
					break;
				case 'F' or 'f':
					mcAddressData.McDataType = MelsecMcDataType.F;
					mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.F.FromBase);
					break;
				case 'V' or 'v':
					mcAddressData.McDataType = MelsecMcDataType.V;
					mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.V.FromBase);
					break;
				case 'B' or 'b':
					mcAddressData.McDataType = MelsecMcDataType.B;
					mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.B.FromBase);
					break;
				case 'R' or 'r':
					mcAddressData.McDataType = MelsecMcDataType.R;
					mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.R.FromBase);
					break;
				case 'S' or 's':
					if (address[1] == 'N' || address[1] == 'n')
					{
						mcAddressData.McDataType = MelsecMcDataType.SN;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SN.FromBase);
					}
					else if (address[1] == 'S' || address[1] == 's')
					{
						mcAddressData.McDataType = MelsecMcDataType.SS;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SS.FromBase);
					}
					else if (address[1] == 'C' || address[1] == 'c')
					{
						mcAddressData.McDataType = MelsecMcDataType.SC;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SC.FromBase);
					}
					else if (address[1] == 'M' || address[1] == 'm')
					{
						mcAddressData.McDataType = MelsecMcDataType.SM;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SM.FromBase);
					}
					else if (address[1] == 'D' || address[1] == 'd')
					{
						mcAddressData.McDataType = MelsecMcDataType.SD;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SD.FromBase);
					}
					else if (address[1] == 'B' || address[1] == 'b')
					{
						mcAddressData.McDataType = MelsecMcDataType.SB;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SB.FromBase);
					}
					else if (address[1] == 'W' || address[1] == 'w')
					{
						mcAddressData.McDataType = MelsecMcDataType.SW;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SW.FromBase);
					}
					else
					{
						mcAddressData.McDataType = MelsecMcDataType.S;
						mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.S.FromBase);
					}
					break;
				case 'Z' or 'z':
					if (address.StartsWith("ZR") || address.StartsWith("zr"))
					{
						mcAddressData.McDataType = MelsecMcDataType.ZR;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.ZR.FromBase);
					}
					else
					{
						mcAddressData.McDataType = MelsecMcDataType.Z;
						mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.Z.FromBase);
					}
					break;
				case 'T' or 't':
					if (address[1] == 'N' || address[1] == 'n')
					{
						mcAddressData.McDataType = MelsecMcDataType.TN;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.TN.FromBase);
						break;
					}
					if (address[1] == 'S' || address[1] == 's')
					{
						mcAddressData.McDataType = MelsecMcDataType.TS;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.TS.FromBase);
						break;
					}
					if (address[1] == 'C' || address[1] == 'c')
					{
						mcAddressData.McDataType = MelsecMcDataType.TC;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.TC.FromBase);
						break;
					}
					throw new Exception(ErrorCode.NotSupportedDataType.Desc());
				case 'C' or 'c':
					if (address[1] == 'N' || address[1] == 'n')
					{
						mcAddressData.McDataType = MelsecMcDataType.CN;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.CN.FromBase);
						break;
					}
					if (address[1] == 'S' || address[1] == 's')
					{
						mcAddressData.McDataType = MelsecMcDataType.CS;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.CS.FromBase);
						break;
					}
					if (address[1] == 'C' || address[1] == 'c')
					{
						mcAddressData.McDataType = MelsecMcDataType.CC;
						mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.CC.FromBase);
						break;
					}
					throw new Exception(ErrorCode.NotSupportedDataType.Desc());
				default:
					throw new Exception(ErrorCode.NotSupportedDataType.Desc());
			}
		}
		catch (Exception ex)
		{
			return new OperateResult<McAddressData>(ex.Message);
		}

		return OperateResult.Ok(mcAddressData);
	}
}
