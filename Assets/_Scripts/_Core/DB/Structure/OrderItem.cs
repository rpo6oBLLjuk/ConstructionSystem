using SQLite;

[Table("OrderItems")]
public class OrderItem
{
    [Indexed]
    public int OrderId { get; set; }       //Id заказа

    [Indexed]
    public int FurnitureId { get; set; }   //Id товара

    public int Count { get; set; }         // Количество товара в заказе
    public double UnitPrice { get; set; } // Цена за единицу товара
}