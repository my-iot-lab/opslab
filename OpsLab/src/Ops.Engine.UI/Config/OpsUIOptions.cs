namespace Ops.Engine.UI.Config;

public sealed class OpsUIOptions
{
    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    public Api Api { get; set; }
}

public sealed class Api
{
    public string? BaseAddress { get; set; }
}
