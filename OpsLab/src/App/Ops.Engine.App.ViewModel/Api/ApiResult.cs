using System.Collections.Generic;

namespace Ops.Engine.App.ViewModel.Api;

/// <summary>
/// Api 调用返回结果
/// </summary>
public class ApiResult
{
    /// <summary>
    /// 状态码，0 表示成功
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; }

    public virtual string GetJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }

    public static ApiResult CreateOK()
    {
        return new ApiResult();
    }

    public static ApiResult CreateError(int code, string message)
    {
        return new ApiResult { Code = code, Message = message };
    }
}

/// <summary>
/// Api 调用返回结果
/// </summary>
public sealed class ApiResult<T> : ApiResult
{
    /// <summary>
    /// 结果数据
    /// </summary>
    public T Data { get; set; }

    public override string GetJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }

    public static ApiResult<T> Create(int code, string message, T data)
    {
        return new ApiResult<T> { Code = code, Message = message, Data = data };
    }

    public static ApiResult<T> CreateOK(T data)
    {
        return new ApiResult<T> { Data = data };
    }
}

public sealed class ReplyResult
{
    /// <summary>
    /// 返回的结果
    /// </summary>
    public short Result { get; set; }

    public IReadOnlyDictionary<string, object> Values { get; } = new Dictionary<string, object>();

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