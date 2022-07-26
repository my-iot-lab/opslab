using BootstrapAdmin.DataAccess.Models;
using BootstrapAdmin.Web.Core;

namespace BootStarpAdmin.DataAccess.FreeSql.Services;

/// <summary>
/// 
/// </summary>
public class TraceService : ITrace
{
    private IFreeSql FreeSql { get; }

    public TraceService(IFreeSql freeSql) => FreeSql = freeSql;

    public (IEnumerable<Trace> Items, int ItemsCount) GetAll(string? searchText, TraceFilter filter, int pageIndex, int pageItems, List<string> sortList)
    {
        var items = FreeSql.Select<Trace>();
        if (!string.IsNullOrEmpty(searchText))
        {
            items.Where("UserName Like @searchText or Ip Like @searchText or RequestUrl Like @searchText", new { searchText });
        }

        if (!string.IsNullOrEmpty(filter.UserName))
        {
            items.Where("UserName Like @UserName", new { filter.UserName });
        }

        if (!string.IsNullOrEmpty(filter.Ip))
        {
            items.Where("Ip Like @Ip", new { filter.Ip });
        }

        if (!string.IsNullOrEmpty(filter.RequestUrl))
        {
            items.Where("ErrorPage Like @RequestUrl", new { filter.RequestUrl });
        }

        items.Where("LogTime >= @0 and LogTime <= @1", new { filter.Star, filter.End });

        if (sortList.Any())
        {
            items.OrderBy(string.Join(", ", sortList));
        }
        else
        {
            items.OrderBy("Logtime desc");
        }

        var traces = items.Count(out var count).Page(pageIndex, pageItems).ToList();

        return (traces, Convert.ToInt32(count));
    }

    public void Log(Trace trace)
    {
        FreeSql.Insert(trace).ExecuteAffrows();
    }
}
