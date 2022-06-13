namespace Ops.Exchange.Model;

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
    /// 设备包含的地址变量集合。
    /// </summary>
    public List<DeviceVariable> Variables { get; set; } = new List<DeviceVariable>(0);
}
