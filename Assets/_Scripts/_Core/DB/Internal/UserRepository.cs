using Cysharp.Threading.Tasks;

public class UserRepository : Repository<User>
{
    public UserRepository(DBService dbService) : base(dbService) { }

    public async UniTask<User> GetByLogin(string login) => await Db.Table<User>().FirstOrDefaultAsync(u => u.Login == login);
}
