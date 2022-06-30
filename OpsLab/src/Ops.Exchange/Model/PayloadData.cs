namespace Ops.Exchange.Model;

/// <summary>
/// 负载的数据
/// </summary>
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
            ExtraFlag = variable.ExtraFlag,
        };
    }
}
