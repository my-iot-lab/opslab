using System.Text.Json.Serialization;

namespace BootstrapAdmin.Web.Core.Models;

/// <summary>
/// 请求数据
/// </summary>
public sealed class ApiData
{
    /// <summary>
    /// 请求的 Id，可用于追踪数据。
    /// </summary>
    [NotNull]
    public string? RequestId { get; set; }

    /// <summary>
    /// 设备 Schema 基础信息。
    /// </summary>
    [NotNull]
    public DeviceSchema? Schema { get; set; }

    /// <summary>
    /// 事件标签 Tag（唯一）
    /// </summary>
    [NotNull]
    public string? Tag { get; set; }

    /// <summary>
    /// 请求的数据
    /// </summary>
    [NotNull]
    public PayloadData[]? Values { get; set; }

    /// <summary>
    /// 获取相应的对象值，没有则为 null。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tag"></param>
    /// <returns></returns>
    public (bool ok, T? value) TryGetValue<T>(string tag)
    {
        try
        {
            var v = GetValue<T>(tag);
            return (true, v);
        }
        catch
        {
            return (false, default);
        }
    }

    /// <summary>
    /// 获取相应的对象值，没有找到对象则为 default。
    /// 若对象类型不能转换，会抛出异常。
    /// <para>注：若原始数据为数组，指定类型为 string，会将数组转换为 string，值以逗号隔开。</para>
    /// </summary>
    /// <param name="tag">tag</param>
    /// <returns></returns>
    public T? GetValue<T>(string tag)
    {
        var data = Values.FirstOrDefault(s => string.Equals(s.Tag, tag, StringComparison.OrdinalIgnoreCase));
        if (data == null || data.Value == null)
        {
            return default;
        }

        return data.GetValue<T>();
    }

    /// <summary>
    /// 是否存在相应的标签
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public bool Exists(string tag)
    {
        return Values.Any(s => string.Equals(s.Tag, tag, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class DeviceSchema
{
    /// <summary>
    /// 线体编号
    /// </summary>
    [NotNull]
    public string? Line { get; set; }

    /// <summary>
    /// 线体名称
    /// </summary>
    [NotNull]
    public string? LineName { get; set; }

    /// <summary>
    /// 工站编号，每条产线工站编号唯一。
    /// </summary>
    [NotNull]
    public string? Station { get; set; }

    /// <summary>
    /// 工站名称
    /// </summary>
    [NotNull]
    public string? StationName { get; set; }

    /// <summary>
    /// 设备主机（IP地址）。
    /// </summary>
    [NotNull]
    public string? Host { get; set; }
}

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
    [JsonConverter(typeof(JsonStringEnumConverter))]
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