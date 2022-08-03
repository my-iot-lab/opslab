using Ops.Exchange.Forwarder;
using Ops.Host.Core.Utils;

namespace Ops.Host.Core.Services;

internal sealed class MaterialService : IMaterialService
{
    private readonly IFreeSql _freeSql;

    public MaterialService(IFreeSql freeSql) => _freeSql = freeSql;

    public async Task<ReplyResult> SaveCriticalMaterialAsync(ForwardData data)
    {
        await Task.Delay(100);

        var barcode = data.GetValue<string>("PLC_Critical_Material_Barcode");
        var index = data.GetValue<short>("PLC_Critical_Material_Index");
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return ReplyResultHelper.Error();
        }

        try
        {
            // 1. 校验物料是否是重复使用
            // 2. 校验 BOM

            return ReplyResultHelper.Ok();
        }
        catch (Exception ex)
        {
            return ReplyResultHelper.Error();
        }
    }

    public Task<ReplyResult> SaveBactchMaterialAsync(ForwardData data)
    {
        return Task.FromResult(ReplyResultHelper.Ok());
    }
}
