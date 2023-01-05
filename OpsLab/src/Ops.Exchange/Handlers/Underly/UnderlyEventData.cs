using Ops.Exchange.Bus;
using Ops.Exchange.Model;

namespace Ops.Exchange.Handlers.Underly;

/// <summary>
/// 底层请求/响应事件数据
/// </summary>
public sealed class UnderlyEventData : EventData
{
    /// <summary>
    /// 请求的数据上下文
    /// </summary>
    public PayloadContext Context { get; }

    /// <summary>
    /// 事件标签 Tag（唯一）
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// 事件名称
    /// </summary>
    public string? Name { get; }

    public PayloadData[] Values { get; }

    /// <summary>
    /// 处理任务超时时间，毫秒
    /// </summary>
    public int HandleTimeout { get; set; }

    public UnderlyEventData(PayloadContext context, string tag, string? name, PayloadData[] values)
    {
        Context = context;
        Tag = tag;
        Name = name;
        Values = values;
    }
}
