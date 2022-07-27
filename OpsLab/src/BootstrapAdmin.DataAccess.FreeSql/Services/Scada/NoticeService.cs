using BootstrapAdmin.Web.Core.Models;
using BootstrapAdmin.Web.Core.Services;

namespace BootstrapAdmin.DataAccess.FreeSql.Services;

internal sealed class NoticeService : INoticeService
{
    private readonly IFreeSql _freeSql;

    public NoticeService(IFreeSql freeSql) => _freeSql = freeSql;

    public Task<ApiResult> SaveNoticeAsync(ApiData data)
    {
        return Task.FromResult(ApiResult.Ok());
    }
}
