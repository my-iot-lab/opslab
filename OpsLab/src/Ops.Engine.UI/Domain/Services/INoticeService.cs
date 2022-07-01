using System.Threading;
using System.Threading.Tasks;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Domain.Services;

/// <summary>
/// 通知服务
/// </summary>
public interface INoticeService : IDomainService
{
    /// <summary>
    /// 设备数据通知服务
    /// </summary>
    /// <param name="data">要通知的数据</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default);
}
