using System;
using System.Linq;
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
        try
        {
            // 警报信息，采用 bool 数组，可以自定义长度。
            // 警报消息，所有都为 false 表示无任何异常，不用处理。
            if (data.Tag == OpsSymbol.PLC_Sys_Alarm)
            {
                var arr = data.Values[0].GetValue<bool[]>();
                if (arr!.All(s => !s))
                {
                    return;
                }

                await _alarmService.SaveAlarmsAsync(data);
                return;
            }

            await _noticeService.SaveNoticeAsync(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OpsNoticeForwarder] Handle Error");
        }
    }
}
