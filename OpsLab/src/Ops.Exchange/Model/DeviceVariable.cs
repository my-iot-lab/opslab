namespace Ops.Exchange.Model;

/// <summary>
/// 设备 PLC 地址变量
/// </summary>
public class DeviceVariable : IEquatable<DeviceVariable>
{
    /// <summary>
    /// 地址的标签名。注意：标签名在每个 PLC 地址中 (或是每个工站中) 必须是唯一的（在不同的DB块中不一定唯一）。
    /// </summary>
    [NotNull]
    public string? Tag { get; set; }

    /// <summary>
    /// 地址 (字符串格式)。
    /// </summary>
    [NotNull]
    public string? Address { get; set; }

    /// <summary>
    /// 变量长度。
    /// 注：普通类型默认长度设置为 0，当为数组或字符串时，需指定长度。
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 地址变量类型
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VariableType VarType { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 变量描述。
    /// </summary>
    public string? Desc { get; }

    /// <summary>
    /// 变量类型标识。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VariableFlag Flag { get; set; }

    /// <summary>
    /// 额外标志。
    /// </summary>
    public string? ExtraFlag { get; set; }

    /// <summary>
    /// 变量监控轮询时间间隔，单位 ms。0 时会采用系统设置的轮询时间。
    /// </summary>
    public int PollingInterval { get; set; }

    /// <summary>
    /// 是否是附加数据。
    /// <para>注：对于部分数据，主数据表示主表中存储的，附加标识表示是额外增项。</para>
    /// </summary>
    public bool IsAdditional { get; set; }

    /// <summary>
    /// 若本地址为 <see cref="VariableFlag.Trigger"/> 类型，该地址表示其负载数据的地址。
    /// </summary>
    public List<DeviceVariable> NormalVariables { get; set; } = new(0);

    public DeviceVariable()
    {

    }

    public DeviceVariable(string tag, string address, int length, VariableType varType, string? name, string? desc, VariableFlag flag, int pollingInterval = 0)
    {
        Tag = tag;
        Address = address;
        Length = length;
        VarType = varType;
        Name = name;
        Desc = desc;
        Flag = flag;
        PollingInterval = pollingInterval;
    }

    /// <summary>
    /// 变量是否为数组对象。
    /// 当值不为 String 类型（包含 S7String 和 S7WString）且设定的长度大于 0 时，判定为数组。
    /// </summary>
    /// <returns></returns>
    public bool IsArray()
    {
        return VarType != VariableType.String
                && VarType != VariableType.S7String
                && VarType != VariableType.S7WString
                && Length > 0;
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
