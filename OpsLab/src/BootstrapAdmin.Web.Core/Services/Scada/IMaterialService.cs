using BootstrapAdmin.Web.Core.Models;

namespace BootstrapAdmin.Web.Core.Services;

public interface IMaterialService
{
    Task<ApiResult> SaveCriticalMaterialAsync(ApiData data);

    Task<ApiResult> SaveBactchMaterialAsync(ApiData data);
}
