namespace Ops.Exchange.Address;

/// <summary>
/// 地址标识
/// </summary>
public enum AddressFlag
{
    /// <summary>
    /// 常规地址。
    /// 只会随 <see cref="Trigger"/> 改变而附加相应的数
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 表示该地址触发数据发送。
    /// 该地址值改变时，会触发数据发送行为。
    /// </summary>
    Trigger = 1,

    /// <summary>
    /// 表示该地址用于心跳。
    /// </summary>
    Heartbeat = 2,

    /// <summary>
    /// 表示该地址数据会不间断发送。
    /// </summary>
    Interval = 3,
}
