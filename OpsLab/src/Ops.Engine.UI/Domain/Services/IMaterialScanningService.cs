using System.Threading;
using System.Threading.Tasks;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Domain.Services;

/// <summary>
/// 物料扫入服务
/// </summary>
public interface IMaterialScanningService : IDomainService
{
    /// <summary>
    /// 扫关键物料请求
    /// </summary>
    /// <param name="data">关键物料数据</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ReplyResult> ScanCriticalAsync(ForwardData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// 扫批次料请求
    /// </summary>
    /// <param name="data">批次料数据</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ReplyResult> ScanBatchAsync(ForwardData data, CancellationToken cancellationToken = default);
}
