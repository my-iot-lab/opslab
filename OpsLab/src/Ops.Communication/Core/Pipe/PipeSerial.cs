using System.IO.Ports;

namespace Ops.Communication.Core.Pipe;

public sealed class PipeSerial : PipeBase, IDisposable
{
    private readonly SerialPort _serialPort;

    public bool RtsEnable
    {
        get
        {
            return _serialPort.RtsEnable;
        }
        set
        {
            _serialPort.RtsEnable = value;
        }
    }

    public PipeSerial()
    {
        _serialPort = new SerialPort();
    }

    public void SerialPortInni(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
    {
        if (!_serialPort.IsOpen)
        {
            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = dataBits;
            _serialPort.StopBits = stopBits;
            _serialPort.Parity = parity;
        }
    }

    public void SerialPortInni(Action<SerialPort> initi)
    {
        if (!_serialPort.IsOpen)
        {
            _serialPort.PortName = "COM1";
            initi(_serialPort);
        }
    }

    public OperateResult Open()
    {
        try
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
            return OperateResult.Ok();
        }
        catch (Exception ex)
        {
            return new OperateResult((int)OpsErrorCode.OpenSerialPortException, ex.Message);
        }
    }

    public bool IsOpen()
    {
        return _serialPort.IsOpen;
    }

    public OperateResult Close(Func<SerialPort, OperateResult> extraOnClose)
    {
        if (_serialPort.IsOpen)
        {
            if (extraOnClose != null)
            {
                OperateResult operateResult = extraOnClose(_serialPort);
                if (!operateResult.IsSuccess)
                {
                    return operateResult;
                }
            }

            try
            {
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                return new OperateResult((int)OpsErrorCode.CloseSerialPortException, ex.Message);
            }
        }

        return OperateResult.Ok();
    }

    public override void Dispose()
    {
        base.Dispose();
        _serialPort?.Dispose();
    }

    public SerialPort GetPipe()
    {
        return _serialPort;
    }

    public override string ToString()
    {
        return $"PipeSerial[{_serialPort.PortName},{_serialPort.BaudRate},{_serialPort.DataBits},{_serialPort.StopBits},{_serialPort.Parity}]";
    }
}
