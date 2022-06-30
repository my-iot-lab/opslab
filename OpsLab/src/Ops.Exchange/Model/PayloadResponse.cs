namespace Ops.Exchange.Model;

/// <summary>
/// 响应数据。
/// <para>用于将应用程序的数据写入到设备。</para>
/// </summary>
public sealed class PayloadResponse
{
    public List<PayloadData> Values { get; } = new();
}
