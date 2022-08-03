using Ops.Exchange.Forwarder;

namespace Ops.Host.Core.Services;

public interface IMaterialService
{
    Task<ReplyResult> SaveCriticalMaterialAsync(ForwardData data);

    Task<ReplyResult> SaveBactchMaterialAsync(ForwardData data);
}
