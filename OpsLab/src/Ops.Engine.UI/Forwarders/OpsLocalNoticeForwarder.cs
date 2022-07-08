using System.Threading;
using System.Threading.Tasks;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Forwarders;

/// <summary>
/// 本地处理通知事件
/// </summary>
internal class OpsLocalNoticeForwarder : INoticeForwarder
{
    public Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        // Do somethings ...

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        
    }
}
