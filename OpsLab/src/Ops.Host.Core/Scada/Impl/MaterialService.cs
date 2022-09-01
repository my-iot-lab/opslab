using Ops.Exchange.Forwarder;
using Ops.Host.Core.Const;
using Ops.Host.Core.Utils;

namespace Ops.Host.Core.Services;

internal sealed class MaterialService : IMaterialService
{
    private readonly IFreeSql _freeSql;

    public MaterialService(IFreeSql freeSql) => _freeSql = freeSql;

    public async Task<ReplyResult> HandleCriticalMaterialAsync(ForwardData data)
    {
        await Task.Delay(100);

        var barcode = data.GetString(PlcSymbolTag.PLC_Critical_Material_Barcode); // 物料条码
        var index = data.GetInt(PlcSymbolTag.PLC_Critical_Material_Index); // 扫描索引
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
        catch (Exception)
        {
            return ReplyResultHelper.Error();
        }
    }

    public Task<ReplyResult> HandleBactchMaterialAsync(ForwardData data)
    {
        return Task.FromResult(ReplyResultHelper.Ok());
    }
}
