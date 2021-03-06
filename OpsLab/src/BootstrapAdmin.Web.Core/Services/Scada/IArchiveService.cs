using BootstrapAdmin.Web.Core.Models;

namespace BootstrapAdmin.Web.Core.Services;

/// <summary>
/// 出站/存档服务
/// </summary>
public interface IArchiveService
{
    /// <summary>
    /// 产品出站
    /// </summary>
    /// <param name="data">数据</param>
    /// <returns></returns>
    Task<ApiResult> SaveArchiveAsync(ApiData data);
}
