﻿namespace Ops.Exchange.Address;

/// <summary>
/// PLC 地址
/// </summary>
public class PlcAddress
{
    public long Id { get; set; }

    // <summary>
    /// 地址的标签名。注意：标签名在每个 PLC 地址中(或是每个 DB 块中)必须是唯一的。
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// 变量地址 (字符串格式)。
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
    /// 地址描述。
    /// </summary>
    public string Desc { get; set; }

    /// <summary>
    /// 地址标识。
    /// </summary>
    public AddressFlag Flag { get; set; }

    /// <summary>
    /// 额外标志。
    /// </summary>
    public string[] ExtraFlag { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 若是触发信号地址，会随触发信号一起发送。
    /// </summary>
    public List<PlcAddress> NormalAddrs { get; set; } = new List<PlcAddress>(0);

    /// <summary>
    /// 两者是否相等
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsEqual(PlcAddress other)
    {
        return Tag.Equals(other.Tag, StringComparison.OrdinalIgnoreCase);
    }
}
