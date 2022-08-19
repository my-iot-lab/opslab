using Ops.Host.Common.Extensions;
using Ops.Host.Core.Models;

namespace Ops.Host.Core.Services.Impl;

internal sealed class UserService : IUserService
{
    private readonly IFreeSql _freeSql;

    public UserService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public User GetUserById(int id)
    {
        return _freeSql.GetRepository<User>().Where(s => s.Id == id).ToOne();
    }

    public User GetUserByName(string userName)
    {
        return _freeSql.GetRepository<User>().Where(s => s.UserName == userName).ToOne();
    }

    public List<User> GetAllUsers()
    {
        return _freeSql.GetRepository<User>().Select.ToList();
    }

    public (List<User> items, long count) GetPaged(UserFilter filter, int pageIndex, int pageItems)
    {
        var items = _freeSql.Select<User>()
                            .WhereIf(!string.IsNullOrEmpty(filter.UserName), s => s.UserName == filter.UserName)
                            .WhereIf(filter.CreatedAtStart != null, s => s.CreatedAt >= filter.CreatedAtStart.ToDayMin())
                            .WhereIf(filter.CreatedAtEnd != null, s => s.CreatedAt <= filter.CreatedAtEnd.ToDayMax())
                            .OrderByDescending(s => s.CreatedAt)
                            .Count(out var total)
                            .Page(pageIndex, pageItems)
                            .ToList();
        return (items, total);
    }

    public async Task<(List<User> items, long count)> GetPagedAsync(UserFilter filter, int pageIndex, int pageItems)
    {
        var items = await _freeSql.Select<User>()
                            .WhereIf(!string.IsNullOrEmpty(filter.UserName), s => s.UserName == filter.UserName)
                            .WhereIf(filter.CreatedAtStart != null, s => s.CreatedAt >= filter.CreatedAtStart.ToDayMin())
                            .WhereIf(filter.CreatedAtEnd != null, s => s.CreatedAt <= filter.CreatedAtEnd.ToDayMax())
                            .OrderByDescending(s => s.CreatedAt)
                            .Count(out var total)
                            .Page(pageIndex, pageItems)
                            .ToListAsync();
        return (items, total);
    }
}
