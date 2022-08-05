using System;

namespace Ops.Host.Common.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// 将日期转换为当天的零点 00:00:00:000。
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime? ToDayMin(this DateTime? dateTime)
    {
        return dateTime?.Date;
    }

    /// <summary>
    /// 将日期转换为当天的 23:59:59:999。
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime? ToDayMax(this DateTime? dateTime)
    {
        return dateTime?.Date.AddMilliseconds(86_399_999); // 24 * 3600 * 1000 - 1
    }
}
