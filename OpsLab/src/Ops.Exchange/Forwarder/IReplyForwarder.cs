namespace Ops.Exchange.Forwarder;

/// <summary>
/// 回复事件下游处理对象
/// </summary>
public interface IReplyForwarder : IForwarder
{
    /// <summary>
    /// 执行事件
    /// </summary>
    /// <param name="data">注：其中 Values 不包含触发信号自身数据。</param>
    /// <returns></returns>
    Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default);
}
