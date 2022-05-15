namespace Ops.Exchange.Monitors;

/// <summary>
/// 监控器选项
/// </summary>
internal class MonitorOptions
{
    /// <summary>
    /// 监控事件间隔（单位：毫秒）
    /// </summary>
    public int Interval { get; set; }

    /// <summary>
    /// 任务监控超时时间（单位：毫秒）
    /// </summary>
    public int Timeout { get; set; }

    /// <summary>
    /// 任务处理超时时间（单位：毫秒）
    /// </summary>
    public int HandleTimeout { get; set; }

    /// <summary>
    /// 任务回调超时时间（单位：毫秒）
    /// </summary>
    public int CallbackTimeout { get; set; }
}
