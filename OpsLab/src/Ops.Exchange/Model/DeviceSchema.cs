namespace Ops.Exchange.Model;

/// <summary>
/// 设备地址。
/// Line + Station。西门子 PLC，一台伺服可能会挂载多个工站。
/// </summary>
public sealed class DeviceSchema : IEquatable<DeviceSchema>
{
    public long Id { get; set; }

    /// <summary>
    /// 线体编号
    /// </summary>
    public string Line { get; set; }

    /// <summary>
    /// 线体名称
    /// </summary>
    public string? LineName { get; set; }

    /// <summary>
    /// 工站编号
    /// </summary>
    public string Station { get; set; }

    /// <summary>
    /// 工站名称
    /// </summary>
    public string? StationName { get; set; }

    /// <summary>
    /// 设备主机（主机名称或IP地址）
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 设备网络端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 设备驱动类型
    /// </summary>
    public DriverModel DriverModel { get; set; }

    #region override

    public bool Equals(DeviceSchema? other)
    {
        return other != null &&
            Line == other.Line &&
            Station.Equals(other.Station, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is DeviceSchema obj2 && Equals(obj2);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Line, Station);
    }

    #endregion
}
