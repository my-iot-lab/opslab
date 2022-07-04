namespace Ops.Exchange.Model;

/// <summary>
/// 设备地址。
/// Line + Station。一台伺服可能会挂载多个工站，如西门子 PLC。
/// </summary>
public sealed class DeviceSchema : IEquatable<DeviceSchema>
{
    /// <summary>
    /// 线体编号
    /// </summary>
    public string Line { get; }

    /// <summary>
    /// 线体名称
    /// </summary>
    public string LineName { get; }

    /// <summary>
    /// 工站编号，每条产线工站编号唯一。
    /// </summary>
    public string Station { get; }

    /// <summary>
    /// 工站名称
    /// </summary>
    public string StationName { get; }

    /// <summary>
    /// 设备主机（IP地址）。
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// 设备网络端口，0 表示会按驱动默认端口设置。
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 设备驱动类型
    /// </summary>
    public DriverModel DriverModel { get; }

    public DeviceSchema(string line, string lineName, string station, string stationName, string host, DriverModel driverModel)
    {
        Line = line;
        LineName = lineName;
        Station = station;
        StationName = stationName;
        Host = host;
        DriverModel = driverModel;
    }

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
