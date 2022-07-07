namespace Ops.Exchange.Forwarder;

/// <summary>
/// 响应结果
/// </summary>
public sealed class ReplyResult
{
    /// <summary>
    /// 返回的结果
    /// </summary>
    public short Result { get; set; }

    public IReadOnlyDictionary<string, object> Values { get; set; } = new Dictionary<string, object>(0);

    /// <summary>
    /// 添加回写数据值
    /// </summary>
    /// <param name="tag">标签值</param>
    /// <param name="value">要添加的值</param>
    public void AddValue(string tag, object value)
    {
        ((Dictionary<string, object>)Values).Add(tag, value);
    }
}
