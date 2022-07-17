using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Forwarders.SignalRForwarders;

internal sealed class OpsSignalRNoticeForwarder : INoticeForwarder
{
    public readonly SignalRForwarderManager _signalRForwarderManager;
    private readonly ILogger _logger;

    public OpsSignalRNoticeForwarder(
        SignalRForwarderManager signalRForwarderManager,
        ILogger<OpsSignalRNoticeForwarder> logger)
    {
        _signalRForwarderManager = signalRForwarderManager;
        _logger = logger;
    }

    public async Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        // 可以重复注册（会更新）
        _signalRForwarderManager.Connection.On<HttpResult>("NoticeCallback", result =>
        {

        });

        try
        {
            await _signalRForwarderManager.Connection.InvokeAsync("Notice", data, cancellationToken);
        }
        catch (Exception)
        {
        }
    }
}
