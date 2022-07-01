using System.Threading;
using System.Threading.Tasks;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Domain.Services.Impl;

internal sealed class MaterialScanningService : IMaterialScanningService
{
    public Task<ReplyResult> ScanBatchAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ReplyResult());
    }

    public Task<ReplyResult> ScanCriticalAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ReplyResult());
    }

    public void Dispose()
    {

    }
}
