namespace Ops.Exchange.Common;

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
    ValueTask QueueAsync(Func<CancellationToken, ValueTask> workItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// 队列出站
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 阻塞直到有可用的数据能取出。
    /// </summary>
    ValueTask<(bool ok, Func<CancellationToken, ValueTask>? ctx)> WaitDequeueAsync(CancellationToken cancellationToken = default);
}
