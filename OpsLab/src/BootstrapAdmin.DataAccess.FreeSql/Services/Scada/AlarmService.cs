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

        var alarmValues = data.Values[0].GetValue<bool[]>(); // 警报数据
        try
        {
            for (int i = 0; i < alarmValues!.Length; i++)
            {
                if (alarmValues[i])
                {
                    // DoSomething...
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
