using System.Threading;
using System.Threading.Tasks;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Domain.Services.Impl;

internal class OutboundService : IOutboundService
{
    public Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ReplyResult());
    }

    public void Dispose()
    {
    }
}
