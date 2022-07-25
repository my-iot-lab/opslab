using BootstrapAdmin.DataAccess.Models;
using BootstrapAdmin.Web.Core;

namespace BootStarpAdmin.DataAccess.FreeSql.Service;

class LoginService : ILogin
{
    private IFreeSql FreeSql { get; }

    public LoginService(IFreeSql freeSql) => FreeSql = freeSql;

    public bool Log(string userName, string? IP, string? OS, string? browser, string? address, string? userAgent, bool result)
    {
        var loginUser = new LoginLog()
        {
            UserName = userName,
            LoginTime = DateTime.Now,
            Ip = IP,
            City = address,
            OS = OS,
            Browser = browser,
            UserAgent = userAgent,
            Result = result ? "登录成功" : "登录失败"
        };
        FreeSql.Insert(loginUser).ExecuteAffrows();
        return true;
    }
}
