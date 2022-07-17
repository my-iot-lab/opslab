namespace BootstrapAdmin.Web.Models;

/// <summary>
/// Api 调用返回结果
/// </summary>
public class ApiResult
{
    /// <summary>
    /// 状态码，1 表示成功。
    /// </summary>
    public short Code { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 要回执的数据集合。注意：回写的数据类型必须与地址变量定义的一致。
    /// </summary>
    public IReadOnlyDictionary<string, object> Values { get; } = new Dictionary<string, object>();

    /// <summary>
    /// 添加回写数据值。注意：回写的数据类型必须与地址变量定义的一致。
    /// </summary>
    /// <param name="tag">标签值</param>
    /// <param name="value">要添加的值</param>
    public void AddValue(string tag, object value)
    {
        ((Dictionary<string, object>)Values).Add(tag, value);
    }

    public static ApiResult Ok(IDictionary<string, object> values = null)
    {
        return From(1, values);
    }

    public static ApiResult From(short code, IDictionary<string, object> values = null)
    {
        var resp = new ApiResult()
        {
            Code = code,
        };

        if (values != null)
        {
            foreach (var item in values)
            {
                resp.AddValue(item.Key, item.Value);
            }
        }

        return resp;
    }

    public virtual string GetJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}
