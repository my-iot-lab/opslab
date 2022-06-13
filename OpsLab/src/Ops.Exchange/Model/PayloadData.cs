namespace Ops.Exchange.Model;

internal sealed class PayloadData
{
    /// <summary>
    /// 数据标签（唯一）
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 变量地址
    /// </summary>
    public string? Address { get; set; }

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
    /// 具体是数据
    /// </summary>
    public object? Value { get; set; }
}
