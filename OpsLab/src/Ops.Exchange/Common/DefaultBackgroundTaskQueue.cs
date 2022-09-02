namespace Ops.Exchange.Common;

/// <summary>
/// 后台队列服务。
/// </summary>
internal sealed class DefaultBackgroundTaskQueue : BackgroundTaskQueue<Func<CancellationToken, ValueTask>>, IBackgroundTaskQueue
{
}
