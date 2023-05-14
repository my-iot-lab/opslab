using System.Net.Sockets;
using System.Net;

namespace Ops.Communication.Core.Pipe;

public sealed class PipeSocket : PipeBase, IDisposable
{
    private string _ipAddress = "127.0.0.1";
    private int[] _port = new int[1] { 2000 };
    private int _indexPort = -1;

    public string IpAddress
    {
        get
        {
            return _ipAddress;
        }
        set
        {
            _ipAddress = OpsHelper.GetIpAddressFromInput(value);
        }
    }

    public int Port
    {
        get
        {
            if (_port.Length == 1)
            {
                return _port[0];
            }

            int num = _indexPort;
            if (num < 0 || num >= _port.Length)
            {
                num = 0;
            }
            return _port[num];
        }
        set
        {
            if (_port.Length == 1)
            {
                _port[0] = value;
                return;
            }

            int num = _indexPort;
            if (num < 0 || num >= _port.Length)
            {
                num = 0;
            }
            _port[num] = value;
        }
    }

    public bool IsSocketError { get; set; }

    public Socket Socket { get; set; }

    public int ConnectTimeOut { get; set; } = 10_000;

    public int ReceiveTimeOut { get; set; } = 5_000;

    public int SleepTime { get; set; } = 0;

    public PipeSocket()
    {
    }

    public PipeSocket(string ipAddress, int port)
    {
        this._ipAddress = ipAddress;
        _port = new int[1] { port };
    }

    /// <summary>
    /// 连接错误，Socket 为空或存在错误。
    /// </summary>
    /// <returns></returns>
    public bool IsConnectitonError()
    {
        return IsSocketError || Socket == null;
    }

    public override void Dispose()
    {
        base.Dispose();
        Socket?.Close();
    }

    public void SetMultiPorts(int[] ports)
    {
        if (ports != null && ports.Length != 0)
        {
            _port = ports;
            _indexPort = -1;
        }
    }

    public IPEndPoint GetConnectIPEndPoint()
    {
        if (_port.Length == 1)
        {
            return new IPEndPoint(IPAddress.Parse(IpAddress), _port[0]);
        }

        ChangePorts();
        int port = _port[_indexPort];
        return new IPEndPoint(IPAddress.Parse(IpAddress), port);
    }

    public void ChangePorts()
    {
        if (_port.Length != 1)
        {
            if (_indexPort < _port.Length - 1)
            {
                _indexPort++;
            }
            else
            {
                _indexPort = 0;
            }
        }
    }

    public override string ToString()
    {
        return $"PipeSocket[{_ipAddress}:{Port}]";
    }
}
