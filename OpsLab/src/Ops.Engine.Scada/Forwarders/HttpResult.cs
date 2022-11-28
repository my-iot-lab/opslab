using System.Collections.Generic;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.Scada.Forwarders;

/// <summary>
/// HTTP 响应结果
/// </summary>
public class HttpResult
{
    /// <summary>
    /// 1 表示 OK
    /// </summary>
    public int Code { get; set; }

    public string Message { get; set; } = "";

    public IReadOnlyDictionary<string, object> Values { get; set; } = new Dictionary<string, object>(0);

    public bool IsOk()
    {
        return Code == 1;
    }

    /// <summary>
    /// 转换为 <see cref="ReplyResult"/> 对象
    /// </summary>
    /// <returns></returns>
    public ReplyResult ToReplyResult()
    {
        return new ReplyResult
        {
            Result = (short)(Code == 1 ? 0 : Code),
            Values = Values,
        };
    }
}
