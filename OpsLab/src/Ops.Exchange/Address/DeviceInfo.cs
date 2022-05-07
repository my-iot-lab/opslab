namespace Ops.Exchange.Address;

/// <summary>
/// 设备信息
/// </summary>
public class DeviceInfo
{
    public long Id { get; set; }

    /// <summary>
    /// 设备 Schema 基础信息。
    /// </summary>
    public DeviceSchema Schema { get; set; }

    /// <summary>
    /// 设备包含的地址集合。
    /// </summary>
    public List<PlcAddress> Addrs { get; set; } = new List<PlcAddress>(0);
}
