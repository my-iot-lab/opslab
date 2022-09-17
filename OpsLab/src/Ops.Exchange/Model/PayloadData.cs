namespace Ops.Exchange.Model;

/// <summary>
/// 负载的数据
/// </summary>
public sealed class PayloadData
{
    /// <summary>
    /// 数据标签（唯一）
    /// </summary>
    [NotNull]
    public string? Tag { get; set; }

    /// <summary>
    /// 变量地址
    /// </summary>
    [NotNull]
    public string? Address { get; set; }

    /// <summary>
    /// 变量长度。
    /// </summary>
    /// <remarks>普通类型默认长度设置为 0，当为数组或字符串时，需指定长度。</remarks>
    public int Length { get; set; }

    /// <summary>
    /// 地址变量类型
    /// </summary>
    public VariableType VarType { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 额外标志。
    /// </summary>
    public string? ExtraFlag { get; set; }

    /// <summary>
    /// 具体的数据，可能是数组。
    /// </summary>
    [NotNull]
    public object? Value { get; set; }

    /// <summary>
    /// 是否是附加数据。
    /// </summary>
    /// <remarks>可用于部分数据（如出站/存档），可用于主数据表示在主表中存储，附加数据表示是额外的、可动态变化的增项（如过程数据）。</remarks>
    public bool IsAdditional { get; set; }

    /// <summary>
    /// 程序配方号。
    /// </summary>
    /// <remarks>与 PLC 配方对应，可用于表示该地址变量在哪些配方中有效；空表示适用于所有配方。</remarks>
    public int[] Formulas { get; set; } = Array.Empty<int>();

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

    /// <summary>
    /// 将 DeviceVariable 转换为 PayloadData 对象
    /// </summary>
    /// <param name="variable">被转换的对象</param>
    /// <param name="value">具体值</param>
    /// <returns></returns>
    public static PayloadData From(DeviceVariable variable, object value)
    {
        var data = From(variable);
        data.Value = value;
        return data;
    }

    /// <summary>
    /// 将 DeviceVariable 转换为 PayloadData 对象
    /// </summary>
    /// <param name="variable">被转换的对象</param>
    /// <returns></returns>
    public static PayloadData From(DeviceVariable variable)
    {
        return new PayloadData
        {
            Tag = variable.Tag,
            Address = variable.Address,
            Length = variable.Length,
            VarType = variable.VarType,
            Name = variable.Name,
            ExtraFlag = variable.ExtraFlag,
            IsAdditional = variable.IsAdditional,
            Formulas = variable.Formulas,
        };
    }
}
