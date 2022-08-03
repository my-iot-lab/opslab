using Ops.Exchange.Forwarder;
using Ops.Host.Core.Utils;

namespace Ops.Host.Core.Services;

internal sealed class NoticeService : INoticeService
{
    private readonly IFreeSql _freeSql;

    public NoticeService(IFreeSql freeSql) => _freeSql = freeSql;

    public Task<ReplyResult> SaveNoticeAsync(ForwardData data)
    {
        return Task.FromResult(ReplyResultHelper.Ok());
    }
}
