using Ops.Exchange.Bus;
using Ops.Exchange.Model;

namespace Ops.Exchange.Handlers.Switch;

/// <summary>
/// 开关事件数据。
/// </summary>
public sealed class SwitchEventData : EventData
{
    public const int StateReady = 0;

    public const int StateOn = 1;

    public const int StateOff = -1;

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
    /// 开关连通状态。根据开关连通状态，可以判定数据的来源以及区分数据的开始与结束。
    /// <para>-1: 终止状态</para>
    /// <para>0: 启动状态</para>
    /// <para>1: 通电状态</para>
    /// </summary>
    public int State { get; set; }

    /// <summary>
    /// 处理任务超时时间，单位毫秒。
    /// </summary>
    public int HandleTimeout { get; set; }

    public SwitchEventData(string requestId, DeviceSchema schema, string tag, string? name, int state, List<PayloadData>? values = default)
    {
        RequestId = requestId;
        Schema = schema;
        Tag = tag;
        Name = name;
        State = state;
        Values = values ?? new(0);
    }
}
