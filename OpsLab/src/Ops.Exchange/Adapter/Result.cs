namespace Ops.Exchange.Adapter;

/// <summary>
/// 表示返回结果，要么成功要么失败。
/// </summary>
/// <typeparam name="T">成功值类型</typeparam>
/// <typeparam name="E">错误值类型</typeparam>
public class Result<T, E>
{
    /// <summary>
    /// 成功值
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// 错误值
    /// </summary>
    public E? Error { get; set; }

    /// <summary>
    /// 返回结果是否 OK（Error 不为 null）
    /// </summary>
    /// <returns></returns>
    public bool IsOk()
    {
        return Error is null;
    }
}

/// <summary>
/// 表示有返回值的返回结果，要么成功要么失败。
/// </summary>
/// <typeparam name="T">成功值类型</typeparam>
public class Result<T> : Result<T, Exception>
{
}

/// <summary>
/// 表示没有返回值的返回结果，可检测返回是否有异常。
/// </summary>
public class Result : Result<object, Exception>
{
    new object? Value { get; set; }
}
