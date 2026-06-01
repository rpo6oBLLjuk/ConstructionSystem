using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class UserProjectRepository : Repository<UserProject>
{
    public UserProjectRepository(DBService dbService) : base(dbService) { }

    // Получение всех сохраненных проектов конкретного пользователя
    public async UniTask<List<UserProject>> GetProjectsByUserId(int userId)
        => await Db.Table<UserProject>().Where(p => p.UserId == userId).OrderByDescending(p => p.UpdatedAt).ToListAsync();

    public async UniTask<bool> ExistsByProjectName(string projectName) => await Db.Table<UserProject>().FirstOrDefaultAsync(project => project.ProjectName == projectName) != null;
}
