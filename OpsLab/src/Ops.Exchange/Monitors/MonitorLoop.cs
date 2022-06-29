using Microsoft.Extensions.Logging;

namespace Ops.Exchange.Monitors;

/// <summary>
/// 监听轮询器
/// </summary>
public sealed class MonitorLoop
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<MonitorLoop> _logger;
    private readonly CancellationToken _cancellationToken = new();

    public MonitorLoop(IBackgroundTaskQueue taskQueue, ILogger<MonitorLoop> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    /// <summary>
    /// 启动监听
    /// </summary>
    public void Start()
    {
        _logger.LogInformation($"{nameof(MonitorAsync)} loop is starting.");

        // Run a console user input loop in a background thread
        Task.Run(async () => await MonitorAsync());
    }

    private async ValueTask MonitorAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            var keyStroke = Console.ReadKey();
            if (keyStroke.Key == ConsoleKey.W)
            {
                // Enqueue a background work item
                await _taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItemAsync);
            }
        }
    }

    private async ValueTask BuildWorkItemAsync(CancellationToken token)
    {
        // Simulate three 5-second tasks to complete
        // for each enqueued work item

        int delayLoop = 0;
        var guid = Guid.NewGuid();

        _logger.LogInformation("Queued work item {Guid} is starting.", guid);

        while (!token.IsCancellationRequested && delayLoop < 3)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if the Delay is cancelled
            }

            ++delayLoop;

            _logger.LogInformation("Queued work item {Guid} is running. {DelayLoop}/3", guid, delayLoop);
        }
    }
}
