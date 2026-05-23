using SQLite;

[Table("OrderItems")]
public class OrderItem : IDBEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }            //Фиктивный ID для SQLite, т.к. нет составных первичных ключей

    [Indexed]
    public int OrderId { get; set; }       //Id заказа

    [Indexed]
    public int FurnitureId { get; set; }   //Id товара

    public int Count { get; set; }         // Количество товара в заказе
    public double UnitPrice { get; set; } // Цена за единицу товара
}