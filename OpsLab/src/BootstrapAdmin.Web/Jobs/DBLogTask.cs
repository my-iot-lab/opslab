using System.Collections.Concurrent;
using BootstrapAdmin.DataAccess.Models;
using Longbow.Tasks;

namespace BootstrapAdmin.Web.Jobs;

/// <summary>
/// 数据库脚本执行日志任务实体类
/// </summary>
public class DBLogTask : ITask
{
    private static readonly BlockingCollection<DBLog> _messageQueue = new(new ConcurrentQueue<DBLog>());

    /// <summary>
    /// 添加数据库日志实体类到内部集合中
    /// </summary>
    /// <param name="log"></param>
    public static Task AddDBLog(DBLog log) => Task.Run(() =>
    {
        if (!_messageQueue.IsAddingCompleted && !_pause)
        {
            _messageQueue.Add(log);
        }
    });

    private static bool _pause;
    /// <summary>
    /// 暂停接收脚本执行日志
    /// </summary>
    public static void Pause() => _pause = true;

    /// <summary>
    /// 开始接收脚本执行日志
    /// </summary>
    public static void Run() => _pause = false;

    /// <summary>
    /// 任务执行方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task Execute(CancellationToken cancellationToken)
    {
        var logs = new List<DBLog>();
        while (_messageQueue.TryTake(out var log))
        {
            if (log != null) logs.Add(log);
            if (logs.Count >= 100) break;
        }

        if (logs.Any())
        {
            //using var db = DbManager.Create(enableLog: false);
            //db.InsertBatch(logs);
        }
        return Task.CompletedTask;
    }
}
