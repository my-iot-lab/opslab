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

    /// <summary>
    /// 获取 Hub 连接。
    /// </summary>
    public HubConnection Connection { get; }

    public SignalRForwarderManager(
        IOptions<OpsUIOptions> opsUIOptions,
        ILogger<OpsSignalRNoticeForwarder> logger)
    {
        _opsUIOptions = opsUIOptions.Value;
        _logger = logger;

        Connection = new HubConnectionBuilder()
               .WithUrl($"{_opsUIOptions.Api.BaseAddress}/Scada")
               .WithAutomaticReconnect(new RetryPolicy())
               .Build();

        // 重连中
        Connection.Reconnecting += error =>
        {
            _logger.LogWarning("[SignalRForwarder] 重连中...");
            return Task.CompletedTask;
        };

        // 重新连接成功，connectionId 是一个新的连接。
        Connection.Reconnected += connectionId =>
        {
            _logger.LogWarning("[SignalRForwarder] 重连成功");
            return Task.CompletedTask;
        };

        // 手动重新连接，在不采用自动重连时可采用此方式。
        //Connection.Closed += async (error) =>
        //{
        //    await Task.Delay(new Random().Next(1, 5) * 1000);
        //    await Connection.StartAsync();
        //};
    }

    /// <summary>
    /// 连接到服务
    /// </summary>
    /// <param name="retry">重试轮询时间间隔</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> ConnectWithRetryAsync(int retryDelay = 5000, CancellationToken cancellationToken = default)
    {
        // WithAutomaticReconnect() 不会将 HubConnection 配置为重试初始启动失败，因此需要手动处理启动失败。
        while (true)
        {
            try
            {
                await Connection.StartAsync(cancellationToken);
                return true;
            }
            catch when (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            catch
            {
                await Task.Delay(retryDelay, cancellationToken);
            }
        }
    }
}

/// <summary>
/// 自定义重连策略。
/// <para>重连规则：重连次数小于50：间隔1s; 重试次数小于250:间隔30s; 重试次数大于250:间隔1m。</para>
/// </summary>
internal sealed class RetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        if (retryContext.PreviousRetryCount < 50)
        {
            return TimeSpan.FromSeconds(1);
        }

        if (retryContext.PreviousRetryCount < 250)
        {
            return TimeSpan.FromSeconds(30);
        }

        return TimeSpan.FromSeconds(60);
    }
}
