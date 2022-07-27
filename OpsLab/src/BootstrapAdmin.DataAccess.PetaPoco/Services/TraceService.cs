using BootstrapAdmin.DataAccess.Models;
using BootstrapAdmin.Web.Core;
using PetaPoco;

namespace BootstrapAdmin.DataAccess.PetaPoco.Services;

class TraceService : ITrace
{
    private IDBManager DBManager { get; }

    public TraceService(IDBManager db)
    {
        DBManager = db;
    }

    public void Log(Trace trace)
    {
        using var db = DBManager.Create();
        db.Insert(trace);
    }

    public (IEnumerable<Trace> Items, int ItemsCount) GetAll(string? searchText, TraceFilter filter, int pageIndex, int pageItems, List<string> sortList)
    {
        var sql = new Sql();
        if (!string.IsNullOrEmpty(searchText))
        {
            sql.Where("UserName Like @0 or Ip Like @0 or RequestUrl Like @0", $"%{searchText}%");
        }

        if (!string.IsNullOrEmpty(filter.UserName))
        {
            sql.Where("UserName Like @0", $"%{filter.UserName}%");
        }

        if (!string.IsNullOrEmpty(filter.Ip))
        {
            sql.Where("Ip Like @0", $"%{filter.Ip}%");
        }

        if (!string.IsNullOrEmpty(filter.RequestUrl))
        {
            sql.Where("ErrorPage Like @0", $"%{filter.RequestUrl}%");
        }

        sql.Where("LogTime >= @0 and LogTime <= @1", filter.Star, filter.End);

        if (sortList.Any())
        {
            sql.OrderBy(string.Join(", ", sortList));
        }
        else
        {
            sql.OrderBy("Logtime desc");
        }

        using var db = DBManager.Create();
        var data = db.Page<Trace>(pageIndex, pageItems, sql);
        return (data.Items, Convert.ToInt32(data.TotalItems));
    }
}
