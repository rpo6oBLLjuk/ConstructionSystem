using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class OrderRepository : Repository<Order>
{
    public OrderRepository(DBService dbService) : base(dbService) { }

    public async UniTask<List<Order>> GetOrdersByUserId(int userId)
        => await Db.Table<Order>().Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).ToListAsync();
}
