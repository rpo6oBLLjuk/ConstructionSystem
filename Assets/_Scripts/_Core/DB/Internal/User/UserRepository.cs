using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SQLite;

public class UserRepository : Repository<User>
{
    public UserRepository(DBService dbService) : base(dbService) { }

    public async UniTask<User> GetByLogin(string login) => await Db.Table<User>().FirstOrDefaultAsync(u => u.Login == login);

    public async UniTask AddRoleTypes()
    {
        List<Role> _defaultRoleTypes = new()
        {
            new Role { Name = "Client" },
            new Role { Name = "Manager" },
            new Role { Name = "Admin" }
        };
        await Db.InsertAllAsync(_defaultRoleTypes);
    }
}
