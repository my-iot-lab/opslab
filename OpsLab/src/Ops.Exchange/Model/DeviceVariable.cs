using System.Text.Json.Serialization;

namespace Ops.Exchange.Model;

/// <summary>
/// 设备 PLC 地址变量
/// </summary>
public class DeviceVariable : IEquatable<DeviceVariable>
{
    /// <summary>
    /// 地址的标签名。注意：标签名在每个 PLC 地址中 (或是每个工站中) 必须是唯一的（在不同的DB块中不一定唯一）。
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// 地址 (字符串格式)。
    /// </summary>
    public string Address { get; }

    /// <summary>
    /// 变量长度。
    /// 注：普通类型默认长度设置为 0，当为数组或字符串时，需指定长度。
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// 地址变量类型
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VariableType VarType { get; }

    /// <summary>
    /// 地址描述。
    /// </summary>
    public string? Desc { get; }

    /// <summary>
    /// 变量类型标识。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VariableFlag Flag { get; }

    /// <summary>
    /// 额外标志。
    /// </summary>
    public string? ExtraFlag { get; set; }

    /// <summary>
    /// 变量监控轮询时间间隔，单位 ms。0 时会采用系统设置的轮询时间。
    /// </summary>
    public int PollingInterval { get; set; }

    /// <summary>
    /// 若本地址为 <see cref="VariableFlag.Trigger"/> 类型，该地址表示其负载数据的地址。
    /// </summary>
    public List<DeviceVariable> NormalVariables { get; set; } = new(0);

    public DeviceVariable(string tag, string address, int length, VariableType varType, string? desc, VariableFlag flag, int pollingInterval = 0)
    {
        Tag = tag;
        Address = address;
        Length = length;
        VarType = varType;
        Desc = desc;
        Flag = flag;
        PollingInterval = pollingInterval;
    }

    #region override

    public bool Equals(DeviceVariable? other)
    {
        return other != null &&
            Tag.Equals(other.Tag, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is DeviceVariable obj2 && Equals(obj2);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Tag);
    }

    #endregion
}
