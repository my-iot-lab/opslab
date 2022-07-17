using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ops.Engine.UI.Config;

namespace Ops.Engine.UI.Forwarders.SignalRForwarders;

internal sealed class SignalRForwarderManager
{
    private readonly OpsUIOptions _opsUIOptions;
    private readonly ILogger _logger;

    public HubConnection Connection { get; }

    public SignalRForwarderManager(
        IOptions<OpsUIOptions> opsUIOptions,
        ILogger<OpsSignalRNoticeForwarder> logger)
    {
        _opsUIOptions = opsUIOptions.Value;
        _logger = logger;

        Connection = new HubConnectionBuilder()
               .WithUrl($"{_opsUIOptions.Api.BaseAddress}/Scada")
               .WithAutomaticReconnect()
               .Build();

        Connection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await Connection.StartAsync();
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Connection.StartAsync(cancellationToken);
        }
        catch (Exception)
        {
        }
    }
}
