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
}
