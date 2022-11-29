using Ops.Exchange.Bus;
using Ops.Exchange.Forwarder;

namespace Ops.Exchange.Handlers.Switch;

/// <summary>
/// 基于开关的事件处理。
/// <para>仅有请求的事件</para>
/// </summary>
internal sealed class SwitchEventHandler : IEventHandler<SwitchEventData>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public SwitchEventHandler(IServiceProvider serviceProvider, ILogger<SwitchEventHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task HandleAsync(SwitchEventData eventData, CancellationToken cancellationToken = default)
    {
        var forwardData = new ForwardData(eventData.RequestId, eventData.Schema, eventData.Tag, eventData.Name, eventData.Values.ToArray());
        var forwardData2 = new SwitchForwardData(eventData.State, forwardData);

        try
        {
            var switchForwarder = _serviceProvider.GetRequiredService<ISwitchForwarder>();

            if (eventData.HandleTimeout > 0)
            {
                CancellationTokenSource cts2 = new(eventData.HandleTimeout);
                using var cts0 = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts2.Token);

                await switchForwarder.ExecuteAsync(forwardData2, cts0.Token);
            }
            else
            {
                await switchForwarder.ExecuteAsync(forwardData2, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("[SwitchEventHandler] 任务超时取消 -- RequestId：{0}，工站：{1}，触发点：{2}",
                eventData.RequestId,
                eventData.Schema.Station,
                eventData.Tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[SwitchEventHandler] 任务异常 -- RequestId：{0}，工站：{1}，触发点：{2}",
                eventData.RequestId,
                eventData.Schema.Station,
                eventData.Tag);
        }
    }
}
