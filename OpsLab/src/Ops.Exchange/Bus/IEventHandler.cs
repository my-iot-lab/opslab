namespace Ops.Exchange.Bus;

/// <summary>
/// 事件处理器接口
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// 处理数据
    /// </summary>
    /// <param name="data">要处理的事件数据</param>
    void Handle(EventData data);
}
