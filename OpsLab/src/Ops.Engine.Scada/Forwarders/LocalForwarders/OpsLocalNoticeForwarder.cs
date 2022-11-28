using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ops.Exchange.Forwarder;
using Ops.Exchange.Model;

namespace Ops.Engine.Scada.Forwarders.LocalForwarders;

/// <summary>
/// 本地处理通知事件
/// </summary>
internal sealed class OpsLocalNoticeForwarder : INoticeForwarder
{
    private readonly ILogger _logger;

    public OpsLocalNoticeForwarder(
        ILogger<OpsLocalNoticeForwarder> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        try
        {
            // 警报信息，采用 bool 数组，可以自定义长度。
            // 警报消息，所有都为 false 表示无任何异常，不用处理。
            if (data.Tag == OpsSymbol.PLC_Sys_Alarm)
            {
                var arr = data.Values[0].GetBitArray();
                if (arr!.All(s => !s))
                {
                    return Task.CompletedTask;
                }

                return Task.CompletedTask;
            }

            // 正常推送
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OpsLocalNoticeForwarder] Handle Error");
        }

        return Task.CompletedTask;
    }
}
