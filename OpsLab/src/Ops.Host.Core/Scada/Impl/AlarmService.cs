using Ops.Exchange.Forwarder;
using Ops.Host.Core.Utils;

namespace Ops.Host.Core.Services;

internal sealed class AlarmService : IAlarmService
{
    private readonly IFreeSql _freeSql;

    public AlarmService(IFreeSql freeSql) => _freeSql = freeSql;

    public async Task<ReplyResult> SaveAlarmsAsync(ForwardData data)
    {
        await Task.Delay(100); // test

        if (data.Values.Length == 0)
        {
            return ReplyResultHelper.Ok();
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
        catch (Exception)
        {
            return ReplyResultHelper.Error();
        }

        return ReplyResultHelper.Ok();
    }
}
