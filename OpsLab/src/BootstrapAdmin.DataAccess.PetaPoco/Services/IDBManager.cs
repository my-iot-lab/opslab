using PetaPoco;

namespace BootstrapAdmin.DataAccess.PetaPoco.Services;

public interface IDBManager
{
    IDatabase Create(string? connectionName = "ba", bool keepAlive = false);
}
