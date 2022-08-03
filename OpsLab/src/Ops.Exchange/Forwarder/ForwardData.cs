using Ops.Exchange.Model;

namespace Ops.Exchange.Forwarder;

/// <summary>
/// 要派发的数据
/// </summary>
public sealed class ForwardData
{
    /// <summary>
    /// 请求的 Id，可用于追踪数据。
    /// </summary>
    public string RequestId { get; }

    /// <summary>
    /// 设备 Schema 基础信息。
    /// </summary>
    public DeviceSchema Schema { get; }

    /// <summary>
    /// 事件标签 Tag（唯一）
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// 附加的数据
    /// </summary>
    public PayloadData[] Values { get; }

    public ForwardData(string requestId, DeviceSchema schema, string tag, PayloadData[] values)
    {
        RequestId = requestId;
        Schema = schema;
        Tag = tag;
        Values = values;
    }

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
