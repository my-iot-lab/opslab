using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ops.Exchange;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.Scada.Forwarders.LocalForwarders;

/// <summary>
/// 本地处理请求/响应事件
/// </summary>
internal sealed class OpsLocalReplyForwarder : IReplyForwarder
{
    private readonly ILogger _logger;

    public OpsLocalReplyForwarder(ILogger<OpsLocalReplyForwarder> logger)
    {
        _logger = logger;
    }

    public Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        try
        {
            // Do somethings ...
            return data.Tag switch
            {
                OpsSymbol.PLC_Sign_Inbound => throw new NotImplementedException(),
                OpsSymbol.PLC_Sign_Archive => throw new NotImplementedException(),
                OpsSymbol.PLC_Sign_Critical_Material => throw new NotImplementedException(),
                OpsSymbol.PLC_Sign_Batch_Material => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OpsLocalReplyForwarder] Handle Error");

            return Task.FromResult(new ReplyResult()
            {
                Result = ExStatusCode.HandlerException,
            });
        }
    }
}
