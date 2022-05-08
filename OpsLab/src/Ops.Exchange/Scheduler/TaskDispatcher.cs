namespace Ops.Exchange.Scheduler;

/// <summary>
/// 任务派发器
/// </summary>
public class TaskDispatcher
{
    private readonly TaskStateTransfer _taskStateTransfer;

    public TaskDispatcher(TaskStateTransfer taskStateTransfer)
    {
        _taskStateTransfer = taskStateTransfer;
    }

    /// <summary>
    /// 派发任务
    /// </summary>
    /// <param name="context">任务上下文对象</param>
    public void Dispatch(TaskContext context)
    {
        // 检查任务是否可以下发

    }
}
