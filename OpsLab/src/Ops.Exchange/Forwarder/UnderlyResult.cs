namespace Ops.Exchange.Forwarder;

/// <summary>
/// 底层响应事件响应结果
/// </summary>
public sealed class UnderlyResult
{
    /// <summary>
    /// 要回写给 PLC 的其他值。
    /// <para>注：Key 必须在地址定义中有存在，值必须与地址定义的类型保持一致（包括数组）。</para>
    /// </summary>
    public IReadOnlyDictionary<string, object> Values { get; set; } = new Dictionary<string, object>(0);

    /// <summary>
    /// 添加回写数据值。
    /// <para>注：tag 必须在地址定义中有存在，value 值必须与地址定义的类型保持一致（包括数组）。</para>
    /// </summary>
    /// <param name="tag">标签值</param>
    /// <param name="value">要添加的值</param>
    public void AddValue(string tag, object value)
    {
        ((Dictionary<string, object>)Values).Add(tag, value);
    }
}
