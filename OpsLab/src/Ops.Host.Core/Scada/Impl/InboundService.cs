using Ops.Exchange.Forwarder;
using Ops.Host.Core.Const;
using Ops.Host.Core.Utils;

namespace Ops.Host.Core.Services;

internal sealed class InboundService : IInboundService
{
    private readonly IFreeSql _freeSql;

    public InboundService(IFreeSql freeSql) => _freeSql = freeSql;

    public async Task<ReplyResult> HandleAsync(ForwardData data)
    {
        await Task.Delay(100);

        var sn = data.GetString(PlcSymbolTag.PLC_Inbound_SN); // SN
        var formula = data.GetInt(PlcSymbolTag.PLC_Inbound_Formula); // PLC 配方号
        var pallet = data.GetString(PlcSymbolTag.PLC_Inbound_Pallet); // 托盘码
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
        catch (Exception)
        {
            return ReplyResultHelper.Error();
        }
    }
}
