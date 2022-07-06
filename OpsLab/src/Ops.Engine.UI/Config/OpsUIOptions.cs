namespace Ops.Engine.UI.Config;

public sealed class OpsUIOptions
{
    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 后台 Web 服务文件路径（可设为相对路径）
    /// </summary>
    public string? WebServerEntryPath { get; set; }

    public Api Api { get; set; } = new();
}

public sealed class Api
{
    public string BaseAddress { get; set; } = "http://localhost:5000";
}
