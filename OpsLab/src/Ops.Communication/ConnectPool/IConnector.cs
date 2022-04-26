namespace Ops.Communication.Infrastructure.ConnectPool;

/// <summary>
/// 连接池的接口，连接池的管理对象必须实现此接口
/// </summary>
public interface IConnector
{
	/// <summary>
	/// 指示当前的连接是否在使用
	/// </summary>
	bool IsUsing { get; set; }

	/// <summary>
	/// 唯一的GUID码
	/// </summary>
	string GuidToken { get; set; }

	/// <summary>
	/// 最近一次使用的时间
	/// </summary>
	DateTime LatestTime { get; set; }

	/// <summary>
	/// 打开连接
	/// </summary>
	void Open();

	/// <summary>
	/// 关闭并释放
	/// </summary>
	void Close();
}
