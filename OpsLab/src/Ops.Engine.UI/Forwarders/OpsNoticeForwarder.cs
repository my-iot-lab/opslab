using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ops.Engine.UI.Domain.Services;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Forwarders;

internal sealed class OpsNoticeForwarder : INoticeForwarder
{
    private readonly INoticeService _noticeService;
    private readonly ILogger _logger;

    public OpsNoticeForwarder(
        INoticeService noticeService,
        ILogger<OpsNoticeForwarder> logger)
    {
        _noticeService = noticeService;
        _logger = logger;
    }

    public async Task ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Notice] RequestId：{0}，工站：{1}, 触发点：{2}，数据：{3}",
                data.RequestId,
                data.Schema.Station,
                data.Tag,
                string.Join("; ", data.Values.Select(s => $"{s.Tag}={s.Value}")));

        // 执行通知
        await _noticeService.ExecuteAsync(data, cancellationToken);
    }

    public void Dispose()
    {
        _noticeService.Dispose();
    }
}
