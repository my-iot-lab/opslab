using BootstrapAdmin.Web.Core.Models;
using BootstrapAdmin.Web.Core.Services;

namespace BootstrapAdmin.DataAccess.FreeSql.Services;

internal sealed class AlarmService : IAlarmService
{
    private readonly IFreeSql _freeSql;

    public AlarmService(IFreeSql freeSql) => _freeSql = freeSql;

    public async Task<ApiResult> SaveAlarmsAsync(ApiData data)
    {
        await Task.Delay(100); // test

        if (data.Values.Length == 0)
        {
            return ApiResult.Ok();
        }

        var alarmValue = data.Values[0].GetValue<int>(); // 警报数据（JSON 转换后成了 int32 类型）
        if (alarmValue == 0)
        {
            return ApiResult.Ok();
        }

        try
        {
            // 按位解析

            // 根据字典表中的配置，解析 int32 对应位（从低到高位）警报类型。
            // 字典配置基数从 1 开始。
            for (int i = 0; i < 31; i++)
            {
                if ((alarmValue & 1 << i) > 0)
                {
                }
            }
        }
        catch (Exception ex)
        {
            return ApiResult.Error(ex.Message);
        }

        return ApiResult.Ok();
    }
}
