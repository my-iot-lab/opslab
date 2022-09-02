namespace Ops.Exchange.Forwarder;

public class ObservableReplyForwarder : IObservableReplyForwarder
{
    public ObservableReplyForwarder()
    {

    }

    public Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
