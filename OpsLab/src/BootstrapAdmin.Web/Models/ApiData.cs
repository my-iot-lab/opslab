namespace BootstrapAdmin.Web.Models;

/// <summary>
/// 请求数据
/// </summary>
public sealed class ApiData
{
    /// <summary>
    /// 请求的 Id，可用于追踪数据。
    /// </summary>
    public string RequestId { get; set; }

    /// <summary>
    /// 设备 Schema 基础信息。
    /// </summary>
    public DeviceSchema Schema { get; set; }

    /// <summary>
    /// 事件标签 Tag（唯一）
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// 请求的数据
    /// </summary>
    public PayloadData[] Values { get; set; }
}

public sealed class DeviceSchema
{
    /// <summary>
    /// 线体编号
    /// </summary>
    public string Line { get; set; }

    /// <summary>
    /// 线体名称
    /// </summary>
    public string LineName { get; set; }

    /// <summary>
    /// 工站编号，每条产线工站编号唯一。
    /// </summary>
    public string Station { get; set; }

    /// <summary>
    /// 工站名称
    /// </summary>
    public string StationName { get; set; }

    /// <summary>
    /// 设备主机（IP地址）。
    /// </summary>
    public string Host { get; set; }
}

public sealed class PayloadData
{
    /// <summary>
    /// 数据标签（唯一）
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// 变量地址
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// 变量长度。
    /// 注：普通类型默认长度设置为 0，当为数组或字符串时，需指定长度。
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 地址变量类型
    /// </summary>
    public VariableType VarType { get; set; }

    /// <summary>
    /// 额外标志。
    /// </summary>
    public string? ExtraFlag { get; set; }

    /// <summary>
    /// 具体的数据，可能是数组。
    /// </summary>
    public object Value { get; set; }
}

/// <summary>
/// 设备地址变量类型
/// </summary>
public enum VariableType
{
    /// <summary>
    /// bool
    /// </summary>
    Bit,

    /// <summary>
    /// byte (8 bits)
    /// </summary>
    Byte,

    /// <summary>
    /// uint16 (16 bits)
    /// </summary>
    Word,

    /// <summary>
    /// uint32 (32 bits)
    /// </summary>
    DWord,

    /// <summary>
    /// int16 (16 bits)
    /// </summary>
    Int,

    /// <summary>
    /// int32 (32 bits)
    /// </summary>
    DInt,

    /// <summary>
    /// float (32 bits)
    /// </summary>
    Real,

    /// <summary>
    /// double (64 bits)
    /// </summary>
    LReal,

    /// <summary>
    /// string
    /// </summary>
    String,

    /// <summary>
    /// 西门子 S7String
    /// </summary>
    S7String,

    /// <summary>
    /// 西门子 S7WString
    /// </summary>
    S7WString,
}