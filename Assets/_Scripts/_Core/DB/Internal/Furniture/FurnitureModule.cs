using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class FurnitureModule
{
    private readonly FurnitureRepository _furnitureRepository;


    public FurnitureModule(FurnitureRepository furnitureRepository) => _furnitureRepository = furnitureRepository;

    public async UniTask<Furniture> GetFurnitureById(int id) => await _furnitureRepository.GetById(id);
    public async UniTask<List<Furniture>> GetFurnitureByIds(List<int> ids) => await _furnitureRepository.GetByIds(ids);

    public async UniTask<int> GetFurnitureCount(string search) => (await GetFilteredFurniture(search)).Count;
    public async UniTask<List<Furniture>> GetFurniturePage(int offset, int count, string search) => (await GetFilteredFurniture(search)).OrderBy(item => item.Id).Skip(offset).Take(count).ToList();

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

    public async UniTask<List<FurnitureType>> GetFurnitureTypes() => await _furnitureRepository.GetFurnitureTypes();
    public async UniTask<List<ColorType>> GetColorTypes() => await _furnitureRepository.GetColorTypes();

    public async UniTask<int> GetNextId() => await _furnitureRepository.GetNextId();

    private async UniTask<List<Furniture>> GetFilteredFurniture(string search)
    {
        List<Furniture> furniture = await _furnitureRepository.GetAll();

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