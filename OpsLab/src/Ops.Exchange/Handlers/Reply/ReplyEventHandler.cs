using Microsoft.Extensions.Logging;
using Ops.Exchange.Bus;
using Ops.Exchange.Stateless;

namespace Ops.Exchange.Handlers.Reply;

/// <summary>
/// 基于响应的事件处理。
/// <para>请求响应事件</para>
/// </summary>
internal sealed class ReplyEventHandler : IEventHandler<ReplyEventData>
{
    private readonly StateManager _stateManager;
    private readonly ILogger _logger;

    public ReplyEventHandler(StateManager stateManager, ILogger<ReplyEventHandler> logger)
    {
        _stateManager = stateManager;
        _logger = logger;
    }

    public Task HandleAsync(ReplyEventData eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Entry ... {eventData.Schema.Station}-{eventData.Tag}_{eventData.State}");

        if (!_stateManager.CanTransfer(new StateKey(eventData.Schema.Station, eventData.Tag), eventData.State))
        {
            return Task.CompletedTask;
        }

        // 向后处理
        _logger.LogInformation($"Can Next ... {eventData.Schema.Station}-{eventData.Tag}_{eventData.State}");

        return Task.CompletedTask;
    }
}
