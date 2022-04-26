namespace Ops.Communication.Ethernet.Modbus;

/// <summary>
/// Modbus-Ascii通讯协议的类库，基于rtu类库完善过来，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式，详细见备注说明
/// </summary>
/// <remarks>
/// 本客户端支持的标准的modbus协议，Modbus-Tcp及Modbus-Udp内置的消息号会进行自增，地址支持富文本格式，具体参考示例代码。<br />
/// 读取线圈，输入线圈，寄存器，输入寄存器的方法中的读取长度对商业授权用户不限制，内部自动切割读取，结果合并。
/// </remarks>
public class ModbusAscii : ModbusRtu
{
	/// <summary>
	/// 实例化一个Modbus-ascii协议的客户端对象。
	/// </summary>
	public ModbusAscii()
	{
		LogMsgFormatBinary = false;
	}

	public ModbusAscii(byte station = 1)
		: base(station)
	{
		LogMsgFormatBinary = false;
	}

	protected override byte[] PackCommandWithHeader(byte[] command)
	{
		return ModbusInfo.TransModbusCoreToAsciiPackCommand(command);
	}

	protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
	{
		var operateResult = ModbusInfo.TransAsciiPackCommandToCore(response);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}

		if (send[1] + 128 == operateResult.Content[1])
		{
			return new OperateResult<byte[]>(operateResult.Content[2], ModbusInfo.GetDescriptionByErrorCode(operateResult.Content[2]));
		}

		return ModbusInfo.ExtractActualData(operateResult.Content);
	}

	public override string ToString()
	{
		return $"ModbusAscii[{base.PortName}:{base.BaudRate}]";
	}
}
