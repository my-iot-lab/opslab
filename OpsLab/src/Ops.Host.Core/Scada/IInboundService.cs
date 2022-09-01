using Ops.Exchange.Forwarder;

namespace Ops.Host.Core.Services;

/// <summary>
/// 进站服务
/// </summary>
public interface IInboundService
{
    /// <summary>
    /// 产品进站
    /// </summary>
    /// <param name="data">数据</param>
    /// <returns></returns>
    Task<ReplyResult> HandleAsync(ForwardData data);
}
