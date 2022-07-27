using BootstrapAdmin.Web.Core.Models;
using BootstrapAdmin.Web.Core.Services;

namespace BootstrapAdmin.DataAccess.FreeSql.Services;

internal sealed class MaterialService : IMaterialService
{
    private readonly IFreeSql _freeSql;

    public MaterialService(IFreeSql freeSql) => _freeSql = freeSql;

    public async Task<ApiResult> SaveCriticalMaterialAsync(ApiData data)
    {
        await Task.Delay(100);

        var barcode = data.GetValue<string>("PLC_Critical_Material_Barcode");
        var index = data.GetValue<int>("PLC_Critical_Material_Index");
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return ApiResult.Error();
        }

        try
        {
            // 1. 校验物料是否是重复使用
            // 2. 校验 BOM

            return ApiResult.Ok();
        }
        catch (Exception ex)
        {
            return ApiResult.Error(ex.Message);
        }
    }

    public Task<ApiResult> SaveBactchMaterialAsync(ApiData data)
    {
        return Task.FromResult(ApiResult.Ok());
    }
}
