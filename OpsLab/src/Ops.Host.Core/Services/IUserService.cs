using Ops.Host.Common.Domain;
using Ops.Host.Core.Models;

namespace Ops.Host.Core.Services;

public interface IUserService : IDomainService
{
    User GetUserById(int id);

    User GetUserByName(string userName);

    List<User> GetAllUsers();

    (List<User> items, long count) GetPaged(UserFilter filter, int pageIndex, int pageItems);

    Task<(List<User> items, long count)> GetPagedAsync(UserFilter filter, int pageIndex, int pageItems);
}
