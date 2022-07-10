namespace Ops.Exchange.Utils;

/// <summary>
/// 提供一个任务调度程序，确保在 ThreadPool 之上运行时的最大并发级别。
/// </summary>
public sealed class LimitingScheduler
{
    private static readonly TaskFactory kFactory;
    private static readonly LimitingTaskScheduler kScheduler;

    public TaskFactory Factory => kFactory;

    static LimitingScheduler()
    {
        kScheduler = new LimitingTaskScheduler(Environment.ProcessorCount);
        kFactory = new TaskFactory(CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskContinuationOptions.None,
                kScheduler);
    }

    public void Dump(Action<Task> logger)
    {
        kScheduler?.Dump(logger);
    }

    /// <summary>
    /// 调度器实现。
    /// </summary>
    private sealed class LimitingTaskScheduler : TaskScheduler
    {
        // 当前线程是否正在处理工作项。
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        private readonly LinkedList<Task> _tasks = new();
        private readonly int _maxDegreeOfParallelism;
        private int _delegatesQueuedOrRunning;

        /// <summary>
        /// 获取此调度程序支持的最大并发级别。
        /// </summary>
        public sealed override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;

        /// <summary>
        /// 初始化 TaskScheduler 的一个实例具有指定并行度的类。
        /// </summary>
        /// <param name="maxDegreeOfParallelism">
        /// 此调度程序提供的最大并行度。一般设置为 OS 的内核数。
        /// </param>
        public LimitingTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            }
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        protected sealed override void QueueTask(Task task)
        {
            // 将任务添加到要处理的任务列表中。
            // 如果当前排队或运行的委托不足以处理任务，需安排另一个。
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        /// <summary>
        /// 通知 ThreadPool 有要为此调度程序执行的工作。
        /// </summary>
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // 注意，当前线程现在正在处理工作项。
                // 这对于启用将任务内联到此线程中是必要的。
                _currentThreadIsProcessingItems = true;
                try
                {
                    // 处理队列中所有可用的项目。
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // 当没有更多要处理的项目时，请注意这里已完成处理，然后离开。
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // 从队列中获取下一个要处理的项。
                            item = _tasks.First!.Value;
                            _tasks.RemoveFirst();
                        }

                        // 执行上面取出的任务。
                        TryExecuteTask(item);
                    }
                }
                finally
                {
                    // 已经完成了当前线程上的项目处理。
                    _currentThreadIsProcessingItems = false;
                }
            }, null);
        }

        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // 如果此线程尚未处理任务，这里不支持内联。
            if (!_currentThreadIsProcessingItems)
            {
                return false;
            }

            // 如果任务先前已排队，则将其从队列中删除。
            if (taskWasPreviouslyQueued)
            {
                TryDequeue(task);
            }

            // 尝试允许任务。
            return TryExecuteTask(task);
        }

        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks)
            {
                return _tasks.Remove(task);
            }
        }

        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken)
                {
                    return _tasks.ToArray();
                }

                throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_tasks);
                }
            }
        }

        /// <summary>
        /// Dump 调度器。
        /// </summary>
        /// <param name="logger"></param>
        internal void Dump(Action<Task> logger)
        {
            foreach (var task in GetScheduledTasks())
            {
                logger(task);
            }
        }
    }
}
