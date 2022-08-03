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
    /// 注：普通类型默认长度设置为 0，当为数组或字符串时，需指定长度。
    /// </summary>
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
    /// <para>注：对于部分数据，主数据表示主表中存储的，附加标识表示是额外增项。</para>
    /// </summary>
    public bool IsAdditional { get; set; }

    /// <summary>
    /// 变量是否为数组对象。
    /// </summary>
    /// <returns></returns>
    public bool IsArray()
    {
        return VarType != VariableType.String && Length > 0;
    }

    /// <summary>
    /// 获取相应的对象值，没有找到对象则为 default。
    /// 若对象类型不能转换，会抛出异常。
    /// <para>注：若原始数据为数组，指定类型为 string，会将数组转换为 string，值以逗号隔开。</para>
    /// </summary>
    /// <returns></returns>
    public T? GetValue<T>()
    {
        if (Value == null)
        {
            return default;
        }

        // 若不是数组
        if (!Value.GetType().IsArray)
        {
            if (typeof(T) == typeof(string))
            {
                object? obj = Value.ToString();
                return (T?)obj;
            }

            return (T?)Value;
        }

        if (typeof(T) == typeof(string))
        {
            object obj = VarType switch
            {
                VariableType.Bit => string.Join(",", (bool[])Value),
                VariableType.Byte => string.Join(",", (int[])Value),
                VariableType.Word or VariableType.DWord => string.Join(",", (uint[])Value),
                VariableType.DInt or VariableType.Int => string.Join(",", (int[])Value),
                VariableType.Real or VariableType.LReal => string.Join(",", (double[])Value),
                VariableType.String or VariableType.S7String or VariableType.S7WString => string.Join(",", (string[])Value),
                _ => throw new NotImplementedException(),
            };

            return (T)obj;
        }

        if (typeof(T).IsArray)
        {
            return (T?)Value;
        }

        return default;
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
        };
    }
}
