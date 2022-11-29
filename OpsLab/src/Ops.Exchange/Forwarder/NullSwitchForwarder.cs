namespace Ops.Exchange.Forwarder;

internal sealed class NullSwitchForwarder : ISwitchForwarder
{
    public Task ExecuteAsync(SwitchForwardData data, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
