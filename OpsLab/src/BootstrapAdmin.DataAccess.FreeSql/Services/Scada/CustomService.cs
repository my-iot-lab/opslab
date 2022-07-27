using BootstrapAdmin.Web.Core.Models;
using BootstrapAdmin.Web.Core.Services;

namespace BootstrapAdmin.DataAccess.FreeSql.Services;

internal sealed class CustomService : ICustomService
{
    private readonly IFreeSql _freeSql;

    public CustomService(IFreeSql freeSql) => _freeSql = freeSql;

    public Task<ApiResult> SaveCustomAsync(ApiData data)
    {
        return Task.FromResult(ApiResult.Ok());
    }
}
