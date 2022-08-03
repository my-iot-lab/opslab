using Ops.Host.Common.Domain;
using Ops.Host.Core.Models;

namespace Ops.Host.Core.Services;

public interface IUserService : IDomainService
{
    User GetUserById(int id);

    User GetUserByName(string userName);

    List<User> GetAllUsers();
}
