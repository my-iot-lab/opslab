using Ops.Exchange.Bus;
using Ops.Exchange.Model;

namespace Ops.Exchange.Handlers.Notice;

/// <summary>
/// 通知事件数据。
/// </summary>
internal sealed class NoticeEventData : EventData
{
    /// <summary>
    /// 设备 Schema 基础信息。
    /// </summary>
    public DeviceSchema Schema { get; }

    /// <summary>
    /// 事件标签 Tag（唯一）
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// 要通知的数据。
    /// </summary>
    public PayloadData Value { get; }

    public NoticeEventData(DeviceSchema schema, string tag, PayloadData value)
    {
        Schema = schema;
        Tag = tag;
        Value = value;
    }
}
