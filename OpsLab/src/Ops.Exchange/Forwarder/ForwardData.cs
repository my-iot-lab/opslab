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
    /// 事件名称
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// 附加的数据
    /// </summary>
    public PayloadData[] Values { get; }

    public ForwardData(string requestId, DeviceSchema schema, string tag, string? name, PayloadData[] values)
    {
        RequestId = requestId;
        Schema = schema;
        Tag = tag;
        Name = name;
        Values = values;
    }
}
