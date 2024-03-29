﻿using Ops.Exchange.Bus;
using Ops.Exchange.Model;

namespace Ops.Exchange.Handlers.Reply;

/// <summary>
/// 请求/响应事件数据
/// </summary>
public sealed class ReplyEventData : EventData
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

    /// <summary>
    /// 任务处理标识。
    /// 0 --> 初始状态;
    /// 1 --> 已就绪，待被处理;
    /// 2 ~ N --> 其他状态。
    /// <para>只有为 1 时才能触发数据推送到下游处理。</para>
    /// </summary>
    public int State { get; }

    public PayloadData[] Values { get; }

    /// <summary>
    /// 处理任务超时时间，毫秒
    /// </summary>
    public int HandleTimeout { get; set; }

    public ReplyEventData(PayloadContext context, string tag, string? name, int state, PayloadData[] values)
    {
        Context = context;
        Tag = tag;
        Name = name;
        State = state;
        Values = values;
    }
}
