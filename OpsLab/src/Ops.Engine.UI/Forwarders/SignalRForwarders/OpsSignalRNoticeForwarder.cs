using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Forwarders.SignalRForwarders;

internal sealed class OpsSignalRNoticeForwarder : INoticeForwarder
{
    public readonly SignalRForwarderManager _signalRManager;
    private readonly ILogger _logger;

    public OpsSignalRNoticeForwarder(
        SignalRForwarderManager signalRForwarderManager,
        ILogger<OpsSignalRNoticeForwarder> logger)
    {
        _signalRManager = signalRForwarderManager;
        _logger = logger;

        _signalRManager.Connection.On<HttpResult>("NoticeCallback", OnCallback); // 可以重复注册（会更新）
    }

    public async Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        try
        {
            await _signalRManager.Connection.InvokeAsync("Notice", data, cancellationToken);
        }
        catch (Exception)
        {

        }
    }

    void OnCallback(HttpResult httpResult)
    {
        if (httpResult?.IsOk() != true)
        {
            // 记录数据推送失败信息

            return;
        }
    }
}
