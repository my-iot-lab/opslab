using BootstrapAdmin.Web.Core.Models;
using BootstrapAdmin.Web.Core.Services;

namespace BootstrapAdmin.DataAccess.FreeSql.Services;

internal sealed class ArchiveService : IArchiveService
{
    private readonly IFreeSql _freeSql;

    public ArchiveService(IFreeSql freeSql) => _freeSql = freeSql;

    public async Task<ApiResult> SaveArchiveAsync(ApiData data)
    {
        await Task.Delay(100); // test

        var sn = data.GetValue<string>("PLC_Archive_SN");
        var pass = data.GetValue<int>("PLC_Archive_Pass");
        var ct = data.GetValue<int>("PLC_Archive_Cycletime");
        var @operator = data.GetValue<string>("PLC_Archive_Operator");
        var shift = data.GetValue<int>("PLC_Archive_Shift");
        var pallet = data.GetValue<string>("PLC_Archive_Pallet");

        if (string.IsNullOrWhiteSpace(sn))
        {
            return ApiResult.Error();
        }

        // 记录进站信息
        try
        {
            // 按位解析

            // 主数据

            // 明细数据
            foreach (var item in data.Values.Where(s => s.IsAdditional))
            {
               
            }

            // 工站状态统计

            return ApiResult.Ok();
        }
        catch(Exception ex)
        {
            return ApiResult.Error(ex.Message);
        }
    }
}
