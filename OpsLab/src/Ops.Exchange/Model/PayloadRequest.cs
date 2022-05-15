using Ops.Exchange.Utils;

namespace Ops.Exchange.Model;

/// <summary>
/// 请求数据。
/// <para>从设备中读取，用于发送给应用程序。</para>
/// </summary>
internal sealed class PayloadRequest
{
    /// <summary>
    /// 用于请求追踪的唯一 Id。
    /// </summary>
    public string RequestId { get; } = GuidIdGenerator.NextId();

    /// <summary>
    /// 请求的值集合。
    /// </summary>
    public List<PayloadData> Values { get; } = new();
}
