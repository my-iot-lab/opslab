using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Logging;
using Ops.Communication.Core;
using Ops.Communication.Extensions;

namespace Ops.Communication.Serial;

/// <summary>
/// 所有串行通信类的基类，提供了一些基础的服务，核心的通信实现
/// </summary>
public class SerialBase : IDisposable
{
	protected bool LogMsgFormatBinary = true;
	private bool disposedValue = false;
	private readonly SimpleHybirdLock hybirdLock;
	private int sleepTime = 20;
	private bool isClearCacheBeforeRead = false;
	private int connectErrorCount = 0;

	/// <summary>
	/// 串口交互的核心
	/// </summary>
	protected SerialPort m_ReadData = null;

	public ILogger Logger { get; set; }

	/// <summary>
	/// 获取或设置一个值，该值指示在串行通信中是否启用请求发送 (RTS) 信号。
	/// </summary>
	public bool RtsEnable
	{
		get
		{
			return m_ReadData.RtsEnable;
		}
		set
		{
			m_ReadData.RtsEnable = value;
		}
	}

	/// <summary>
	/// 接收数据的超时时间，默认5000ms
	/// </summary>
	public int ReceiveTimeout { get; set; } = 5000;

	/// <summary>
	/// 连续串口缓冲数据检测的间隔时间，默认20ms，该值越小，通信速度越快，但是越不稳定。
	/// </summary>
	public int SleepTime
	{
		get
		{
			return sleepTime;
		}
		set
		{
			if (value > 0)
			{
				sleepTime = value;
			}
		}
	}

	/// <summary>
	/// 是否在发送数据前清空缓冲数据，默认是false
	/// </summary>
	public bool IsClearCacheBeforeRead
	{
		get
		{
			return isClearCacheBeforeRead;
		}
		set
		{
			isClearCacheBeforeRead = value;
		}
	}

	/// <summary>
	/// 当前连接串口信息的端口号名称
	/// </summary>
	public string PortName { get; private set; }

	/// <summary>
	/// 当前连接串口信息的波特率
	/// </summary>
	public int BaudRate { get; private set; }

	/// <summary>
	/// 实例化一个无参的构造方法
	/// </summary>
	public SerialBase()
	{
		m_ReadData = new SerialPort();
		hybirdLock = new SimpleHybirdLock();
	}

	/// <summary>
	/// 初始化串口信息，9600波特率，8位数据位，1位停止位，无奇偶校验
	/// </summary>
	/// <param name="portName">端口号信息，例如"COM3"</param>
	public virtual void SerialPortInit(string portName)
	{
		SerialPortInit(portName, 9600);
	}

	/// <summary>
	/// 初始化串口信息，波特率，8位数据位，1位停止位，无奇偶校验
	/// </summary>
	/// <param name="portName">端口号信息，例如"COM3"</param>
	/// <param name="baudRate">波特率</param>
	public virtual void SerialPortInit(string portName, int baudRate)
	{
		SerialPortInit(portName, baudRate, 8, (StopBits)1, 0);
	}

