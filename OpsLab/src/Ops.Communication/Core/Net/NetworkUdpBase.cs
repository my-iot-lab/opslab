using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Ops.Communication.Basic;
using Ops.Communication.Extensions;

namespace Ops.Communication.Core.Net;

/// <summary>
/// 基于Udp的应答式通信类。
/// </summary>
public class NetworkUdpBase : NetworkBase
{
	private readonly SimpleHybirdLock hybirdLock;

	private int connectErrorCount = 0;

	private string ipAddress = "127.0.0.1";

	public virtual string IpAddress
	{
		get
		{
			return ipAddress;
		}
		set
		{
			ipAddress = OpsHelper.GetIpAddressFromInput(value);
		}
	}

	public virtual int Port { get; set; }

	public int ReceiveTimeout { get; set; }

	public string ConnectionId { get; set; }

	/// <summary>
	/// 获取或设置一次接收时的数据长度，默认2KB数据长度，特殊情况的时候需要调整。
	/// </summary>
	public int ReceiveCacheLength { get; set; } = 2048;

	public IPEndPoint LocalBinding { get; set; }

	/// <summary>
	/// 实例化一个默认的方法。
	/// </summary>
	public NetworkUdpBase()
	{
		hybirdLock = new SimpleHybirdLock();
		ReceiveTimeout = 5000;
		ConnectionId = SoftBasic.GetUniqueStringByGuidAndRandom();
	}

	protected virtual byte[] PackCommandWithHeader(byte[] command)
	{
		return command;
	}

	protected virtual OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
	{
		return OperateResult.CreateSuccessResult(response);
	}

	public virtual OperateResult<byte[]> ReadFromCoreServer(byte[] send)
	{
		return ReadFromCoreServer(send, hasResponseData: true, usePackAndUnpack: true);
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
		byte[] array = (usePackAndUnpack ? PackCommandWithHeader(send) : send);
		Logger?.LogDebug($"{ToString()} Send: {SoftBasic.ByteToHexString(array)}");

		hybirdLock.Enter();
		try
		{
			var remoteEP = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			if (LocalBinding != null)
			{
				socket.Bind(LocalBinding);
			}
			socket.SendTo(array, array.Length, SocketFlags.None, remoteEP);
			if (ReceiveTimeout < 0)
			{
				hybirdLock.Leave();
				return OperateResult.CreateSuccessResult(new byte[0]);
			}
			if (!hasResponseData)
			{
				hybirdLock.Leave();
				return OperateResult.CreateSuccessResult(new byte[0]);
			}

			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveTimeout);
			var iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
			EndPoint remoteEP2 = iPEndPoint;
			byte[] array2 = new byte[ReceiveCacheLength];
			int length = socket.ReceiveFrom(array2, ref remoteEP2);
			byte[] array3 = array2.SelectBegin(length);
			hybirdLock.Leave();

			Logger?.LogDebug($"{ToString()} Receive: {SoftBasic.ByteToHexString(array3)}");
			connectErrorCount = 0;
			return usePackAndUnpack ? UnpackResponseContent(array, array3) : OperateResult.CreateSuccessResult(array3);
		}
		catch (Exception ex)
		{
			hybirdLock.Leave();
			if (connectErrorCount < 100000000)
			{
				connectErrorCount++;
			}
			return new OperateResult<byte[]>(-connectErrorCount, ex.Message);
		}
	}

	public async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] value)
	{
		return await Task.Run(() => ReadFromCoreServer(value));
	}

	public IPStatus IpAddressPing()
	{
		using var ping = new Ping();
		return ping.Send(IpAddress).Status;
	}

	public override string ToString()
	{
		return $"NetworkUdpBase[{IpAddress}:{Port}]";
	}
}
