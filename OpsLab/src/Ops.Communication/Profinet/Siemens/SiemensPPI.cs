using Ops.Communication.Core;
using Ops.Communication.Serial;
using Ops.Communication.Utils;

namespace Ops.Communication.Profinet.Siemens;

/// <summary>
/// 西门子的PPI协议，适用于s7-200plc，注意，由于本类库的每次通讯分成2次操作，内部增加了一个同步锁，所以单次通信时间比较久，另外，地址支持携带站号，例如：s=2;M100
/// </summary>
/// <remarks>
/// 适用于西门子200的通信，非常感谢 合肥-加劲 的测试，让本类库圆满完成。注意：M地址范围有限 0-31地址<br />
/// 在本类的<see cref="SiemensPPIOverTcp" />实现类里，如果使用了Async的异步方法，没有增加同步锁，多线程调用可能会引发数据错乱的情况。
/// </remarks>
public sealed class SiemensPPI : SerialDeviceBase
{
	private byte station = 2;

	private readonly object communicationLock;

	/// <summary>
	/// 西门子PLC的站号信息<br />
	/// Siemens PLC station number information
	/// </summary>
	public byte Station
	{
		get
		{
			return station;
		}
		set
		{
			station = value;
		}
	}

	/// <summary>
	/// 实例化一个西门子的PPI协议对象<br />
	/// Instantiate a Siemens PPI protocol object
	/// </summary>
	public SiemensPPI()
	{
		base.ByteTransform = new ReverseBytesTransform();
		base.WordLength = 2;
		communicationLock = new object();
	}

