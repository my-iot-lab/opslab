namespace Ops.Host.Core.Management;

/// <summary>
/// 消息分类
/// </summary>
public sealed class MessageClassify
{
    /// <summary>
    /// 警报
    /// </summary>
    public const string Alarm = "Alarm";

    /// <summary>
    /// 点检
    /// </summary>
    public const string SpotCheck = "SpotCheck";

    /// <summary>
    /// 能耗
    /// </summary>
    public const string Energy = "Energy";

    /// <summary>
    /// 产品进站
    /// </summary>
    public const string Inbound = "Inbound";

    /// <summary>
    /// 产品出站
    /// </summary>
    public const string Outbound = "Outbound";
}