	/// <summary>
	/// 初始化串口信息，波特率，数据位，停止位，奇偶校验需要全部自己来指定
	/// </summary>
	/// <param name="portName">端口号信息，例如"COM3"</param>
	/// <param name="baudRate">波特率</param>
	/// <param name="dataBits">数据位</param>
	/// <param name="stopBits">停止位</param>
	/// <param name="parity">奇偶校验</param>
	public virtual void SerialPortInit(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
	{
		if (!m_ReadData.IsOpen)
		{
			m_ReadData.PortName = portName;
			m_ReadData.BaudRate = baudRate;
			m_ReadData.DataBits = dataBits;
			m_ReadData.StopBits = stopBits;
			m_ReadData.Parity = parity;
			PortName = m_ReadData.PortName;
			BaudRate = m_ReadData.BaudRate;
		}
	}

	/// <summary>
	/// 打开一个新的串行端口连接<br />
	/// Open a new serial port connection
	/// </summary>
	public OperateResult Open()
	{
		try
		{
			if (!m_ReadData.IsOpen)
			{
				m_ReadData.Open();
				return InitializationOnOpen();
			}
			return OperateResult.Ok();
		}
		catch (Exception ex)
		{
			if (connectErrorCount < 100000000)
			{
				connectErrorCount++;
			}
			return new OperateResult(-connectErrorCount, ex.Message);
		}
	}

	/// <summary>
	/// 获取一个值，指示串口是否处于打开状态
	/// </summary>
	/// <returns>是或否</returns>
	public bool IsOpen()
	{
		return m_ReadData.IsOpen;
	}

	/// <summary>
	/// 关闭当前的串口连接
	/// </summary>
	public void Close()
	{
		if (m_ReadData.IsOpen)
		{
			ExtraOnClose();
			m_ReadData.Close();
		}
	}

	/// <summary>
	/// 将原始的字节数据发送到串口，然后从串口接收一条数据。
	/// </summary>
	/// <param name="send">发送的原始字节数据</param>
	/// <returns>带接收字节的结果对象</returns>
	public OperateResult<byte[]> ReadFromCoreServer(byte[] send)
	{
		return ReadFromCoreServer(send, hasResponseData: true);
	}

	protected virtual byte[] PackCommandWithHeader(byte[] command)
	{
		return command;
	}

	protected virtual OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
	{
		return OperateResult.Ok(response);
	}

	/// <summary>
	/// 将原始的字节数据发送到串口，然后从串口接收一条数据。
	/// </summary>
	/// <param name="send">发送的原始字节数据</param>
	/// <param name="hasResponseData">是否有数据相应，如果为true, 需要等待数据返回，如果为false, 不需要等待数据返回</param>
	/// <param name="usePackAndUnpack">是否需要对命令重新打包，在重写<see cref="PackCommandWithHeader(Byte[])" />方法后才会有影响</param>
	/// <returns>带接收字节的结果对象</returns>
	public OperateResult<byte[]> ReadFromCoreServer(byte[] send, bool hasResponseData, bool usePackAndUnpack = true)
	{
		byte[] array = (usePackAndUnpack ? PackCommandWithHeader(send) : send);
		Logger?.LogDebug($"{ToString()} Send: " + (LogMsgFormatBinary ? array.ToHexString(' ') : Encoding.ASCII.GetString(array)));

		hybirdLock.Enter();
		OperateResult operateResult = Open();
		if (!operateResult.IsSuccess)
		{
			hybirdLock.Leave();
			return OperateResult.Error<byte[]>(operateResult);
		}

		if (IsClearCacheBeforeRead)
		{
			ClearSerialCache();
		}

		OperateResult operateResult2 = SPSend(m_ReadData, array);
		if (!operateResult2.IsSuccess)
		{
			hybirdLock.Leave();
			return OperateResult.Error<byte[]>(operateResult2);
		}

		if (!hasResponseData)
		{
			hybirdLock.Leave();
			return OperateResult.Ok(new byte[0]);
		}

		OperateResult<byte[]> operateResult3 = SPReceived(m_ReadData, awaitData: true);
		hybirdLock.Leave();
		if (!operateResult3.IsSuccess)
		{
			return operateResult3;
		}

		Logger?.LogDebug($"{ToString()} Receive: {(LogMsgFormatBinary ? operateResult3.Content.ToHexString(' ') : Encoding.ASCII.GetString(operateResult3.Content))}");
		return usePackAndUnpack ? UnpackResponseContent(array, operateResult3.Content) : operateResult3;
	}

	/// <summary>
	/// 清除串口缓冲区的数据，并返回该数据，如果缓冲区没有数据，返回的字节数组长度为0
	/// </summary>
	/// <returns>是否操作成功的方法</returns>
	public OperateResult<byte[]> ClearSerialCache()
	{
		return SPReceived(m_ReadData, awaitData: false);
	}

	public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] value)
	{
		return await Task.Run(() => ReadFromCoreServer(value));
	}

	protected virtual OperateResult InitializationOnOpen()
	{
		return OperateResult.Ok();
	}

	protected virtual OperateResult ExtraOnClose()
	{
		return OperateResult.Ok();
	}

	/// <summary>
	/// 发送数据到串口去。
	/// </summary>
	/// <param name="serialPort">串口对象</param>
	/// <param name="data">字节数据</param>
	/// <returns>是否发送成功</returns>
	protected virtual OperateResult SPSend(SerialPort serialPort, byte[] data)
	{
		if (data != null && data.Length != 0)
		{
			try
			{
				serialPort.Write(data, 0, data.Length);
				return OperateResult.Ok();
			}
			catch (Exception ex)
			{
				if (connectErrorCount < 100000000)
				{
					connectErrorCount++;
				}
				return new OperateResult(-connectErrorCount, ex.Message);
			}
		}
		return OperateResult.Ok();
	}

	/// <summary>
	/// 从串口接收一串字节数据信息，直到没有数据为止，如果参数awaitData为false, 第一轮接收没有数据则返回
	/// </summary>
	/// <param name="serialPort">串口对象</param>
	/// <param name="awaitData">是否必须要等待数据返回</param>
	/// <returns>结果数据对象</returns>
	protected virtual OperateResult<byte[]> SPReceived(SerialPort serialPort, bool awaitData)
	{
		byte[] array = new byte[1024];
		using var memoryStream = new MemoryStream();
		DateTime now = DateTime.Now;
		while (true)
		{
			Thread.Sleep(sleepTime);
			try
			{
				if (serialPort.BytesToRead < 1)
				{
					if ((DateTime.Now - now).TotalMilliseconds > ReceiveTimeout)
					{
						if (connectErrorCount < 100000000)
						{
							connectErrorCount++;
						}
						return new OperateResult<byte[]>(-connectErrorCount, $"Time out: {ReceiveTimeout}");
					}

					if (memoryStream.Length > 0 || !awaitData)
					{
						break;
					}
					continue;
				}

				int count = serialPort.Read(array, 0, array.Length);
				memoryStream.Write(array, 0, count);
				continue;
			}
			catch (Exception ex)
			{
				if (connectErrorCount < 100000000)
				{
					connectErrorCount++;
				}
				return new OperateResult<byte[]>(-connectErrorCount, ex.Message);
			}
		}

		byte[] value = memoryStream.ToArray();
		connectErrorCount = 0;
		return OperateResult.Ok(value);
	}

	/// <summary>
	/// 释放当前的对象
	/// </summary>
	/// <param name="disposing">是否在</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				hybirdLock?.Dispose();
				((Component)(object)m_ReadData)?.Dispose();
			}
			disposedValue = true;
		}
	}

	/// <summary>
	/// 释放当前的对象
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public override string ToString()
	{
		return $"SerialBase[{m_ReadData.PortName},{m_ReadData.BaudRate},{m_ReadData.DataBits},{m_ReadData.StopBits},{m_ReadData.Parity}]";
	}
}
