using Ops.Exchange.Forwarder;

namespace Ops.Host.Core.Utils;

public static class ReplyResultHelper
{
    public static ReplyResult Ok(IDictionary<string, object>? values = null)
    {
        return From(0, values);
    }

    public static ReplyResult Error(short code = 2)
    {
        return From(code);
    }

    public static ReplyResult From(short code, IDictionary<string, object>? values = null)
    {
        var resp = new ReplyResult()
        {
            Result = code,
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
