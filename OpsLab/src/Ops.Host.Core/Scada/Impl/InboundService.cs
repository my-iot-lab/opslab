using Ops.Exchange.Forwarder;
using Ops.Host.Core.Utils;

namespace Ops.Host.Core.Services;

internal sealed class InboundService : IInboundService
{
    private readonly IFreeSql _freeSql;

    public InboundService(IFreeSql freeSql) => _freeSql = freeSql;

    public async Task<ReplyResult> SaveInboundAsync(ForwardData data)
    {
        await Task.Delay(100);

        var sn = data.GetValue<string>("PLC_Inbound_SN");
        var formula = data.GetValue<short>("PLC_Inbound_Formula");
        var pallet = data.GetValue<string>("PLC_Inbound_Pallet");
        if (string.IsNullOrWhiteSpace(sn))
        {
            return ReplyResultHelper.Error();
        }
        if (formula == 0)
        {
            return ReplyResultHelper.Error();
        }

        try
        { 

            return ReplyResultHelper.Ok();
        }
        catch (Exception ex)
        {
            return ReplyResultHelper.Error();
        }
    }
}
