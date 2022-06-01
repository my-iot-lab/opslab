using System.Collections.Concurrent;

namespace Ops.Exchange.Bus;

/// <summary>
/// 状态 Key 值
/// </summary>
/// <param name="Key">Key 值</param>
public record class EventStateKey(string Key)
{
}

/// <summary>
/// 跳变事件状态管理，用于检测有跳变的事件。
/// </summary>
public sealed class TransitionEventState
{
    private readonly ConcurrentDictionary<EventStateKey, ConcurrentDictionary<string, EventState>> _states = new();

    /// <summary>
    /// 获取当前站点的所有状态
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ICollection<EventState> Get(EventStateKey key)
    {
        if (_states.TryGetValue(key, out var result))
        {
            return result.Values;
        }
        return Array.Empty<EventState>();
    }

    /// <summary>
    /// 检查是否能下发。
    /// </summary>
    /// <returns></returns>
    public bool CanTransfer(EventStateKey key, EventState state)
    {
        if (_states.TryGetValue(key, out var map))
        {
            if (map.TryGetValue(state.Tag, out var state2))
            {
                // 校验状态
                state2.Update(state.State);
            }
            else
            {
                map.TryAdd(state.Tag, state);
            }
        }
        else
        {
            // 状态集合中还不存在该站点状态数据时添加。
            ConcurrentDictionary<string, EventState> state3 = new();
            state3[state.Tag] = state;
            _states.TryAdd(key, state3);
        }

        // 校验状态
        return state.State == 1;
    }

    /// <summary>
    /// 移除指定的任务状态
    /// </summary>
    /// <param name="key"></param>
    public void Remove(EventStateKey key)
    {
        _states.TryRemove(key, out var _);
    }

    /// <summary>
    /// 重置所有状态。
    /// </summary>
    public void Reset()
    {
        _states.Clear();
    }
}
