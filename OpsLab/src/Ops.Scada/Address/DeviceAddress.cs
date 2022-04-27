namespace Ops.Scada.Address;

/// <summary>
/// 设备（如 PLC）地址
/// </summary>
public class DeviceAddress
{
    // <summary>
    /// 地址的标签名。注意：标签名在每个 PLC 地址中(或是每个 DB 块中)必须是唯一的。
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// 变量地址 (字符串格式)
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
    public AddrVarType VarType { get; set; }

    /// <summary>
    /// 地址描述
    /// </summary>
    public string Desc { get; set; } = string.Empty;

    /// <summary>
    /// 额外标志
    /// </summary>
    public string[] Flag { get; set; } = Array.Empty<string>();
}
