using System;

namespace Ops.Engine.Scada.Models;

/// <summary>
/// 日志消息 Model
/// </summary>
public sealed class LogMessageModel
{
    public DateTime CreatedTime { get; set; }

    public string? Line { get; set; }

    public string? Station { get; set; }

    public string? Tag { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
