using System.Collections.Concurrent;

namespace Ops.Exchange.Scheduler;

/// <summary>
/// 状态 Key 值
/// </summary>
/// <param name="Line">线体</param>
/// <param name="Station">站点</param>
public record class TaskStateKey(string Line, int Station)
{
}

/// <summary>
/// 任务状态管理，用于检测任务的跳变。
/// </summary>
public class TaskStateTransfer
{
    private readonly ConcurrentDictionary<TaskStateKey, ConcurrentDictionary<string, TaskState>> _states = new();

    /// <summary>
    /// 获取当前站点的所有状态
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ICollection<TaskState> Get(TaskStateKey key)
    {
        if (_states.TryGetValue(key, out var result))
        {
            return result.Values;
        }
        return Array.Empty<TaskState>();
    }

    /// <summary>
    /// 重置所有状态。
    /// </summary>
    public void Reset()
    {
        _states.Clear();
    }

    /// <summary>
    /// 检查是否能下发。
    /// </summary>
    /// <returns></returns>
    public bool CanTransfer(TaskStateKey key, TaskState state)
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
            ConcurrentDictionary<string, TaskState> state3 = new();
            state3[state.Tag] = state;
            _states.TryAdd(key, state3);
        }

        // 校验状态
        return state.State == 1;
    }
}
