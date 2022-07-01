using System.Threading;
using System.Threading.Tasks;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Domain.Services.Impl;

internal sealed class NoticeService : INoticeService
{
    public Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}
