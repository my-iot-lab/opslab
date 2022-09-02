namespace Ops.Exchange.Forwarder;

/// <summary>
/// Observable 模式的回复事件下游处理对象，适用于异步响应模式。
/// </summary>
public interface IObservableReplyForwarder : IForwarder
{
    /// <summary>
    /// 执行事件。
    /// </summary>
    /// <param name="data">注：其中 Values 不包含触发信号自身数据。</param>
    /// <returns></returns>
    Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default);
}
