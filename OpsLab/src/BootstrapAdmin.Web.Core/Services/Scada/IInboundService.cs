using BootstrapAdmin.Web.Core.Models;

namespace BootstrapAdmin.Web.Core.Services;

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
    Task<ApiResult> SaveInboundAsync(ApiData data);
}
