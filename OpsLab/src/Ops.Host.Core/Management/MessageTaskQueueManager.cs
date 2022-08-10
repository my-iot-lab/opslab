using System.Threading.Channels;

namespace Ops.Host.Core.Management;

/// <summary>
/// 队列消息
/// </summary>
public sealed class Message
{
    /// <summary>
    /// 产线
    /// </summary>
    public string Line { get; }

    /// <summary>
    /// 工站
    /// </summary>
    public string Station { get; }

    /// <summary>
    /// 消息代码，可用于区分是哪种消息。
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 消息名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 消息的值对象。
    /// </summary>
    public object? Value { get; }

    public Message(string line, string station, string code, string name, object? value = null)
    {
        Line = line;
        Station = station;
        Code = code;
        Name = name;
        Value = value;
    }
}

/// <summary>
/// 消息传递队列，可以在业务端写入消息，其他地方如 UI 端消费。
/// <para>注：此消息队并不可靠，当队列中消息到达容量而新消息还在写入时，会丢弃最旧的一部分。</para>
/// </summary>
public sealed class MessageTaskQueueManager : IDisposable
{
    private const int Capacity = 64;
    private readonly Channel<Message> _queue;

    /// <summary>
    /// 消息队列实例。
    /// </summary>
    public static readonly MessageTaskQueueManager Default = new();

    private MessageTaskQueueManager()
    {
        BoundedChannelOptions options = new(Capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest, // 以防过多的发布服务器/调用开始累积。
        };
        _queue = Channel.CreateBounded<Message>(options);
    }

    public async ValueTask QueueAsync(Message item, CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(item, cancellationToken);
    }

    public async ValueTask<Message> DequeueAsync(CancellationToken cancellationToken = default)
    {
        var item = await _queue.Reader.ReadAsync(cancellationToken);
        return item;
    }

    /// <summary>
    /// 阻塞直到有可用的数据能取出。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>若 Channel Completed，返回 ok 为 false。</returns>
    public async ValueTask<(bool ok, Message? ctx)> WaitDequeueAsync(CancellationToken cancellationToken = default)
    {
        // 当 Channel 未 Completed，会阻塞直到有可读的数据。
        if (await _queue.Reader.WaitToReadAsync(cancellationToken))
        {
            var item = await _queue.Reader.ReadAsync(cancellationToken);
            return (true, item);
        }

        return (false, null);
    }

    /// <summary>
    /// 关闭队列，意味着队列不能再写入数据。
    /// </summary>
    public void Dispose()
    {
        _queue.Writer.TryComplete();
    }
}
