using System.Threading;
using System.Threading.Tasks;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Domain.Services;

/// <summary>
/// 产品出站服务
/// </summary>
public interface IOutboundService : IDomainService
{
    /// <summary>
    /// 执行出站请求
    /// </summary>
    /// <param name="data">出站数据</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default);
}
