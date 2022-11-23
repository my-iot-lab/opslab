using Ops.Exchange.Bus;
using Ops.Exchange.Model;

namespace Ops.Exchange.Handlers.Switch;

/// <summary>
/// 开关事件数据。
/// </summary>
public sealed class SwitchEventData : EventData
{
    /// <summary>
    /// 请求的 Id，可用于追踪数据。
    /// </summary>
    public string RequestId { get; }

    /// <summary>
    /// 设备 Schema 基础信息。
    /// </summary>
    public DeviceSchema Schema { get; }

    /// <summary>
    /// 事件标签 Tag（唯一）
    /// </summary>
    public string Tag { get; }
    
    /// <summary>
    /// 事件名称
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// 要通知的数据。
    /// </summary>
    public List<PayloadData> Values { get; }

    /// <summary>
    /// 开关连通状态。
    /// <para>根据开关连通状态，可以判定数据的来源以及区分数据的开始与结束。</para>
    /// </summary>
    public SwitchState State { get; set; }

    /// <summary>
    /// 处理任务超时时间，单位毫秒。
    /// </summary>
    public int HandleTimeout { get; set; }

    public SwitchEventData(string requestId, DeviceSchema schema, string tag, string? name, SwitchState state, List<PayloadData>? values = default)
    {
        RequestId = requestId;
        Schema = schema;
        Tag = tag;
        Name = name;
        State = state;
        Values = values ?? new(0);
    }
}

/// <summary>
/// 开关连通状态。
/// </summary>
public enum SwitchState
{
    /// <summary>
    /// 启动信号，表示开关刚闭合。
    /// </summary>
    Ready,

    /// <summary>
    /// 启动信号，表示开关连通中。
    /// </summary>
    On,

    /// <summary>
    /// 终止状态，表示开关已断开。
    /// </summary>
    Off,
}
