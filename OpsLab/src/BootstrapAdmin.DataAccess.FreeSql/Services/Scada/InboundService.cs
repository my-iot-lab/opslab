using BootstrapAdmin.Web.Core.Models;
using BootstrapAdmin.Web.Core.Services;

namespace BootstrapAdmin.DataAccess.FreeSql.Services;

internal sealed class InboundService : IInboundService
{
    private readonly IFreeSql _freeSql;

    public InboundService(IFreeSql freeSql) => _freeSql = freeSql;

    public async Task<ApiResult> SaveInboundAsync(ApiData data)
    {
        await Task.Delay(100);

        var sn = data.GetValue<string>("PLC_Inbound_SN");
        var formula = data.GetValue<int>("PLC_Inbound_Formula");
        var pallet = data.GetValue<string>("PLC_Inbound_Pallet");
        if (string.IsNullOrWhiteSpace(sn))
        {
            return ApiResult.Error();
        }
        if (formula == 0)
        {
            return ApiResult.Error();
        }

        try
        { 

            return ApiResult.Ok();
        }
        catch (Exception ex)
        {
            return ApiResult.Error(ex.Message);
        }
    }
}
