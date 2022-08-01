using System.Threading.Channels;
using Ops.Engine.Scada.Models;

namespace Ops.Engine.Scada.Managements;

/// <summary>
/// 定义全局 Channel 对象。
/// </summary>
public sealed class GlobalChannelManager
{
    public static readonly GlobalChannelManager Defalut = new();

    private GlobalChannelManager()
    {}

    /// <summary>
    /// 用于界面显示日志的 Channel。
    /// </summary>
    public Channel<LogMessageModel> LogMessageChannel = Channel.CreateBounded<LogMessageModel>(32);
}
