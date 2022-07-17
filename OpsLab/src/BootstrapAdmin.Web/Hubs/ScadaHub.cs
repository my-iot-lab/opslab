using Microsoft.AspNetCore.SignalR;
using BootstrapAdmin.Web.Models;

namespace BootstrapAdmin.Web.Hubs;

/// <summary>
/// Scada 数据监控 Hub
/// </summary>
public class ScadaHub : Hub
{
    /// <summary>
    /// 进站
    /// </summary>
    /// <returns></returns>
    public async Task Inbound(ApiData data, CancellationToken cancellationToken = default)
    {
        await Clients.All.SendAsync("InboundCallback", ApiResult.Ok(), cancellationToken);
    }

    /// <summary>
    /// 出站
    /// </summary>
    /// <returns></returns>
    public async Task Outbound(ApiData data, CancellationToken cancellationToken = default)
    {
        await Clients.All.SendAsync("OutboundCallback", ApiResult.Ok(), cancellationToken);
    }
      
    /// <summary>
    /// 扫关键物料
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task MaterialCritical(ApiData data, CancellationToken cancellationToken = default)
    {
        await Clients.All.SendAsync("MaterialCriticalCallback", ApiResult.Ok(), cancellationToken);
    }

    /// <summary>
    /// 扫批次料
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task MaterialBatch(ApiData data, CancellationToken cancellationToken = default)
    {
        await Clients.All.SendAsync("MaterialBatchCallback", ApiResult.Ok(), cancellationToken);
    }

    /// <summary>
    /// 通知消息
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Notice(ApiData data, CancellationToken cancellationToken = default)
    {
        await Clients.All.SendAsync("NoticeCallback", ApiResult.Ok(), cancellationToken);
    }

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}
