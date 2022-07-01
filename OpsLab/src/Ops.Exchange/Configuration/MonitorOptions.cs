namespace Ops.Exchange.Configuration;

/// <summary>
/// 监控器选项
/// </summary>
public class MonitorOptions
{
    /// <summary>
    /// 默认轮询监听时间间隔，单位毫秒。考虑每站轮询时间各有差异。
    /// </summary>
    public int DefaultPollingInterval { get; set; } = 500;

    /// <summary>
    /// 事件处理超时时间（单位：毫秒）
    /// </summary>
    public int EventHandlerTimeout { get; set; } = 5_000;
}
