namespace Ops.Exchange.Forwarder;

/// <summary>
/// 开关事件下游处理对象
/// </summary>
public interface ISwitchForwarder : IForwarder
{
    /// <summary>
    /// 执行事件。
    /// </summary>
    /// <param name="data">注：其中 Values 会包含一条数据，此数据为通知数据（触发信号）本身。</param>
    /// <returns></returns>
    Task ExecuteAsync(SwitchForwardData data, CancellationToken cancellationToken = default);
}
