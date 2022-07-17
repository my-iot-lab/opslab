using Microsoft.AspNetCore.SignalR;

namespace BootstrapAdmin.Web.Hubs;

/// <summary>
/// Scada 数据监控 Hub
/// </summary>
public class ScadaHub : Hub
{
    /// <summary>
    /// 向连接的客户端发送消息
    /// </summary>
    public async Task SendMessage(string user, string message, CancellationToken cancellationToken = default)
        => await Clients.All.SendAsync("ReceiveMessage", user, message, cancellationToken); // 客户端 ReceiveMessage 结束消息
}
