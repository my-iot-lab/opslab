using Ops.Exchange.Forwarder;

namespace Ops.Host.Core.Services;

internal sealed class AndonService : IAndonService
{
    public Task HandleAsync(ForwardData data)
    {
        return Task.CompletedTask;
    }
}
