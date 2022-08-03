using Ops.Exchange.Forwarder;

namespace Ops.Host.Core.Services;

/// <summary>
/// 自定义服务
/// </summary>
public interface ICustomService
{
    /// <summary>
    /// 触发自定义数据。
    /// </summary>
    /// <param name="data">数据</param>
    /// <returns></returns>
    Task<ReplyResult> SaveCustomAsync(ForwardData data);
}
