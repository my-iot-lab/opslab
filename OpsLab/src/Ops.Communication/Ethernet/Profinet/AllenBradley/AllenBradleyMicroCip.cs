namespace Ops.Communication.Ethernet.Profinet.AllenBradley;

/// <summary>
/// AB PLC的cip通信实现类，适用Micro800系列控制系统。
/// </summary>
public class AllenBradleyMicroCip : AllenBradleyNet
{
	public AllenBradleyMicroCip()
	{
	}

	public AllenBradleyMicroCip(string ipAddress, int port = 44818)
		: base(ipAddress, port)
	{
	}

	protected override byte[] PackCommandService(byte[] portSlot, params byte[][] cips)
	{
		return AllenBradleyHelper.PackCleanCommandService(portSlot, cips);
	}

	public override string ToString()
	{
		return $"AllenBradleyMicroCip[{IpAddress}:{Port}]";
	}
}
