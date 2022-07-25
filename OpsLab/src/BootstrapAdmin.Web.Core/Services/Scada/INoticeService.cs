using BootstrapAdmin.Web.Core.Models;

namespace BootstrapAdmin.Web.Core.Services;

public interface INoticeService
{
    Task<ApiResult> SaveNoticeAsync(ApiData data);
}
