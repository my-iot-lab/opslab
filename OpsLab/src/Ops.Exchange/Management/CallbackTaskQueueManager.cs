using Ops.Exchange.Common;
using Ops.Exchange.Model;

namespace Ops.Exchange.Management;

/// <summary>
/// 数据回调队列管理对象，用于向设备回写数据
/// </summary>
public sealed class CallbackTaskQueueManager : BackgroundTaskQueue<PayloadContext>
{
}
