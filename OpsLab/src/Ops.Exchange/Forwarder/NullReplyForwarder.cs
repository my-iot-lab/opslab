namespace Ops.Exchange.Forwarder;

internal sealed class NullReplyForwarder : IReplyForwarder
{
    public Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ReplyResult());
    }

    public void Dispose()
    {

    }
}
