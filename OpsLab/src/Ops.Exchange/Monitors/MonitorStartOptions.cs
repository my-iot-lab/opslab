using Ops.Exchange.Handlers.Heartbeat;
using Ops.Exchange.Handlers.Notice;
using Ops.Exchange.Handlers.Reply;
using Ops.Exchange.Handlers.Switch;
using Ops.Exchange.Handlers.Underly;

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
    /// 监控到数据后触发的开关事件
    /// </summary>
    public Action<UnderlyEventData>? UnderlyDelegate { get; set; }

    /// <summary>
    /// 开关子任务循环事件间隔（单位：毫秒），默认50ms。
    /// </summary>
    public int SwitchPollingInterval { get; set; } = 50;

    /// <summary>
    /// 是否在 Heartbeat 触发信号读取数据时的异常记录日志，默认为 false。
    /// </summary>
    public bool IsLoggerHeartbeatError { get; set; } = false;

    /// <summary>
    /// 是否在 Notice 触发信号读取数据时的异常记录日志，默认为 true。
    /// </summary>
    public bool IsLoggerNoticeError { get; set; } = true;

    /// <summary>
    /// 是否在 Notice 触发信号读取数据时的异常记录日志，默认为 true。
    /// </summary>
    public bool IsLoggerTriggerError { get; set; } = true;

    /// <summary>
    /// 是否在 Underly 触发信号读取数据时的异常记录日志，默认为 true。
    /// </summary>
    public bool IsLoggerUnderlyError { get; set; } = true;
}
