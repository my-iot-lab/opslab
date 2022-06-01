namespace Ops.Exchange.Bus;

/// <summary>
/// 事件状态
/// </summary>
public sealed class EventState
{
    /// <summary>
    /// 事件标签 Tag（唯一）
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// 任务处理标识。
    /// 0 --> 初始状态;
    /// 1 --> 已就绪，待被处理;
    /// 2 ~ N --> 其他状态。
    /// <para>只有为 1 时才能触发数据推送到下游处理。</para>
    /// </summary>
    public int State { get; private set; }

    /// <summary>
    /// 状态最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    public EventState(string tag)
    {
        Tag = tag;
        UpdatedAt = DateTime.Now;
    }

    public void Update(int state)
    {
        if (State != state)
        {
            State = state;
            UpdatedAt = DateTime.Now;
        }
    }
}
