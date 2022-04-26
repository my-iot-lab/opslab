namespace Ops.Communication.Address;

/// <summary>
/// 所有设备通信类的地址基础类
/// </summary>
public class DeviceAddressBase
{
	/// <summary>
	/// 获取或设置起始地址
	/// </summary>
	public ushort Address { get; set; }

	/// <summary>
	/// 解析字符串的地址
	/// </summary>
	/// <param name="address">地址信息</param>
	public virtual void Parse(string address)
	{
		Address = ushort.Parse(address);
	}

	public override string ToString()
	{
		return Address.ToString();
	}
}
