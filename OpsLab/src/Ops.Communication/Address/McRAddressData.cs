using Ops.Communication.Ethernet.Profinet.Melsec;

namespace Ops.Communication.Address;

/// <summary>
/// 三菱R系列的PLC的地址表示对象
/// </summary>
public class McRAddressData : DeviceAddressDataBase
{
	/// <summary>
	/// 三菱的数据类型及地址信息
	/// </summary>
	public MelsecMcRDataType McDataType { get; set; }

	/// <summary>
	/// 实例化一个默认的对象
	/// </summary>
	public McRAddressData()
	{
		McDataType = MelsecMcRDataType.D;
	}

	/// <summary>
	/// 从指定的地址信息解析成真正的设备地址信息，默认是三菱的地址
	/// </summary>
	/// <param name="address">地址信息</param>
	/// <param name="length">数据长度</param>
	public override void Parse(string address, ushort length)
	{
		OperateResult<McRAddressData> operateResult = ParseMelsecRFrom(address, length);
		if (operateResult.IsSuccess)
		{
			base.AddressStart = operateResult.Content.AddressStart;
			base.Length = operateResult.Content.Length;
			McDataType = operateResult.Content.McDataType;
		}
	}

	/// <summary>
	/// 解析出三菱R系列的地址信息
	/// </summary>
	/// <param name="address">三菱的地址信息</param>
	/// <param name="length">读取的长度，对写入无效</param>
	/// <returns>解析结果</returns>
	public static OperateResult<McRAddressData> ParseMelsecRFrom(string address, ushort length)
	{
		OperateResult<MelsecMcRDataType, int> operateResult = MelsecMcRNet.AnalysisAddress(address);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.CreateFailedResult<McRAddressData>(operateResult);
		}

		return OperateResult.CreateSuccessResult(new McRAddressData
		{
			McDataType = operateResult.Content1,
			AddressStart = operateResult.Content2,
			Length = length
		});
	}
}
