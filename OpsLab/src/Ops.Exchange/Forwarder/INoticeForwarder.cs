namespace Ops.Exchange.Forwarder;

/// <summary>
/// 通知事件下游处理对象
/// </summary>
public interface INoticeForwarder : IForwarder
{
    /// <summary>
    /// 执行
    /// </summary>
    /// <returns></returns>
    Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default);
}
