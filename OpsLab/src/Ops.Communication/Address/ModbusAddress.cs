using System.Text;

namespace Ops.Communication.Address;

/// <summary>
/// Modbus协议地址格式，可以携带站号，功能码，地址信息。<br />
/// 若是富地址，例如s=2;x=3;100，其中 s 表示站号，x 表示功能码，100 表示地址。对于 Coil，可以用 100.1 表示。
/// </summary>
public class ModbusAddress : DeviceAddressBase
{
	/// <summary>
	/// 获取或设置当前地址的站号信息
	/// </summary>
	public int Station { get; set; }

	/// <summary>
	/// 获取或设置当前地址携带的功能码
	/// </summary>
	public int Function { get; set; }

	/// <summary>
	/// 实例化一个默认的对象
	/// </summary>
	public ModbusAddress()
	{
		Station = -1;
		Function = -1;
		Address = 0;
	}

	/// <summary>
	/// 实例化一个对象，使用指定的地址初始化
	/// </summary>
	/// <param name="address">传入的地址信息，支持富地址，例如s=2;x=3;100</param>
	public ModbusAddress(string address)
	{
		Station = -1;
		Function = -1;
		Address = 0;
		Parse(address);
	}

	/// <summary>
	/// 实例化一个对象，使用指定的地址及功能码初始化
	/// </summary>
	/// <param name="address">传入的地址信息，支持富地址，例如s=2;x=3;100</param>
	/// <param name="function">默认的功能码信息</param>
	public ModbusAddress(string address, byte function)
	{
		Station = -1;
		Function = function;
		Address = 0;
		Parse(address);
	}

	/// <summary>
	/// 实例化一个对象，使用指定的地址，站号，功能码来初始化
	/// </summary>
	/// <param name="address">传入的地址信息，支持富地址，例如s=2;x=3;100</param>
	/// <param name="station">站号信息</param>
	/// <param name="function">默认的功能码信息</param>
	public ModbusAddress(string address, byte station, byte function)
	{
		Station = -1;
		Function = function;
		Station = station;
		Address = 0;
		Parse(address);
	}

	public override void Parse(string address)
	{
		if (address.IndexOf(';') < 0)
		{
			Address = ushort.Parse(address);
			return;
		}

		string[] array = address.Split(new char[1] { ';' });
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i][0] == 's' || array[i][0] == 'S')
			{
				Station = byte.Parse(array[i][2..]);
			}
			else if (array[i][0] == 'x' || array[i][0] == 'X')
			{
				Function = byte.Parse(array[i][2..]);
			}
			else
			{
				Address = ushort.Parse(array[i]);
			}
		}
	}

	/// <summary>
	/// 地址偏移指定的位置，返回一个新的地址对象
	/// </summary>
	/// <param name="value">数据值信息</param>
	/// <returns>新增后的地址信息</returns>
	public ModbusAddress AddressAdd(int value)
	{
		return new ModbusAddress
		{
			Station = Station,
			Function = Function,
			Address = (ushort)(Address + value)
		};
	}

	/// <summary>
	/// 地址偏移1，返回一个新的地址对象
	/// </summary>
	/// <returns>新增后的地址信息</returns>
	public ModbusAddress AddressAdd()
	{
		return AddressAdd(1);
	}

	public override string ToString()
	{
		var stringBuilder = new StringBuilder();
		if (Station >= 0)
		{
			stringBuilder.Append("s=" + Station + ";");
		}
		if (Function >= 1)
		{
			stringBuilder.Append("x=" + Function + ";");
		}
		stringBuilder.Append(Address.ToString());
		return stringBuilder.ToString();
	}
}
