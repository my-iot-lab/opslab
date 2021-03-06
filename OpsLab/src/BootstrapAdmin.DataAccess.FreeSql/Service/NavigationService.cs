using BootStarpAdmin.DataAccess.FreeSql.Models;
using BootstrapAdmin.Caching;
using BootstrapAdmin.DataAccess.Models;
using BootstrapAdmin.Web.Core;

namespace BootStarpAdmin.DataAccess.FreeSql.Service;

class NavigationService : INavigation
{
    private const string NavigationServiceGetAllCacheKey = "NavigationService-GetAll";

    private const string NavigationServiceGetMenusByRoleIdCacheKey = "NavigationService-GetMenusByRoleId";

    private IFreeSql FreeSql { get; }

    public NavigationService(IFreeSql freeSql) => FreeSql = freeSql;

    public List<Navigation> GetAllMenus(string userName) => CacheManager.GetOrAdd($"{NavigationServiceGetAllCacheKey}-{userName}", entry =>
    {
        return FreeSql.Ado.Query<Navigation>($"select n.ID, n.ParentId, n.Name, n.Order, n.Icon, n.Url, n.Category, n.Target, n.IsResource, n.Application, ln.Name as ParentName from Navigations n inner join Dicts d on n.Category = d.Code and d.Category = @Category left join Navigations ln on n.ParentId = ln.ID inner join (select nr.NavigationID from Users u inner join UserRole ur on ur.UserID = u.ID inner join NavigationRole nr on nr.RoleID = ur.RoleID where u.UserName = @UserName union select nr.NavigationID from Users u inner join UserGroup ug on u.ID = ug.UserID inner join RoleGroup rg on rg.GroupID = ug.GroupID inner join NavigationRole nr on nr.RoleID = rg.RoleID where u.UserName = @UserName union select n.ID from Navigations n where EXISTS (select UserName from Users u inner join UserRole ur on u.ID = ur.UserID inner join Roles r on ur.RoleID = r.ID where u.UserName = @UserName and r.RoleName = @RoleName)) nav on n.ID = nav.NavigationID ORDER BY n.Application, n.Order", new { UserName = userName, Category = "菜单", RoleName = "Administrators" });
    });

    public List<string> GetMenusByRoleId(string? roleId) => CacheManager.GetOrAdd($"{NavigationServiceGetMenusByRoleIdCacheKey}-{roleId}", entry => FreeSql.Ado.Query<string>("select NavigationID from NavigationRole where RoleID = @roleId", new { roleId }));

    public bool SaveMenusByRoleId(string? roleId, List<string> menuIds)
    {
        var ret = false;
        try
        {
            FreeSql.Transaction(() =>
            {
                FreeSql.Ado.ExecuteNonQuery("delete from NavigationRole where RoleID = @roleId", new { roleId });
                FreeSql.Insert(menuIds.Select(g => new NavigationRole { NavigationID = g, RoleID = roleId })).ExecuteAffrows();
                ret = true;
            });
        }
        catch (Exception)
        {
            throw;
        }

        if (ret)
        {
            CacheManager.Clear();
        }

        return ret;
    }
}
