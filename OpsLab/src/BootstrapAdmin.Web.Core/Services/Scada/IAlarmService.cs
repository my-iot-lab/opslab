using BootstrapAdmin.Web.Core.Models;

namespace BootstrapAdmin.Web.Core.Services;

public interface IAlarmService
{
    Task<ApiResult> SaveAlarmsAsync(ApiData data);
}
