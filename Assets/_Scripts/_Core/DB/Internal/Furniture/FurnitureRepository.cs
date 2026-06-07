using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class FurnitureRepository : Repository<Furniture>
{
    public FurnitureRepository(DBService dbService) : base(dbService) { }

    public async UniTask<List<Furniture>> GetAvailableFurniture() => await Db.Table<Furniture>().Where(f => f.IsAvailable).ToListAsync();

    public async UniTask<List<FurnitureType>> GetFurnitureTypes() => await Db.Table<FurnitureType>().ToListAsync();
    public async UniTask<List<ColorType>> GetColorTypes() => await Db.Table<ColorType>().ToListAsync();

    public async UniTask InsertFurnitureTypes(List<FurnitureType> types) => await Db.InsertAllAsync(types);
    public async UniTask InsertColorTypes(List<ColorType> types) => await Db.InsertAllAsync(types);

    public async UniTask<int> GetNextId()
    {
        var lastFurniture = await Db.Table<Furniture>().OrderByDescending(f => f.Id).FirstOrDefaultAsync();
        return lastFurniture == null ? 1 : lastFurniture.Id + 1;
    }

    public async UniTask InsertOrReplaceAsync(Furniture newFurniture) => await Db.InsertOrReplaceAsync(newFurniture);
}
