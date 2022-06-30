using Microsoft.Extensions.Logging;
using Ops.Exchange.Bus;
using Ops.Exchange.Management;
using Ops.Exchange.Model;

namespace Ops.Exchange.Handlers.Heartbeat;

/// <summary>
/// 基于心跳的事件处理。
/// <para>此事件处理器，用于处理心跳响应事件。</para>
/// </summary>
internal sealed class HeartbeatEventHandler : IEventHandler<HeartbeatEventData>
{
    private readonly CallbackTaskQueue _callbackTaskQueue;
    private readonly ILogger _logger;

    public HeartbeatEventHandler(
        CallbackTaskQueue callbackTaskQueue,
        ILogger<HeartbeatEventHandler> logger)
    {
        _callbackTaskQueue = callbackTaskQueue;
        _logger = logger;
    }

    public async Task HandleAsync(HeartbeatEventData eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"{eventData.Context.Request.DeviceInfo.Schema.Station} - {eventData.Tag} - {eventData.State}");

        var variable = eventData.Context.Request.DeviceInfo.GetVariable(eventData.Tag);
        eventData.Context.Response.Values.Add(PayloadData.From(variable!, (short)0)); // 注：类型要与之类型对应，防止强制转换异常
        await _callbackTaskQueue.QueueAsync(eventData.Context);
    }
}
