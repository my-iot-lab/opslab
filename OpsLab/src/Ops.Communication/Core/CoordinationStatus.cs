namespace Ops.Communication.Core;

/// <summary>
/// 线程的协调逻辑状态
/// </summary>
internal enum CoordinationStatus
{
	/// <summary>
	/// 所有项完成
	/// </summary>
	AllDone,

	/// <summary>
	/// 超时
	/// </summary>
	Timeout,

	/// <summary>
	/// 任务取消
	/// </summary>
	Cancel,
}
