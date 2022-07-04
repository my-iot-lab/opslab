namespace Ops.Engine.UI.Forwarders;

public class HttpResult
{
    public int Code { get; set; }

    public string Message { get; set; }

    public bool IsOk()
    {
        return Code == 0;
    }
}

public sealed class HttpResult<T> : HttpResult
{
    public T Data { get; set; }
}
