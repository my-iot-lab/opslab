namespace Ops.Communication;

/// <summary>
/// Result is a type that represents either success (Ok) or failure (Err)
/// </summary>
/// <typeparam name="T">成功值类型</typeparam>
/// <typeparam name="E">错误值类型</typeparam>
public class Result<T, E>
{
    /// <summary>
    /// 成功值
    /// </summary>
    public T Ok { get; set; }

    /// <summary>
    /// 错误值
    /// </summary>
    public E Err { get; set; }
}

public class Result
{
    public void Ok<T>(T ok)
    {

    }

    public void Err<T>(T err)
    {

    }
}