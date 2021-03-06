using BootStarpAdmin.DataAccess.FreeSql.Models;
using BootstrapAdmin.Caching;
using BootstrapAdmin.DataAccess.Models;
using BootstrapAdmin.Web.Core;

namespace BootStarpAdmin.DataAccess.FreeSql.Service;

class RoleService : IRole
{
    private const string RoleServiceGetAllCacheKey = "RoleService-GetAll";

    private const string RoleServiceGetRolesByUserIdCacheKey = "RoleService-GetRolesByUserId";

    private const string RoleServiceGetRolesByGroupIdCacheKey = "RoleService-GetRolesByGroupId";

    private const string RoleServiceGetRolesByMenuIdCacheKey = "RoleService-GetRolesByMenusId";

    private IFreeSql FreeSql { get; }

    public RoleService(IFreeSql freeSql) => FreeSql = freeSql;

    public List<Role> GetAll() => CacheManager.GetOrAdd(RoleServiceGetAllCacheKey, entry => FreeSql.Select<Role>().ToList());

    public List<string> GetRolesByGroupId(string? groupId) => CacheManager.GetOrAdd($"{RoleServiceGetRolesByGroupIdCacheKey}-{groupId}", entry => FreeSql.Ado.Query<string>("select RoleID from RoleGroup where GroupID = @groupId", new { groupId }));

    public List<string> GetRolesByMenuId(string? menuId) => CacheManager.GetOrAdd($"{RoleServiceGetRolesByMenuIdCacheKey}-{menuId}", entry => FreeSql.Ado.Query<string>("select RoleID from NavigationRole where NavigationID = @menuId", new { menuId }));

    public List<string> GetRolesByUserId(string? userId) => CacheManager.GetOrAdd($"{RoleServiceGetRolesByUserIdCacheKey}-{userId}", entry => FreeSql.Ado.Query<string>("select RoleID from UserRole where UserID = @userId", new { userId }));

    public bool SaveRolesByGroupId(string? groupId, IEnumerable<string> roleIds)
    {
        var ret = false;
        try
        {
            FreeSql.Transaction(() =>
            {
                FreeSql.Ado.ExecuteNonQuery("delete from RoleGroup where GroupID = @groupId", new { groupId });
                FreeSql.Insert(roleIds.Select(g => new RoleGroup { RoleID = g, GroupID = groupId })).ExecuteAffrows();
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

    public bool SaveRolesByMenuId(string? menuId, IEnumerable<string> roleIds)
    {
        var ret = false;
        try
        {
            FreeSql.Transaction(() =>
            {
                FreeSql.Ado.ExecuteNonQuery("delete from NavigationRole where NavigationID = @menuId", new { menuId });
                FreeSql.Insert(roleIds.Select(g => new NavigationRole { RoleID = g, NavigationID = menuId })).ExecuteAffrows();
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

    public bool SaveRolesByUserId(string? userId, IEnumerable<string> roleIds)
    {
        var ret = false;
        try
        {
            FreeSql.Transaction(() =>
            {
                FreeSql.Ado.ExecuteNonQuery("delete from UserRole where UserID = @userId", new { userId });
                FreeSql.Insert(roleIds.Select(g => new UserRole { RoleID = g, UserID = userId })).ExecuteAffrows();
            });
            ret = true;
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
