namespace Ops.Contrib.Kepware.IotGateway.RESTful;

public sealed class RESTfulOptions
{
    /// <summary>
    /// REST 服务端基地址。
    /// </summary>
    [NotNull]
    public string? RESTServerBaseAddress { get; set; }

    /// <summary>
    /// 是否允许匿名访问，默认为true。
    /// </summary>
    public bool AllowAnonymous { get; set; } = true;

    /// <summary>
    /// 凭证路径。
    /// </summary>
    public string? CertificatePath { get; set; }
}
