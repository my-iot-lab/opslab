namespace Ops.Exchange.Model;

/// <summary>
/// 响应数据。
/// <para>用于将应用程序的数据写入到设备。</para>
/// </summary>
public sealed class PayloadResponse
{
    /// <summary>
    /// 响应的数据，该数据回写给设备。
    /// </summary>
    public List<PayloadData> Values { get; } = new();

    /// <summary>
    /// 数据回写完后的执行方法
    /// </summary>
    public Action? LastDelegate { get; set; }
}
