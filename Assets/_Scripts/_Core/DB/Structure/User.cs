using System;
using SQLite;

[Table("Users")]
public class User : IDBEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }                  // Id пользователя 

    [Indexed, NotNull]
    public int RoleId { get; set; } = 1;

    public string FirstName { get; set; }        // Имя
    public string LastName { get; set; }         // Фамилия

    public string PhoneNumber { get; set; }      // Номер телефона
    public string Email { get; set; }            // Почта

    [Unique]
    public string Login { get; set; }            // Логин
    public string PasswordHash { get; set; }         // Пароль (hash)

    public bool OrderingEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; }  // Дата создания аккаунта
    public DateTime LastLoginAt { get; set; } // Дата последнего входа
}
