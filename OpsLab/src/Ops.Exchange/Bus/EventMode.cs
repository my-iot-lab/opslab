namespace Ops.Exchange.Bus;

/// <summary>
/// 事件模式
/// </summary>
internal enum EventMode
{
    None = 0,

    /// <summary>
    /// 仅有请求的事件
    /// </summary>
    OnlyRequest = 1,

    /// <summary>
    /// 仅有响应的事件
    /// </summary>
    OnlyReply = 2,

    /// <summary>
    /// 请求响应事件
    /// </summary>
    RequestReply = 3,
}
