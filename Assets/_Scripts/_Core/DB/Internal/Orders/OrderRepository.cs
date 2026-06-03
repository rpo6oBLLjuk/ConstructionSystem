using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class OrderRepository : Repository<Order>
{
    public OrderRepository(DBService dbService) : base(dbService) { }

    public async UniTask<List<Order>> GetOrdersByUserId(int userId) => await Db.Table<Order>().Where(o => o.UserId == userId).ToListAsync();

    public async UniTask<int> CountByUserId(int userId) => await Db.Table<Order>().Where(order => order.UserId == userId).CountAsync();

    public async UniTask<List<Order>> PagingByUserId(int userId, int offset, int count) => await Db.Table<Order>()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.Id)
            .Skip(offset)
            .Take(count)
            .ToListAsync();
}
