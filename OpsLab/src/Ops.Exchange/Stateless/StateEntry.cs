namespace Ops.Exchange.Stateless;

/// <summary>
/// 状态数据条目。
/// </summary>
public sealed class StateEntry : IEquatable<StateEntry>
{
    /// <summary>
    /// 状态点位名称
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// 当前状态值
    /// </summary>
    public int State { get; private set; }

    /// <summary>
    /// 最后变更时间
    /// </summary>
    public DateTime ChangedAt { get; private set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// 是否可更改。
    /// <para>
    ///  参数1 --> 实体本身；
    ///  参数2 --> 要变更的状态。
    /// </para>
    /// </summary>
    public Func<StateEntry, int, bool> CanChange { get; }

    /// <summary>
    /// 状态更改后的处理事件
    /// </summary>
    public Action<StateChangedEventArgs>? OnChanged { get; set; }

    public StateEntry(string tag, int initState, Func<StateEntry, int, bool> canChange)
    {
        Tag = tag;
        State = initState;
        CanChange = canChange;
        ChangedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }

    /// <summary>
    /// 检查状态，若可变更，则进行状态变更。
    /// 当 <see cref="CanChange"/> 为 true 时才会进行更改。
    /// </summary>
    /// <param name="newState">变更后的状态</param>
    /// <returns>true 表示可更改，否则为 false</returns>
    public bool CheckAndChange(int newState)
    {
        UpdatedAt = DateTime.Now;

        if (!CanChange(this, newState))
        {
            return false;
        }

        Change(newState);

        return true;
    }

    /// <summary>
    /// 变更状态。
    /// </summary>
    /// <param name="newState">变更后的状态</param>
    public void Change(int newState)
    {
        var oldState = State;
        State = newState;
        ChangedAt = DateTime.Now;
        UpdatedAt = ChangedAt;

        OnChanged?.Invoke(new StateChangedEventArgs(Tag, oldState, newState));
    }

    #region override

    public bool Equals(StateEntry? other)
    {
        return other != null && Tag == other.Tag;
    }

    public override bool Equals(object? obj)
    {
        return obj is StateEntry obj2 && Equals(obj2);
    }

    public override int GetHashCode()
    {
        return Tag.GetHashCode();
    }

    #endregion
}
