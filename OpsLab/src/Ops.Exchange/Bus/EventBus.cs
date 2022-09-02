namespace Ops.Exchange.Bus;

/// <summary>
/// 事件总线。
/// </summary>
public sealed class EventBus
{
    /// <summary>
    /// 定义线程安全集合
    /// </summary>
    private readonly ConcurrentDictionary<Type, List<Type>> _eventAndHandlerMapping = new();

    private readonly IServiceProvider _serviceProvider;

    public EventBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 手动绑定事件源与事件处理。
    /// </summary>
    public void Register<TEventData, TEventHandler>()
        where TEventData : IEventData
        where TEventHandler : IEventHandler
    {
        if (_eventAndHandlerMapping.TryGetValue(typeof(TEventData), out var handlerTypes))
        {
            if (!handlerTypes.Contains(typeof(TEventHandler)))
            {
                handlerTypes.Add(typeof(TEventHandler));
            }
        }
        else
        {
            handlerTypes = new List<Type> { typeof(TEventHandler) };
            _eventAndHandlerMapping[typeof(TEventData)] = handlerTypes;
        }
    }

    /// <summary>
    /// 手动解除事件源与事件处理的绑定。
    /// </summary>
    public void UnRegister<TEventData, TEventHandler>()
        where TEventData : IEventData
        where TEventHandler : IEventHandler
    {
        if (_eventAndHandlerMapping.TryGetValue(typeof(TEventData), out var handlerTypes))
        {
            if (handlerTypes.Contains(typeof(TEventHandler)))
            {
                handlerTypes.Remove(typeof(TEventHandler));
            }
        }
    }

    /// <summary>
    /// 根据事件源触发绑定的事件。
    /// </summary>
    /// <typeparam name="TEventData"></typeparam>
    /// <param name="eventData"></param>
    public async Task TriggerAsync<TEventData>(TEventData eventData, CancellationToken cancellationToken = default)
        where TEventData : IEventData
    {
        if (_eventAndHandlerMapping.TryGetValue(typeof(TEventData), out var handlerTypes))
        {
            if (handlerTypes.Any())
            {
                foreach (var handlerType in handlerTypes)
                {
                    // 通过依赖注入来解析事件处理对象
                    var handler = _serviceProvider.GetService(handlerType);
                    if (handler is IEventHandler<TEventData> handler0)
                    {
                        await handler0.HandleAsync(eventData, cancellationToken);
                    }
                }
            }
        }
    }
}
