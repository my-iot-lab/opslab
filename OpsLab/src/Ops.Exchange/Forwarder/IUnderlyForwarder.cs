namespace Ops.Exchange.Forwarder;

/// <summary>
/// 底层事件下游处理对象。
/// </summary>
public interface IUnderlyForwarder
{
    /// <summary>
    /// 执行事件
    /// </summary>
    /// <param name="data">其中 Values 包含触发信号自身数据。</param>
    /// <returns></returns>
    Task<UnderlyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default);
}
