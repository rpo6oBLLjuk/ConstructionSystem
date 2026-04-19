using Cysharp.Threading.Tasks;
using SQLite;

public class UserRepository
{
    private readonly DBService _dbService;
    private SQLiteAsyncConnection Db => _dbService.GetConnection();


    public UserRepository(DBService dbService) => _dbService = dbService;

    public async UniTask<User> GetByLogin(string login)
        => await Db.Table<User>().FirstOrDefaultAsync(u => u.Login == login);

    public async UniTask Insert(User user) => await Db.InsertAsync(user);

    public async UniTask Update(User user) => await Db.UpdateAsync(user);
}
