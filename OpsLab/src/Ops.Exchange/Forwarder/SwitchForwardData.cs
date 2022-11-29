namespace Ops.Exchange.Forwarder;

/// <summary>
/// 基于开关的 ForwardData
/// </summary>
public sealed class SwitchForwardData
{
    /// <summary>
    /// 开关状态。根据开关连通状态，可以判定数据的来源以及区分数据的开始与结束。
    /// </summary>
    public SwitchState SwitchState { get; set; }

    /// <summary>
    /// 传递的数据。
    /// </summary>
    [NotNull]
    public ForwardData? Data { get; set; }

    public SwitchForwardData(SwitchState switchState, ForwardData? data)
    {
        SwitchState = switchState;
        Data = data;
    }

    public SwitchForwardData(int switchState, ForwardData? data)
    {
        SwitchState = switchState switch
        {
            -1 => SwitchState.Off,
            0 => SwitchState.Ready,
            1 => SwitchState.On,
            _ => throw new NotImplementedException(),
        };
        Data = data;
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
    /// 通电信号，表示开关连通中。
    /// </summary>
    On,

    /// <summary>
    /// 终止状态，表示开关即将断开。
    /// </summary>
    Off,
}
