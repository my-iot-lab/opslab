using Ops.Communication.Core;

namespace Ops.Communication.Ethernet.Modbus;

/// <summary>
/// Modbus设备的接口，用来表示Modbus相关的设备对象，<see cref="ModbusTcpNet" />, <see cref="ModbusRtu" />,
///  <see cref="ModbusAscii" />, <see cref="ModbusRtuOverTcp" />, <see cref="ModbusUdpNet" />均实现了该接口信息<br />
/// </summary>
public interface IModbus : IReadWriteDevice, IReadWriteNet
{
	bool AddressStartWithZero { get; set; }

	byte Station { get; set; }

	DataFormat DataFormat { get; set; }

	bool IsStringReverse { get; set; }

	/// <summary>
	/// 将当前的地址信息转换成Modbus格式的地址，如果转换失败，返回失败的消息。默认不进行任何的转换。
	/// </summary>
	/// <param name="address">传入的地址</param>
	/// <param name="modbusCode">Modbus的功能码</param>
	/// <returns>转换之后Modbus的地址</returns>
	OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode);
}
