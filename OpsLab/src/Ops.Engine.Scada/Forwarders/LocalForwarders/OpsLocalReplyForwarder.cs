using System.Threading;
using System.Threading.Tasks;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.Scada.Forwarders.LocalForwarders;

/// <summary>
/// 本地处理请求/响应事件
/// </summary>
internal sealed class OpsLocalReplyForwarder : IReplyForwarder
{
    public Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        // Do somethings ...

        return Task.FromResult(new ReplyResult());
    }
}
