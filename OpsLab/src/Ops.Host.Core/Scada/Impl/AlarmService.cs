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

        var alarmValue = data.Values[0].GetValue<uint>(); // 警报数据
        if (alarmValue == 0)
        {
            return ReplyResultHelper.Ok();
        }

        try
        {
            // 按位解析

            // 根据字典表中的配置，解析 uint32 对应位（从低到高位）警报类型。
            // 字典配置基数从 1 开始。
            for (int i = 0; i < 32; i++)
            {
                if ((alarmValue & 1 << i) > 0)
                {
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
