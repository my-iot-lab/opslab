using Ops.Exchange.Bus;
using Ops.Exchange.Model;

namespace Ops.Exchange.Handlers.Heartbeat;

/// <summary>
/// 心跳事件数据。
/// </summary>
internal class HeartbeatEventData : EventData
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
    /// 任务处理标识。
    /// 0 --> 初始状态;
    /// 1 --> 已就绪，待被处理;
    /// 2 ~ N --> 其他状态。
    /// <para>只有为 1 时才能触发数据推送到下游处理。</para>
    /// </summary>
    public int State { get; }

    public HeartbeatEventData(DeviceSchema schema, string tag, int state)
    {
        Schema = schema;
        Tag = tag;
        State = state;
    }
}
