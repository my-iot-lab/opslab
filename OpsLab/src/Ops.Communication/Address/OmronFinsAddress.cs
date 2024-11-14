using Ops.Communication.Core;
using Ops.Communication.Profinet.Omron;
using Ops.Communication.Extensions;

namespace Ops.Communication.Address;

/// <summary>
/// 欧姆龙的Fins协议的地址类对象
/// </summary>
public class OmronFinsAddress : DeviceAddressDataBase
{
	/// <summary>
	/// 进行位操作的指令
	/// </summary>
	public byte BitCode { get; set; }

	/// <summary>
	/// 进行字操作的指令
	/// </summary>
	public byte WordCode { get; set; }

	/// <summary>
	/// 从指定的地址信息解析成真正的设备地址信息
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="length">数据长度</param>
	public override void Parse(string address, ushort length)
	{
		OperateResult<OmronFinsAddress> operateResult = ParseFrom(address, length);
		if (operateResult.IsSuccess)
		{
			AddressStart = operateResult.Content.AddressStart;
			Length = operateResult.Content.Length;
			BitCode = operateResult.Content.BitCode;
			WordCode = operateResult.Content.WordCode;
		}
	}

	/// <summary>
	/// 从实际的欧姆龙的地址里面解析出地址对象。
	/// </summary>
	/// <param name="address">欧姆龙的地址数据信息</param>
	/// <returns>是否成功的结果对象</returns>
	public static OperateResult<OmronFinsAddress> ParseFrom(string address)
	{
		return ParseFrom(address, 0);
	}

	/// <summary>
	/// 从实际的欧姆龙的地址里面解析出地址对象。
	/// </summary>
	/// <param name="address">欧姆龙的地址数据信息</param>
	/// <param name="length">读取的数据长度</param>
	/// <returns>是否成功的结果对象</returns>
	public static OperateResult<OmronFinsAddress> ParseFrom(string address, ushort length)
	{
		var omronFinsAddress = new OmronFinsAddress();
		try
		{
			omronFinsAddress.Length = length;
			switch (address[0])
			{
				case 'D' or 'd':
					omronFinsAddress.BitCode = OmronFinsDataType.DM.BitCode;
					omronFinsAddress.WordCode = OmronFinsDataType.DM.WordCode;
					break;
				case 'C' or 'c':
					omronFinsAddress.BitCode = OmronFinsDataType.CIO.BitCode;
					omronFinsAddress.WordCode = OmronFinsDataType.CIO.WordCode;
					break;
				case 'W' or 'w':
					omronFinsAddress.BitCode = OmronFinsDataType.WR.BitCode;
					omronFinsAddress.WordCode = OmronFinsDataType.WR.WordCode;
					break;
				case 'H' or 'h':
					omronFinsAddress.BitCode = OmronFinsDataType.HR.BitCode;
					omronFinsAddress.WordCode = OmronFinsDataType.HR.WordCode;
					break;
				case 'A' or 'a':
					omronFinsAddress.BitCode = OmronFinsDataType.AR.BitCode;
					omronFinsAddress.WordCode = OmronFinsDataType.AR.WordCode;
					break;
				case 'E' or 'e':
					string[] array = address.SplitDot();
					int num = Convert.ToInt32(array[0][1..], 16);
					if (num < 16)
					{
						omronFinsAddress.BitCode = (byte)(32 + num);
						omronFinsAddress.WordCode = (byte)(160 + num);
					}
					else
					{
						omronFinsAddress.BitCode = (byte)(224 + num - 16);
						omronFinsAddress.WordCode = (byte)(96 + num - 16);
					}
					break;
				default:
					throw new Exception(ConnErrorCode.NotSupportedDataType.Desc());
			}

			if (address[0] == 'E' || address[0] == 'e')
			{
				string[] array2 = address.SplitDot();
				int num2 = ushort.Parse(array2[1]) * 16;
				if (array2.Length > 2)
				{
					num2 += ConnHelper.CalculateBitStartIndex(array2[2]);
				}
				omronFinsAddress.AddressStart = num2;
			}
			else
			{
				string[] array3 = address[1..].SplitDot();
				int num3 = ushort.Parse(array3[0]) * 16;
				if (array3.Length > 1)
				{
					num3 += ConnHelper.CalculateBitStartIndex(array3[1]);
				}
				omronFinsAddress.AddressStart = num3;
			}
		}
		catch (Exception ex)
		{
			return new OperateResult<OmronFinsAddress>(ex.Message);
		}

		return OperateResult.Ok(omronFinsAddress);
	}
}