	/// <summary>
	/// 从西门子的PLC中读取数据信息，地址为"M100","AI100","I0","Q0","V100","S100"等<br />
	/// Read data information from Siemens PLC with addresses "M100", "AI100", "I0", "Q0", "V100", "S100", etc.
	/// </summary>
	/// <param name="address">西门子的地址数据信息</param>
	/// <param name="length">数据长度</param>
	/// <returns>带返回结果的结果对象</returns>
	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = SiemensPPIOverTcp.BuildReadCommand(b, address, length, isBit: false);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}
		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 229)
			{
				return new OperateResult<byte[]>("PLC Receive Check Failed:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			}
			OperateResult<byte[]> operateResult3 = ReadFromCoreServer(SiemensPPIOverTcp.GetExecuteConfirm(b));
			if (!operateResult3.IsSuccess)
			{
				return operateResult3;
			}
			OperateResult operateResult4 = SiemensPPIOverTcp.CheckResponse(operateResult3.Content);
			if (!operateResult4.IsSuccess)
			{
				return OperateResult.Error<byte[]>(operateResult4);
			}
			byte[] array = new byte[length];
			if (operateResult3.Content[21] == byte.MaxValue && operateResult3.Content[22] == 4)
			{
				Array.Copy(operateResult3.Content, 25, array, 0, length);
			}
			return OperateResult.Ok(array);
		}
	}

	/// <summary>
	/// 从西门子的PLC中读取bool数据信息，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等<br />
	/// Read bool data information from Siemens PLC, the addresses are "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc.
	/// </summary>
	/// <param name="address">西门子的地址数据信息</param>
	/// <param name="length">数据长度</param>
	/// <returns>带返回结果的结果对象</returns>
	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = SiemensPPIOverTcp.BuildReadCommand(b, address, length, isBit: true);
		if (!operateResult.IsSuccess)
		{
			return OperateResult.Error<bool[]>(operateResult);
		}
		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult2);
			}
			if (operateResult2.Content[0] != 229)
			{
				return new OperateResult<bool[]>("PLC Receive Check Failed:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			}
			OperateResult<byte[]> operateResult3 = ReadFromCoreServer(SiemensPPIOverTcp.GetExecuteConfirm(b));
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult3);
			}
			OperateResult operateResult4 = SiemensPPIOverTcp.CheckResponse(operateResult3.Content);
			if (!operateResult4.IsSuccess)
			{
				return OperateResult.Error<bool[]>(operateResult4);
			}
			byte[] array = new byte[operateResult3.Content.Length - 27];
			if (operateResult3.Content[21] == byte.MaxValue && operateResult3.Content[22] == 3)
			{
				Array.Copy(operateResult3.Content, 25, array, 0, array.Length);
			}
			return OperateResult.Ok(SoftBasic.ByteToBoolArray(array, length));
		}
	}

	/// <summary>
	/// 将字节数据写入到西门子PLC中，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等<br />
	/// Write byte data to Siemens PLC with addresses "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc.
	/// </summary>
	/// <param name="address">西门子的地址数据信息</param>
	/// <param name="value">数据长度</param>
	/// <returns>带返回结果的结果对象</returns>
	public override OperateResult Write(string address, byte[] value)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = SiemensPPIOverTcp.BuildWriteCommand(b, address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}
		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 229)
			{
				return new OperateResult<byte[]>("PLC Receive Check Failed:" + operateResult2.Content[0]);
			}
			OperateResult<byte[]> operateResult3 = ReadFromCoreServer(SiemensPPIOverTcp.GetExecuteConfirm(b));
			if (!operateResult3.IsSuccess)
			{
				return operateResult3;
			}
			OperateResult operateResult4 = SiemensPPIOverTcp.CheckResponse(operateResult3.Content);
			if (!operateResult4.IsSuccess)
			{
				return operateResult4;
			}
			return OperateResult.Ok();
		}
	}

	/// <summary>
	/// 将bool数据写入到西门子PLC中，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等<br />
	/// Write the bool data to Siemens PLC with the addresses "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc.
	/// </summary>
	/// <param name="address">西门子的地址数据信息</param>
	/// <param name="value">数据长度</param>
	/// <returns>带返回结果的结果对象</returns>
	public override OperateResult Write(string address, bool[] value)
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref address, "s", Station);
		OperateResult<byte[]> operateResult = SiemensPPIOverTcp.BuildWriteCommand(b, address, value);
		if (!operateResult.IsSuccess)
		{
			return operateResult;
		}
		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 229)
			{
				return new OperateResult<byte[]>("PLC Receive Check Failed:" + operateResult2.Content[0]);
			}
			OperateResult<byte[]> operateResult3 = ReadFromCoreServer(SiemensPPIOverTcp.GetExecuteConfirm(b));
			if (!operateResult3.IsSuccess)
			{
				return operateResult3;
			}
			OperateResult operateResult4 = SiemensPPIOverTcp.CheckResponse(operateResult3.Content);
			if (!operateResult4.IsSuccess)
			{
				return operateResult4;
			}
			return OperateResult.Ok();
		}
	}

	/// <summary>
	/// 从西门子的PLC中读取byte数据信息，地址为"M100","AI100","I0","Q0","V100","S100"等，详细请参照API文档
	/// </summary>
	/// <param name="address">西门子的地址数据信息</param>
	/// <returns>带返回结果的结果对象</returns>
	public OperateResult<byte> ReadByte(string address)
	{
		return ByteTransformHelper.GetResultFromArray(Read(address, 1));
	}

	/// <summary>
	/// 向西门子的PLC中读取byte数据，地址为"M100","AI100","I0","Q0","V100","S100"等，详细请参照API文档
	/// </summary>
	/// <param name="address">西门子的地址数据信息</param>
	/// <param name="value">数据长度</param>
	/// <returns>带返回结果的结果对象</returns>
	public OperateResult Write(string address, byte value)
	{
		return Write(address, new byte[1] { value });
	}

	/// <summary>
	/// 启动西门子PLC为RUN模式，参数信息可以携带站号信息 "s=2;", 注意，分号是必须的。
	/// </summary>
	/// <param name="parameter">额外的参数信息，例如可以携带站号信息 "s=2;", 注意，分号是必须的。</param>
	/// <returns>是否启动成功</returns>
	public OperateResult Start(string parameter = "")
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref parameter, "s", Station);
		byte[] obj = new byte[39]
		{
			104, 33, 33, 104, 0, 0, 108, 50, 1, 0,
			0, 0, 0, 0, 20, 0, 0, 40, 0, 0,
			0, 0, 0, 0, 253, 0, 0, 9, 80, 95,
			80, 82, 79, 71, 82, 65, 77, 170, 22
		};
		obj[4] = b;
		byte[] send = obj;
		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult = ReadFromCoreServer(send);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			if (operateResult.Content[0] != 229)
			{
				return new OperateResult<byte[]>("PLC Receive Check Failed:" + operateResult.Content[0]);
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(SiemensPPIOverTcp.GetExecuteConfirm(b));
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			return OperateResult.Ok();
		}
	}

	/// <summary>
	/// 停止西门子PLC，切换为Stop模式，参数信息可以携带站号信息 "s=2;", 注意，分号是必须的。
	/// </summary>
	/// <param name="parameter">额外的参数信息，例如可以携带站号信息 "s=2;", 注意，分号是必须的。</param>
	/// <returns>是否停止成功</returns>
	public OperateResult Stop(string parameter = "")
	{
		byte b = (byte)OpsHelper.ExtractParameter(ref parameter, "s", Station);
		byte[] obj = new byte[35]
		{
			104, 29, 29, 104, 0, 0, 108, 50, 1, 0,
			0, 0, 0, 0, 16, 0, 0, 41, 0, 0,
			0, 0, 0, 9, 80, 95, 80, 82, 79, 71,
			82, 65, 77, 170, 22
		};
		obj[4] = b;
		byte[] send = obj;
		lock (communicationLock)
		{
			OperateResult<byte[]> operateResult = ReadFromCoreServer(send);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			if (operateResult.Content[0] != 229)
			{
				return new OperateResult<byte[]>("PLC Receive Check Failed:" + operateResult.Content[0]);
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(SiemensPPIOverTcp.GetExecuteConfirm(b));
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			return OperateResult.Ok();
		}
	}

	public override string ToString()
	{
		return $"SiemensPPI[{base.PortName}:{base.BaudRate}]";
	}
}
