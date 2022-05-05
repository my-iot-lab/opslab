namespace Ops.Scada.Monitor;

/// <summary>
/// 时间状态
/// </summary>
internal class EventState
{
    /// <summary>
    /// 任务获取的开始时间
    /// </summary>
    public DateTime D1 { get; set; }

    /// <summary>
    /// 任务获取的结束时间
    /// </summary>
    public DateTime D2 { get; set; }

    /// <summary>
    /// 任务推送的开始时间
    /// </summary>
    public DateTime D3 { get; set; }

    /// <summary>
    /// 任务相应的结束时间
    /// </summary>
    public DateTime D4 { get; set; }

    /// <summary>
    /// 任务回写的开始时间
    /// </summary>
    public DateTime D5 { get; set; }

    /// <summary>
    /// 推送数据
    /// </summary>
    public object Value1 { get; set; }

    /// <summary>
    /// 回调数据
    /// </summary>
    public object Value2 { get; set; }
}
