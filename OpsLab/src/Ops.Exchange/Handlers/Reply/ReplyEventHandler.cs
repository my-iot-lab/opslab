using Microsoft.Extensions.Logging;
using Ops.Exchange.Bus;
using Ops.Exchange.Management;
using Ops.Exchange.Model;
using Ops.Exchange.Stateless;

namespace Ops.Exchange.Handlers.Reply;

/// <summary>
/// 基于响应的事件处理。
/// <para>请求响应事件</para>
/// </summary>
internal sealed class ReplyEventHandler : IEventHandler<ReplyEventData>
{
    private readonly StateManager _stateManager;
    private readonly CallbackTaskQueue _callbackTaskQueue;
    private readonly ILogger _logger;

    public ReplyEventHandler(StateManager stateManager,
        CallbackTaskQueue callbackTaskQueue,
        ILogger<ReplyEventHandler> logger)
    {
        _stateManager = stateManager;
        _callbackTaskQueue = callbackTaskQueue;
        _logger = logger;
    }

    public async Task HandleAsync(ReplyEventData eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Entry ... {eventData.Context.Request.DeviceInfo.Schema.Station}-{eventData.Tag}_{eventData.State}");

        if (!_stateManager.CanTransfer(new StateKey(eventData.Context.Request.DeviceInfo.Schema.Station, eventData.Tag), eventData.State))
        {
            _logger.LogInformation($"Can not transfer ... {eventData.Context.Request.DeviceInfo.Schema.Station}-{eventData.Tag}_{eventData.State}");
            return;
        }

        // 向后处理
        _logger.LogInformation($"Can Next ... {eventData.Context.Request.DeviceInfo.Schema.Station}-{eventData.Tag}_{eventData.State}");

        await Task.Delay(new Random().Next(100, 5000));

        var variable = eventData.Context.Request.DeviceInfo.GetVariable(eventData.Tag);
        eventData.Context.Response.Values.Add(PayloadData.From(variable!, (short)0)); // 注：类型要与之类型对应，防止强制转换异常
        await _callbackTaskQueue.QueueAsync(eventData.Context);
    }
}
