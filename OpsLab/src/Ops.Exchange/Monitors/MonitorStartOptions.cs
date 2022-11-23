using Ops.Exchange.Handlers.Heartbeat;
using Ops.Exchange.Handlers.Notice;
using Ops.Exchange.Handlers.Reply;
using Ops.Exchange.Handlers.Switch;

namespace Ops.Exchange.Monitors;

/// <summary>
/// 监控器启动参数设置选项
/// </summary>
public sealed class MonitorStartOptions
{
    /// <summary>
    /// 监控到数据后触发的心跳事件
    /// </summary>
    public Action<HeartbeatEventData>? HeartbeatDelegate { get; set; }

    /// <summary>
    /// 监控到数据后触发的响应事件
    /// </summary>
    public Action<ReplyEventData>? TriggerDelegate { get; set; }

    /// <summary>
    /// 监控到数据后触发的通知事件
    /// </summary>
    public Action<NoticeEventData>? NoticeDelegate { get; set; }

    /// <summary>
    /// 监控到数据后触发的开关事件
    /// </summary>
    public Action<SwitchEventData>? SwitchDelegate { get; set; }

    /// <summary>
    /// 开关子任务循环事件间隔（单位：毫秒），默认50ms。
    /// </summary>
    public int SwitchPollingInterval { get; set; } = 50;
}
