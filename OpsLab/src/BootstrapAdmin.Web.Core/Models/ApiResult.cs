using BootstrapAdmin.DataAccess.Models;

namespace BootstrapAdmin.Web.Core.Models;

/// <summary>
/// Api 调用返回结果
/// </summary>
public sealed class ApiResult
{
    /// <summary>
    /// 状态码，1 表示成功。
    /// 注：状态码不要设置为 0。
    /// </summary>
    public int Code { get; set; } = ErrorCode.Success;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

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

    public static ApiResult Ok(IDictionary<string, object>? values = null)
    {
        return From(ErrorCode.Success, values);
    }

    public static ApiResult Error(string message)
    {
        return Error(err: message);
    }

    public static ApiResult Error(int code = ErrorCode.Error, string? err = null)
    {
        return new ApiResult
        {
            Code = code,
            Message = err ?? ErrorCode.GetDescription(code),
        };
    }

    /// <summary>
    /// 转换为 <see cref="ApiResult"/> 对象
    /// </summary>
    /// <param name="code">状态</param>
    /// <param name="values">要回写的数据集合</param>
    /// <returns></returns>
    public static ApiResult From(int code, IDictionary<string, object>? values = null)
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
}
