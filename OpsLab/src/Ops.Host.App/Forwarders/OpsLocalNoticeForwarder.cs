using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ops.Exchange.Forwarder;
using Ops.Host.Core.Services;

namespace Ops.Host.App.Forwarders;

/// <summary>
/// 本地处理通知事件
/// </summary>
internal sealed class OpsLocalNoticeForwarder : INoticeForwarder
{
    private readonly IAlarmService _alarmService;
    private readonly INoticeService _noticeService;
    private readonly ILogger _logger;

    public OpsLocalNoticeForwarder(
        IAlarmService alarmService,
        INoticeService noticeService,
        ILogger<OpsLocalNoticeForwarder> logger)
    {
        _alarmService = alarmService;
        _noticeService = noticeService;
        _logger = logger;
    }

    public async Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        // 警报消息，0 表示无任何异常，不用推送
        if (data.Tag == OpsSymbol.PLC_Sys_Alarm)
        {
            if ((uint)data.Values[0].Value == 0)
            {
                return;
            }

            await _alarmService.SaveAlarmsAsync(data);
            return;
        }

        await _noticeService.SaveNoticeAsync(data);
    }
}
