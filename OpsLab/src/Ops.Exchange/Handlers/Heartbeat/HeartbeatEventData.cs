using Ops.Exchange.Bus;
using Ops.Exchange.Model;

namespace Ops.Exchange.Handlers.Heartbeat;

/// <summary>
/// 心跳事件数据。
/// </summary>
public sealed class HeartbeatEventData : EventData
{
    /// <summary>
    /// 请求的数据上下文
    /// </summary>
    public PayloadContext Context { get; }

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

    public HeartbeatEventData(PayloadContext context, string tag, int state)
    {
        Context = context;
        Tag = tag;
        State = state;
    }
}
