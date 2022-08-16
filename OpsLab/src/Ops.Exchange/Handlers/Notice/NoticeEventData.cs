using Ops.Exchange.Bus;
using Ops.Exchange.Model;

namespace Ops.Exchange.Handlers.Notice;

/// <summary>
/// 通知事件数据。
/// </summary>
public sealed class NoticeEventData : EventData
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
    /// 要通知的数据。
    /// </summary>
    public PayloadData Value { get; }

    /// <summary>
    /// 处理任务超时时间，毫秒
    /// </summary>
    public int HandleTimeout { get; set; }

    public NoticeEventData(string requestId, DeviceSchema schema, string tag, string? name, PayloadData value)
    {
        RequestId = requestId;
        Schema = schema;
        Tag = tag;
        Name = name;
        Value = value;
    }
}
