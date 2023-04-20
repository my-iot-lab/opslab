namespace Ops.Exchange.Common;

/// <summary>
/// 后台线程的任务队列。
/// </summary>
/// <typeparam name="T"></typeparam>
public class BackgroundTaskQueue<T> : IDisposable
{
    private readonly Channel<T> _queue;

    /// <summary>
    /// 初始化一个新的对象。
    /// </summary>
    /// <param name="capacity">队列容量</param>
    /// <param name="fullMode">FullMode, 默认为 Wait，以防过多的发布服务器/调用开始累积。</param>
    public BackgroundTaskQueue(int capacity = 64, BoundedChannelFullMode fullMode = BoundedChannelFullMode.Wait)
    {
        BoundedChannelOptions options = new(capacity)
        {
            FullMode = fullMode,
        };
        _queue = Channel.CreateBounded<T>(options);
    }

    /// <summary>
    /// 向队列中写入数据。
    /// </summary>
    /// <param name="item">要写入的数据项。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask QueueAsync(T item, CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 从队列中读取数据。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<T> DequeueAsync(CancellationToken cancellationToken = default)
    {
        var item = await _queue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        return item;
    }

    /// <summary>
    /// 阻塞直到有可用的数据能取出。
    /// </summary>
    public async ValueTask<(bool ok, T? ctx)> WaitDequeueAsync(CancellationToken cancellationToken = default)
    {
        // 当 Channel 未 Completed，会阻塞直到有可读的数据。
        if (await _queue.Reader.WaitToReadAsync(cancellationToken))
        {
            var item = await _queue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            return (true, item);
        }

        return (false, default);
    }

    public void Dispose() => _queue.Writer.TryComplete();
}
