namespace Ops.Exchange.Forwarder;

public interface IReplyForwarder : IForwarder
{
    /// <summary>
    /// 执行
    /// </summary>
    /// <returns></returns>
    Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default);
}
