namespace Ops.Exchange.Stateless;

/// <summary>
/// 触发状态管理对象
/// </summary>
public sealed class TriggerStateManager
{
    private readonly ConcurrentDictionary<StateKey, StateTable> _states = new();

    /// <summary>
    /// 获取当前站点的所有状态
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IReadOnlyCollection<StateEntry> Get(StateKey key)
    {
        if (_states.TryGetValue(key, out var result))
        {
            return result.States;
        }

        return Array.Empty<StateEntry>();
    }

    /// <summary>
    /// 检查是否能下发。如果能下发，会顺便更改至对应的状态。
    /// </summary>
    /// <param name="state">要检查的状态</param>
    /// <returns></returns>
    public bool CanTransfer(StateKey key, int state)
    {
        if (_states.TryGetValue(key, out var map))
        {
            var entry = map[key.Tag];
            if (entry != null)
            {
                return entry.CheckAndChange(state);
            }
            else
            {
                var entry2 = CreateStateEntry(key.Tag, state);
                map.AddState(entry2);
            }
        }
        else
        {
            var entry3 = CreateStateEntry(key.Tag, state);

            StateTable state3 = new(key.Group);
            state3.AddState(entry3);
            _states.TryAdd(key, state3);
        }

        return state == ExStatusCode.Trigger;
    }

    /// <summary>
    /// 更改为指定的状态
    /// </summary>
    /// <param name="key">Key值</param>
    /// <param name="newState">新状态</param>
    public void Change(StateKey key, int newState)
    {
        if (_states.TryGetValue(key, out var map))
        {
            var entry = map[key.Tag];
            entry?.Change(newState);
        }
    }

    /// <summary>
    /// 移除指定的任务状态
    /// </summary>
    /// <param name="key"></param>
    public void Remove(StateKey key)
    {
        _states.TryRemove(key, out _);
    }

    /// <summary>
    /// 重置所有状态。
    /// </summary>
    public void Clear()
    {
        _states.Clear();
    }

    private static StateEntry CreateStateEntry(string tag, int initState)
    {
        var entry0 = new StateEntry(tag, initState, (entry, newState) =>
        {
            // 新旧状态不等，且新状态值为 1。
            return entry.State != newState && newState == ExStatusCode.Trigger;
        });

        return entry0;
    }
}
