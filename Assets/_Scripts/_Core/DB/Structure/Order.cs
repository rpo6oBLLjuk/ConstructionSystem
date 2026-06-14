using SQLite;
using System;

[Table("Orders")]
public class Order : IDBEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }              // Id заказа

    [Indexed]
    public int UserId { get; set; }          // Id пользователя

    [Indexed]
    public int UserProjectId { get; set; }

    public DateTime CreatedAt { get; set; }  // Дата создания заказа
    public DateTime UpdatedAt { get; set; }  // Дата обновления статуса заказа

    public OrderStatus Status { get; set; }  // Статус заказа 

    public double TotalAmount { get; set; } // Сумма заказа (по списку OrderItem'ов)
    public string Comment { get; set; }     // Комментарии к заказу
}


public enum OrderStatus
{
    New = 0,
    Processing = 1,
    Completed = 2,
    Cancelled = 3
}