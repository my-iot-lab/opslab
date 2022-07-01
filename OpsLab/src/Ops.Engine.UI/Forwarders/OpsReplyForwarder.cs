using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ops.Engine.UI.Domain.Services;
using Ops.Exchange.Forwarder;

namespace Ops.Engine.UI.Forwarders;

internal sealed class OpsReplyForwarder : IReplyForwarder
{
    private readonly IInboundService _inboundService;
    private readonly IOutboundService _outboundService;
    private readonly IMaterialScanningService _materialScanningService;
    private readonly ILogger _logger;

    public OpsReplyForwarder(
        IInboundService inboundService,
        IOutboundService outboundService,
        IMaterialScanningService materialScanningService,
        ILogger<OpsReplyForwarder> logger)
    {
        _inboundService = inboundService;
        _outboundService = outboundService;
        _materialScanningService = materialScanningService;
        _logger = logger;
    }

    public async Task<ReplyResult> ExecuteAsync(ForwardData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Reply] RequestId：{0}，工站：{1}, 触发点：{2}，数据：{3}",
                data.RequestId,
                data.Schema.Station,
                data.Tag,
                string.Join("; ", data.Values.Select(s => $"{s.Tag}={s.Value}")));

        // 派发数据
        return data.Tag switch
        {
            OpsSymbol.PLC_Sign_Inbound => await _inboundService.ExecuteAsync(data, cancellationToken),
            OpsSymbol.PLC_Sign_Outbound => await _outboundService.ExecuteAsync(data, cancellationToken),
            OpsSymbol.PLC_Sign_Critical_Material => await _materialScanningService.ScanCriticalAsync(data, cancellationToken),
            OpsSymbol.PLC_Sign_Batch_Material => await _materialScanningService.ScanBatchAsync(data, cancellationToken),
            _ => throw new System.NotImplementedException(),
        };
    }

    public void Dispose()
    {
        _inboundService.Dispose();
        _outboundService.Dispose();
        _materialScanningService.Dispose();
    }
}
