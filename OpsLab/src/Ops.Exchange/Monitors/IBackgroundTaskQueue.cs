namespace Ops.Exchange.Monitors;

/// <summary>
/// 后台队列服务接口
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// 队列进站
    /// </summary>
    /// <param name="workItem"></param>
    /// <returns></returns>
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

    /// <summary>
    /// 队列出站
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}
