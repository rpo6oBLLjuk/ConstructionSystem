using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class OrderItemRepository : Repository<OrderItem>
{
    public OrderItemRepository(DBService dbService) : base(dbService) { }

    public async UniTask<List<OrderItem>> GetItemsByOrderId(int orderId)
        => await Db.Table<OrderItem>().Where(item => item.OrderId == orderId).ToListAsync();
}
