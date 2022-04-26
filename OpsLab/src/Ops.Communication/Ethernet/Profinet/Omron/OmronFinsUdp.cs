using System.Text;
using Ops.Communication.Core;
using Ops.Communication.Core.Net;

namespace Ops.Communication.Ethernet.Profinet.Omron;

/// <summary>
/// 欧姆龙的Udp协议的实现类，地址类型和Fins-TCP一致，无连接的实现，可靠性不如<see cref="OmronFinsNet" />。
/// </summary>
/// <remarks>
/// </remarks>
public class OmronFinsUdp : NetworkUdpDeviceBase
{
	public override string IpAddress
	{
		get
		{
			return base.IpAddress;
		}
		set
		{
			base.IpAddress = value;
			DA1 = Convert.ToByte(base.IpAddress[(base.IpAddress.LastIndexOf(".") + 1)..]);
		}
	}

	public byte ICF { get; set; } = 128;

	public byte RSV { get; private set; } = 0;

	public byte GCT { get; set; } = 2;

	public byte DNA { get; set; } = 0;

	public byte DA1 { get; set; } = 19;

	public byte DA2 { get; set; } = 0;

	public byte SNA { get; set; } = 0;

	public byte SA1 { get; set; } = 13;

	public byte SA2 { get; set; }

	public byte SID { get; set; } = 0;

	public int ReadSplits { get; set; } = 500;

	public OmronFinsUdp(string ipAddress, int port)
		: this()
	{
		IpAddress = ipAddress;
		Port = port;
	}

	public OmronFinsUdp()
	{
		WordLength = 1;
		ByteTransform = new ReverseWordTransform
		{
			DataFormat = DataFormat.CDAB,
			IsStringReverseByteWord = true,
		};
	}

	private byte[] PackCommand(byte[] cmd)
	{
		byte[] array = new byte[10 + cmd.Length];
		array[0] = ICF;
		array[1] = RSV;
		array[2] = GCT;
		array[3] = DNA;
		array[4] = DA1;
		array[5] = DA2;
		array[6] = SNA;
		array[7] = SA1;
		array[8] = SA2;
		array[9] = SID;
		cmd.CopyTo(array, 10);
		return array;
	}

	protected override byte[] PackCommandWithHeader(byte[] command)
	{
		return PackCommand(command);
	}

	protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
	{
		return OmronFinsNetHelper.UdpResponseValidAnalysis(response);
	}

	public override OperateResult<byte[]> Read(string address, ushort length)
	{
		return OmronFinsNetHelper.Read(this, address, length, ReadSplits);
	}

	public override OperateResult Write(string address, byte[] value)
	{
		return OmronFinsNetHelper.Write(this, address, value);
	}

	public override OperateResult<string> ReadString(string address, ushort length)
	{
		return base.ReadString(address, length, Encoding.UTF8);
	}

	public override OperateResult Write(string address, string value)
	{
		return base.Write(address, value, Encoding.UTF8);
	}

	public override OperateResult<bool[]> ReadBool(string address, ushort length)
	{
		return OmronFinsNetHelper.ReadBool(this, address, length, ReadSplits);
	}

	public override OperateResult Write(string address, bool[] values)
	{
		return OmronFinsNetHelper.Write(this, address, values);
	}

	public OperateResult Run()
	{
		return OmronFinsNetHelper.Run(this);
	}

	public OperateResult Stop()
	{
		return OmronFinsNetHelper.Stop(this);
	}

	public OperateResult<OmronCpuUnitData> ReadCpuUnitData()
	{
		return OmronFinsNetHelper.ReadCpuUnitData(this);
	}

	public OperateResult<OmronCpuUnitStatus> ReadCpuUnitStatus()
	{
		return OmronFinsNetHelper.ReadCpuUnitStatus(this);
	}

	public override string ToString()
	{
		return $"OmronFinsUdp[{IpAddress}:{Port}]";
	}
}
