using Ops.Exchange.Forwarder;
using Ops.Host.Core.Utils;

namespace Ops.Host.Core.Services;

internal sealed class CustomService : ICustomService
{
    private readonly IFreeSql _freeSql;

    public CustomService(IFreeSql freeSql) => _freeSql = freeSql;

    public Task<ReplyResult> SaveCustomAsync(ForwardData data)
    {
        return Task.FromResult(ReplyResultHelper.Ok());
    }
}
