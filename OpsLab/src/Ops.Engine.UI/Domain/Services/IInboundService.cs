using System.Threading;
using System.Threading.Tasks;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Domain.Services;

/// <summary>
/// 产品进站服务
/// </summary>
public interface IInboundService : IDomainService
{
    /// <summary>
    /// 执行进站请求
    /// </summary>
    /// <param name="data">进站数据</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default);
}
