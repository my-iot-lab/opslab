namespace Ops.Exchange.Stateless;

/// <summary>
/// 状态表。
/// </summary>
public sealed class StateTable
{
    public StateTable(string name)
    {
        Name = name;
    }

    /// <summary>
    /// 获取状态值
    /// </summary>
    /// <param name="tag">状态点位标签</param>
    /// <returns>没找到则返回 null</returns>
    public StateEntry? this[string tag] => States.FirstOrDefault(s => s.Tag == tag);

    /// <summary>
    /// 状态表名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 状态集合
    /// </summary>
    public IReadOnlySet<StateEntry> States { get; } = new HashSet<StateEntry>();

    public void AddState(StateEntry entry)
    {
        ((HashSet<StateEntry>)States).Add(entry);
    }

    public void RemoveState(StateEntry entry)
    {
        ((HashSet<StateEntry>)States).Remove(entry);
    }

    public void ClearState()
    {
        ((HashSet<StateEntry>)States).Clear();
    }
}
