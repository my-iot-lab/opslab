using System.Threading;
using System.Threading.Tasks;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.Scada.Forwarders.SignalRForwarders;

internal sealed class OpsSignalRReplyForwarder : IReplyForwarder
{
    public Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}
