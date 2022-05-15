namespace Ops.Exchange.Model;

/// <summary>
/// 设备地址。
/// Line + Station + Host + Port 构成唯一。西门子 PLC，一台伺服可能会挂载多个工站。
/// </summary>
public class DeviceSchema
{
    public long Id { get; set; }

    /// <summary>
    /// 线体编号
    /// </summary>
    public string? Line { get; set; }

    /// <summary>
    /// 线体名称
    /// </summary>
    public string? LineName { get; set; }

    /// <summary>
    /// 工站编号
    /// </summary>
    public string? Station { get; set; }

    /// <summary>
    /// 工站名称
    /// </summary>
    public string? StationName { get; set; }

    /// <summary>
    /// 设备主机（主机名称或IP地址）
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// 设备网络端口（如 Modbus TCP 为 102 等）
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 设备类型（如西门子1200、西门子1500、三菱等）
    /// </summary>
    public string? DeviceModel { get; set; }

    /// <summary>
    /// 两者是否相等
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsEqual(DeviceSchema other)
    {
        return Line == other.Line
            && Station.Equals(other.Station, StringComparison.OrdinalIgnoreCase)
            && Host == other.Host
            && Port == other.Port;
    }
}
