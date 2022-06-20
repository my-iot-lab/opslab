namespace Ops.Exchange.Stateless;

/// <summary>
/// 状态更改后触发事件参数
/// </summary>
public sealed class StateChangedEventArgs : EventArgs
{
    public StateChangedEventArgs(string tag, int oldState, int newState)
    {
        Tag = tag;
        OldState = oldState;
        NewState = newState;
    }

    /// <summary>
    /// 状态标签
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// 旧状态值
    /// </summary>
    public int OldState { get; }

    /// <summary>
    /// 新状态值
    /// </summary>
    public int NewState { get; }
}
