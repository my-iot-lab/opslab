namespace Ops.Scada.Dispatcher;

/// <summary>
/// 任务状态
/// </summary>
internal class TaskState
{
    /// <summary>
    /// 任务处理标识。
    /// 0 --> 初始状态;
    /// 1 --> 已就绪，待被处理;
    /// 2 ~ N --> 其他状态。
    /// </summary>
    public int Tag { get; set; }


}
