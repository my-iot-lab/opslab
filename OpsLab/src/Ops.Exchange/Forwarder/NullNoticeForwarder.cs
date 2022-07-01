namespace Ops.Exchange.Forwarder;

internal sealed class NullNoticeForwarder : INoticeForwarder
{
    public Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        
    }
}
