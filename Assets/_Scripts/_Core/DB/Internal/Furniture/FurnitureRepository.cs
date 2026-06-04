using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class FurnitureRepository : Repository<Furniture>
{
    public FurnitureRepository(DBService dbService) : base(dbService) { }

    public async UniTask<List<Furniture>> GetAvailableFurniture() => await Db.Table<Furniture>().Where(f => f.IsAvailable).ToListAsync();

    public async UniTask<List<FurnitureType>> GetFurnitureTypes() => await Db.Table<FurnitureType>().ToListAsync();
    public async UniTask<List<ColorType>> GetColorTypes() => await Db.Table<ColorType>().ToListAsync();
}
