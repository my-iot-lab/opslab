﻿using Ops.Exchange.Forwarder;
using Ops.Host.Core.Const;
using Ops.Host.Core.Utils;

namespace Ops.Host.Core.Services;

internal sealed class ArchiveService : IArchiveService
{
    private readonly IFreeSql _freeSql;

    public ArchiveService(IFreeSql freeSql) => _freeSql = freeSql;

    public async Task<ReplyResult> HandleAsync(ForwardData data)
    {
        await Task.Delay(100); // test

        var sn = data.GetString(PlcSymbolTag.PLC_Archive_SN); // SN
        var pass = data.GetInt(PlcSymbolTag.PLC_Archive_Pass); // 结果
        var ct = data.GetInt(PlcSymbolTag.PLC_Archive_Cycletime); // CT
        var @operator = data.GetString(PlcSymbolTag.PLC_Archive_Operator); // 操作人
        var shift = data.GetInt(PlcSymbolTag.PLC_Archive_Shift); // 班次
        var pallet = data.GetString(PlcSymbolTag.PLC_Archive_Pallet); // 托盘号

        if (string.IsNullOrWhiteSpace(sn))
        {
            return ReplyResultHelper.Ok();
        }

        // 记录进站信息
        try
        {
            // 按位解析

            // 主数据

            // 明细数据
            foreach (var item in data.Values.Where(s => s.IsAdditional))
            {
               
            }

            // 工站状态统计

            return ReplyResultHelper.Ok();
        }
        catch(Exception)
        {
            return ReplyResultHelper.Error();
        }
    }
}
