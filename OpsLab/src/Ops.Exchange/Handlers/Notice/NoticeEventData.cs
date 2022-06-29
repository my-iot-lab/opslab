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

    public NoticeEventData(DeviceSchema schema, string tag)
    {
        Schema = schema;
        Tag = tag;
    }
}
