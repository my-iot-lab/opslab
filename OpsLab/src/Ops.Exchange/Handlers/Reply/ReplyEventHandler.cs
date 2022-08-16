using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ops.Exchange.Bus;
using Ops.Exchange.Forwarder;
using Ops.Exchange.Management;
using Ops.Exchange.Stateless;

namespace Ops.Exchange.Handlers.Reply;

/// <summary>
/// 基于响应的事件处理。
/// <para>请求响应事件</para>
/// </summary>
internal sealed class ReplyEventHandler : IEventHandler<ReplyEventData>
{
    private readonly TriggerStateManager _stateManager;
    private readonly CallbackTaskQueueManager _callbackTaskQueueManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public ReplyEventHandler(TriggerStateManager stateManager,
        CallbackTaskQueueManager callbackTaskQueueManager,
        IServiceProvider serviceProvider,
        ILogger<ReplyEventHandler> logger)
    {
        _stateManager = stateManager;
        _callbackTaskQueueManager = callbackTaskQueueManager;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task HandleAsync(ReplyEventData eventData, CancellationToken cancellationToken = default)
    {
        var schema = eventData.Context.Request.DeviceInfo.Schema;
        var stateKey = new StateKey(schema.Station, eventData.Tag);
        if (!_stateManager.CanTransfer(stateKey, eventData.State))
        {
            return;
        }

        short newState = ExStatusCode.None;
        try
        {
            ReplyResult replyResult;
            ForwardData forwardData = new(eventData.Context.Request.RequestId, schema, eventData.Tag, eventData.Name, eventData.Values);

            // 采用 Scope 作用域
            var replyForwarder = _serviceProvider.GetRequiredService<IReplyForwarder>();

            if (eventData.HandleTimeout > 0)
            {
                CancellationTokenSource cts2 = new(eventData.HandleTimeout);
                using var cts0 = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts2.Token);

                replyResult = await replyForwarder.ExecuteAsync(forwardData, cts0.Token);
            }
            else
            {
                replyResult = await replyForwarder.ExecuteAsync(forwardData, cancellationToken);
            }

            foreach (var item in replyResult.Values)
            {
                eventData.Context.SetResponseValue(item.Key, item.Value);
            }

            newState = replyResult.Result;
        }
        catch (OperationCanceledException)
        {
            newState = ExStatusCode.HandlerTimeout;

            _logger.LogError("[ReplyEventHandler] 任务超时取消 -- RequestId：{0}，工站：{1}，触发点：{2}",
                eventData.Context.Request.RequestId,
                schema.Station,
                eventData.Tag);
        }
        catch (Exception ex)
        {
            newState = ExStatusCode.HandlerException;

            _logger.LogError(ex, "[ReplyEventHandler] 任务异常 -- RequestId：{0}，工站：{1}，触发点：{2}",
                eventData.Context.Request.RequestId,
                schema.Station,
                eventData.Tag);
        }
        finally
        {
            // 保证触发状态与回写状态一致
            eventData.Context.Response.LastDelegate = () =>
            {
                _stateManager.Change(stateKey, newState);
            };

            // 需保证触发的 Tag 是最后一个回写的
            eventData.Context.SetResponseValue(eventData.Tag, newState);
        }

        await _callbackTaskQueueManager.QueueAsync(eventData.Context, cancellationToken);
    }
}
