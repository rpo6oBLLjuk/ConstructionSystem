using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cysharp.Threading.Tasks;
using SQLite;

public abstract class Repository<T> where T : class, IDBEntity, new()
{
    protected readonly DBService DBService;
    protected SQLiteAsyncConnection Db => DBService.GetConnection();

    protected Repository(DBService dbService) => DBService = dbService;

    public virtual async UniTask<T> GetById(int id) => await Db.Table<T>().FirstOrDefaultAsync(item => item.Id == id);
    public virtual async UniTask<List<T>> GetAll() => await Db.Table<T>().OrderBy(item => item.Id).ToListAsync();
    public virtual async UniTask<List<T>> GetRange(int startIndex, int count)
    {
        if (startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index cannot be negative.");

        if (count <= 0)
            return new List<T>();

        return await Db.Table<T>()
            .OrderBy(item => item.Id)
            .Skip(startIndex)
            .Take(count)
            .ToListAsync();
    }
    public virtual async UniTask<List<T>> GetWhere(Expression<Func<T, bool>> predicate) => await Db.Table<T>().Where(predicate).ToListAsync();

    public virtual async UniTask Insert(T item) => await Db.InsertAsync(item);
    public virtual async UniTask InsertMany(IEnumerable<T> items) => await Db.InsertAllAsync(items);

    public virtual async UniTask Update(T item) => await Db.UpdateAsync(item);
    public virtual async UniTask UpdateMany(IEnumerable<T> items) => await Db.UpdateAllAsync(items);

    public virtual async UniTask Delete(T item) => await Db.DeleteAsync(item);
    public virtual async UniTask DeleteMany(IEnumerable<T> items) => await Db.RunInTransactionAsync(db => items.ToList().ForEach(item => db.Delete(item)));
    public virtual async UniTask DeleteById(int id)
    {
        T item = await GetById(id);

        if (item == null)
            return;

        await Delete(item);
    }

    public virtual async UniTask<int> Count() => await Db.Table<T>().CountAsync();
    public virtual async UniTask<bool> Exists(int id) => await GetById(id) != null;
}