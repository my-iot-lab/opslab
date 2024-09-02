using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Ops.Communication.Core.Pipe;
using Ops.Communication.Extensions;
using Ops.Communication.Utils;

namespace Ops.Communication.Core.Net;

/// <summary>
/// 基于Udp的应答式通信类。
/// </summary>
public class NetworkUdpBase : NetworkBase
{
    private PipeSocket _pipeSocket;

	public virtual string IpAddress
	{
        get => _pipeSocket.IpAddress;
        set
        {
            _pipeSocket.IpAddress = value;
        }
    }

	public int Port 
	{
        get => _pipeSocket.Port;
        set
        {
            _pipeSocket.Port = value;
        }
    }

	public int ReceiveTimeout 
	{
        get => _pipeSocket.ReceiveTimeOut;
        set
        {
            _pipeSocket.ReceiveTimeOut = value;
        }
    }

	public string ConnectionId { get; set; }

	/// <summary>
	/// 获取或设置一次接收时的数据长度，默认2KB数据长度，特殊情况的时候需要调整。
	/// </summary>
	public int ReceiveCacheLength { get; set; } = 2048;

	/// <summary>
	/// 实例化一个默认的方法。
	/// </summary>
	public NetworkUdpBase()
	{
		_pipeSocket = new();
        ConnectionId = SoftBasic.GetUniqueStringByGuidAndRandom();
	}

	protected virtual byte[] PackCommandWithHeader(byte[] command)
	{
		return command;
	}

	protected virtual OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
	{
		return OperateResult.Ok(response);
	}

	public virtual OperateResult<byte[]> ReadFromCoreServer(byte[] send)
	{
		return ReadFromCoreServer(send, hasResponseData: true, usePackAndUnpack: true);
	}

    public OperateResult<byte[]> ReadFromCoreServer(IEnumerable<byte[]> send)
    {
        return NetSupport.ReadFromCoreServer(send, ReadFromCoreServer);
    }

    /// <summary>
    /// 核心的数据交互读取，发数据发送到通道上去，然后从通道上接收返回的数据
    /// </summary>
    /// <param name="send">完整的报文内容</param>
    /// <param name="hasResponseData">是否有等待的数据返回，默认为 true</param>
    /// <param name="usePackAndUnpack">是否需要对命令重新打包，在重写<see cref="PackCommandWithHeader(Byte[])" />方法后才会有影响</param>
    /// <returns>是否成功的结果对象</returns>
    public virtual OperateResult<byte[]> ReadFromCoreServer(byte[] send, bool hasResponseData, bool usePackAndUnpack)
	{
		byte[] array = usePackAndUnpack ? PackCommandWithHeader(send) : send;
		Logger?.LogDebug("{str0} Send: {str1}", ToString(), SoftBasic.ByteToHexString(array));

        _pipeSocket.PipeLockEnter();
		try
		{
			var iPEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
            OperateResult<Socket> availableSocketAsync = GetAvailableSocket(iPEndPoint);
            if (!availableSocketAsync.IsSuccess)
            {
                return OperateResult.Error<byte[]>(availableSocketAsync);
            }

            availableSocketAsync.Content.SendTo(array, array.Length, SocketFlags.None, iPEndPoint);
            if (ReceiveTimeout < 0)
            {
                return OperateResult.Ok(Array.Empty<byte>());
            }

            if (!hasResponseData)
            {
                return OperateResult.Ok(Array.Empty<byte>());
            }

            availableSocketAsync.Content.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveTimeout);
            EndPoint remoteEP = new IPEndPoint((iPEndPoint.AddressFamily == AddressFamily.InterNetworkV6) ? IPAddress.IPv6Any : IPAddress.Any, 0);
            byte[] array2 = new byte[ReceiveCacheLength];
            int length = availableSocketAsync.Content.ReceiveFrom(array2, ref remoteEP);
            byte[] array3 = array2.SelectBegin(length);
            _pipeSocket.IsSocketError = false;

            try
            {
                return usePackAndUnpack ? UnpackResponseContent(array, array3) : OperateResult.Ok(array3);
            }
            catch (Exception ex)
            {
                return new OperateResult<byte[]>((int)OpsErrorCode.UnpackResponseContentError, $"UnpackResponseContent failed: {ex.Message}");
            }
        }
		catch (SocketException ex)
		{
            _pipeSocket.ChangePorts();
            _pipeSocket.IsSocketError = true;
            return new OperateResult<byte[]>((int)OpsErrorCode.SocketReceiveException, ex.Message);
		}
		finally
		{
            _pipeSocket.PipeLockLeave();
        }
	}

	public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] value)
	{
		return await Task.Run(() => ReadFromCoreServer(value)).ConfigureAwait(false);
	}

    public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(IEnumerable<byte[]> send)
    {
        return await NetSupport.ReadFromCoreServerAsync(send, ReadFromCoreServerAsync).ConfigureAwait(false);
    }

    public IPStatus IpAddressPing()
	{
		using var ping = new Ping();
		return ping.Send(IpAddress).Status;
	}

    private OperateResult<Socket> GetAvailableSocket(IPEndPoint endPoint)
    {
        if (_pipeSocket.IsConnectitonError())
        {
            OperateResult operateResult;
            try
            {
                _pipeSocket.Socket?.Close();
                _pipeSocket.Socket = new(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                _pipeSocket.IsSocketError = false;
                operateResult = OperateResult.Ok();
            }
            catch (SocketException ex)
            {
                _pipeSocket.IsSocketError = true;
                operateResult = new OperateResult((int)OpsErrorCode.SocketException, ex.Message);
            }

            if (!operateResult.IsSuccess)
            {
                _pipeSocket.IsSocketError = true;
                return OperateResult.Error<Socket>(operateResult);
            }

            return OperateResult.Ok(_pipeSocket.Socket);
        }

        return OperateResult.Ok(_pipeSocket.Socket);
    }

    public override string ToString()
	{
		return $"NetworkUdpBase[{IpAddress}:{Port}]";
	}
}
