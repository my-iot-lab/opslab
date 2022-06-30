using System.Threading.Channels;
using Ops.Exchange.Model;

namespace Ops.Exchange.Management;

/// <summary>
/// 数据回调队列，用于向设备回写数据
/// </summary>
public sealed class CallbackTaskQueue : IDisposable
{
    private const int Capacity = 64;
    private readonly Channel<PayloadContext> _queue;

    public CallbackTaskQueue()
    {
        BoundedChannelOptions options = new(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait, // 以防过多的发布服务器/调用开始累积。
        };
        _queue = Channel.CreateBounded<PayloadContext>(options);
    }

    public async ValueTask QueueAsync(PayloadContext item)
    {
        await _queue.Writer.WriteAsync(item);
    }

    public async ValueTask<PayloadContext> DequeueAsync(CancellationToken cancellationToken)
    {
        var item = await _queue.Reader.ReadAsync(cancellationToken);

        return item;
    }

    /// <summary>
    /// 阻塞直到有可用的数据能取出。
    /// </summary>
    public async ValueTask<(bool ok, PayloadContext? ctx)> WaitDequeueAsync(CancellationToken cancellationToken)
    {
        // 当 Channel 未 Completed，会阻塞直到有可读的数据。
        if (await _queue.Reader.WaitToReadAsync(cancellationToken))
        {
            var item = await _queue.Reader.ReadAsync(cancellationToken);
            return (true, item);
        }

        return (false, null);
    }

    public void Dispose()
    {
        _queue.Writer.TryComplete();
    }
}
