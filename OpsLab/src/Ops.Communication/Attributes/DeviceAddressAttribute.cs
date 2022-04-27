namespace Ops.Communication.Attributes;

/// <summary>
/// 应用于组件库读取的动态地址解析，具体用法为创建一个类，创建数据属性，如果这个属性需要绑定PLC的真实数据，就在属性的特性上应用本特性。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DeviceAddressAttribute : Attribute
{
	/// <summary>
	/// 设备的类型，如果指定了特殊的PLC，那么该地址就可以支持多种不同PLC
	/// </summary>
	public Type DeviceType { get; set; }

	/// <summary>
	/// 数据的地址信息，真实的设备的地址信息
	/// </summary>
	public string Address { get; }

	/// <summary>
	/// 读取的数据长度
	/// </summary>
	public int Length { get; }

	/// <summary>
	/// 实例化一个地址特性，指定地址信息，用于单变量的数据
	/// </summary>
	/// <param name="address">真实的地址信息</param>
	public DeviceAddressAttribute(string address)
	{
		Address = address;
		Length = -1;
		DeviceType = null;
	}

	/// <summary>
	/// 实例化一个地址特性，指定地址信息，用于单变量的数据，并指定设备类型
	/// </summary>
	/// <param name="address">真实的地址信息</param>
	/// <param name="deviceType">设备的地址信息</param>
	public DeviceAddressAttribute(string address, Type deviceType)
	{
		Address = address;
		Length = -1;
		DeviceType = deviceType;
	}

	/// <summary>
	/// 实例化一个地址特性，指定地址信息和数据长度，通常应用于数组的批量读取
	/// </summary>
	/// <param name="address">真实的地址信息</param>
	/// <param name="length">读取的数据长度</param>
	public DeviceAddressAttribute(string address, int length)
	{
		Address = address;
		Length = length;
		DeviceType = null;
	}

	/// <summary>
	/// 实例化一个地址特性，指定地址信息和数据长度，通常应用于数组的批量读取，并指定设备的类型，可用于不同种类的PLC
	/// </summary>
	/// <param name="address">真实的地址信息</param>
	/// <param name="length">读取的数据长度</param>
	/// <param name="deviceType">设备类型</param>
	public DeviceAddressAttribute(string address, int length, Type deviceType)
	{
		Address = address;
		Length = length;
		DeviceType = deviceType;
	}

	public override string ToString()
	{
		return $"DeviceAddressAttribute[{Address}:{Length}]";
	}
}
