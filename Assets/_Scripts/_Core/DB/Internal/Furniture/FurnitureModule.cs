using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class FurnitureModule
{
    private readonly FurnitureRepository _furnitureRepository;


    public FurnitureModule(FurnitureRepository furnitureRepository) => _furnitureRepository = furnitureRepository;

    public async UniTask<Furniture> GetFurnitureById(int id) => await _furnitureRepository.GetById(id);
    public async UniTask<List<Furniture>> GetFurnitureByIds(List<int> ids) => await _furnitureRepository.GetByIds(ids);

    public async UniTask<int> GetFurnitureCount(string search, int? furnitureTypeId, int? colorTypeId) => (await GetFilteredFurniture(search, furnitureTypeId, colorTypeId)).Count;
    public async UniTask<List<Furniture>> GetFurniturePage(int offset, int count, string search, int? furnitureTypeId, int? colorTypeId) => (await GetFilteredFurniture(search, furnitureTypeId, colorTypeId)).OrderBy(item => item.Id).Skip(offset).Take(count).ToList();

    public async UniTask CreateFurnitureWithCustomId(Furniture furniture)
    {
        furniture.CreatedAt = System.DateTime.Now;
        furniture.UpdatedAt = System.DateTime.Now;

        await _furnitureRepository.InsertOrReplaceAsync(furniture);
    }
    public async UniTask UpdateFurniture(Furniture furniture)
    {
        furniture.UpdatedAt = System.DateTime.Now;
        await _furnitureRepository.Update(furniture);
    }
    public async UniTask DeleteFurniture(Furniture furniture) => await _furnitureRepository.Delete(furniture);

    public async UniTask<List<FurnitureType>> GetFurnitureTypes()
    {
        List<FurnitureType> types = await _furnitureRepository.GetFurnitureTypes();
        if (types.Count == 0)
        {
            types = new()
            {
                new FurnitureType { Name = "Chair" },
                new FurnitureType { Name = "Table" },
                new FurnitureType { Name = "Sofa" },
                new FurnitureType { Name = "Cabinet" },
                new FurnitureType { Name = "Shelf" },
                new FurnitureType { Name = "Bed" },
                new FurnitureType { Name = "Desk" },
                new FurnitureType { Name = "Armchair" },
                new FurnitureType { Name = "Wardrobe" },
                new FurnitureType { Name = "Lamp" },
                new FurnitureType { Name = "Nightstand" },
                new FurnitureType { Name = "TV Stand" }
            };
            await _furnitureRepository.InsertFurnitureTypes(types);
        }

        return types;
    }
    public async UniTask<List<ColorType>> GetColorTypes()
    {
        List<ColorType> types = await _furnitureRepository.GetColorTypes();

        if (types.Count == 0)
        {
            types = new()
            {
                new ColorType { Name = "White" },
                new ColorType { Name = "Black" },
                new ColorType { Name = "Gray" },
                new ColorType { Name = "Brown" },
                new ColorType { Name = "Beige" },
                new ColorType { Name = "Natural Wood" },
                new ColorType { Name = "Dark Wood" },
                new ColorType { Name = "Oak" },
                new ColorType { Name = "Walnut" },
                new ColorType { Name = "Red" },
                new ColorType { Name = "Blue" },
                new ColorType { Name = "Green" },
                new ColorType { Name = "Yellow" },
                new ColorType { Name = "Orange" },
                new ColorType { Name = "Metallic" },
                new ColorType { Name = "Transparent" }
            };

            await _furnitureRepository.InsertColorTypes(types);
        }
        return types;
    }

    public async UniTask<int> GetNextId() => await _furnitureRepository.GetNextId();

    private async UniTask<List<Furniture>> GetFilteredFurniture(string search, int? furnitureTypeId, int? colorTypeId)
    {
        List<Furniture> furniture;

        if (furnitureTypeId.HasValue)
        {
            furniture = await _furnitureRepository.GetWhere(
                item => item.FurnitureTypeId == furnitureTypeId.Value
            );
        }
        else if (colorTypeId.HasValue)
        {
            furniture = await _furnitureRepository.GetWhere(
                item => item.ColorTypeId == colorTypeId.Value
            );
        }
        else
        {
            furniture = await _furnitureRepository.GetAll();
        }

        if (furnitureTypeId.HasValue && colorTypeId.HasValue)
        {
            furniture = furniture
                .Where(item => item.ColorTypeId == colorTypeId.Value)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string normalizedSearch = NormalizeSearch(search);

            furniture = furniture
                .Where(item =>
                    NormalizeSearch(item.Name).Contains(normalizedSearch) ||
                    NormalizeSearch(item.Manufacturer).Contains(normalizedSearch) ||
                    NormalizeSearch(item.Description).Contains(normalizedSearch))
                .ToList();
        }

        return furniture;
    }

    private string NormalizeSearch(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
}