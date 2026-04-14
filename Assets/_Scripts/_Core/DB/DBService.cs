using SQLite;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System;
using UnityEngine;
using Zenject;
using System.IO;

public class DBService : MonoBehaviour
{
    //[Inject] DBConfig config;


    public void OpenConnection()
    {

    }

    private SQLiteConnection _db;

    void Awake()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, "FurnitureShop.db");
        _db = new SQLiteConnection(dbPath);

        // Создаём таблицы
        _db.CreateTable<User>();
        _db.CreateTable<Furniture>();
        _db.CreateTable<Order>();
        _db.CreateTable<OrderItem>();
    }

    // ============================================
    // 1. ДОБАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯ
    // ============================================

    /// <summary>
    /// Добавляет нового пользователя в базу данных
    /// </summary>
    /// <returns>ID созданного пользователя или -1 при ошибке</returns>
    public int AddUser(string firstName, string lastName, string phoneNumber, string email, string login, string password)
    {
        try
        {
            // Хешируем пароль перед сохранением
            string passwordHash = HashPassword(password);

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Email = email,
                Login = login,
                Password = passwordHash, // Сохраняем хеш, не пароль!
                CreatedAt = DateTime.Now,
                LastLoginAt = DateTime.Now
            };

            int insertedId = _db.Insert(user);
            Debug.Log($"Пользователь '{login}' добавлен с ID: {insertedId}");
            return insertedId;
        }
        catch (SQLiteException ex)
        {
            Debug.LogError($"Ошибка при добавлении пользователя: {ex.Message}");
            return -1;
        }
    }

    // ============================================
    // 2. ДОБАВЛЕНИЕ ТОВАРА
    // ============================================

    /// <summary>
    /// Добавляет новый товар (мебель) в каталог
    /// </summary>
    /// <returns>ID созданного товара или -1 при ошибке</returns>
    public int AddFurniture(string name, string description, string filePath, string thumbnailPath, decimal price, bool isAvailable = true)
    {
        try
        {
            var furniture = new Furniture
            {
                Name = name,
                Description = description,
                FilePath = filePath,
                ThumbnailPath = thumbnailPath,
                Price = price,
                CreatedAt = DateTime.Now,
                IsAvailable = isAvailable
            };

            _db.Insert(furniture); // Не сохраняем возвращаемое значение

            // ID теперь доступен в свойстве объекта!
            int insertedId = furniture.Id;

            Debug.Log($"Товар '{name}' добавлен с ID: {insertedId}");
            return insertedId;
        }
        catch (SQLiteException ex)
        {
            Debug.LogError($"Ошибка при добавлении товара: {ex.Message}");
            return -1;
        }
    }

    // ============================================
    // 3. СОЗДАНИЕ ЗАКАЗА ИЗ СПИСКА ТОВАРОВ
    // ============================================

    /// <summary>
    /// Вспомогательный класс для передачи позиций заказа
    /// </summary>
    public class OrderPosition
    {
        public int FurnitureId { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Создаёт заказ из списка товаров
    /// </summary>
    /// <param name="userId">ID пользователя, делающего заказ</param>
    /// <param name="positions">Список позиций (ID товара + количество)</param>
    /// <param name="comment">Комментарий к заказу (опционально)</param>
    /// <returns>ID созданного заказа или -1 при ошибке</returns>
    public int CreateOrder(int userId, List<OrderPosition> positions, string comment = "")
    {
        // Проверяем, существует ли пользователь
        var user = _db.Find<User>(userId);
        if (user == null)
        {
            Debug.LogError($"Пользователь с ID {userId} не найден!");
            return -1;
        }

        // Проверяем, что список позиций не пуст
        if (positions == null || positions.Count == 0)
        {
            Debug.LogError("Невозможно создать пустой заказ!");
            return -1;
        }

        try
        {
            _db.BeginTransaction();

            // 1. Создаём заказ
            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = OrderStatus.New,
                Comment = comment,
                TotalAmount = 0 // Пока 0, посчитаем после добавления позиций
            };

            int orderId = _db.Insert(order);

            // 2. Добавляем позиции и считаем общую сумму
            decimal totalAmount = 0;

            foreach (var pos in positions)
            {
                // Получаем актуальную цену товара из каталога
                var furniture = _db.Find<Furniture>(pos.FurnitureId);

                if (furniture == null)
                {
                    Debug.LogError($"Товар с ID {pos.FurnitureId} не найден!");
                    _db.Rollback();
                    return -1;
                }

                if (!furniture.IsAvailable)
                {
                    Debug.LogError($"Товар '{furniture.Name}' недоступен для заказа!");
                    _db.Rollback();
                    return -1;
                }

                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    FurnitureId = pos.FurnitureId,
                    Count = pos.Count,
                    UnitPrice = furniture.Price // Фиксируем цену на момент заказа
                };

                _db.Insert(orderItem);
                totalAmount += furniture.Price * pos.Count;
            }

            // 3. Обновляем итоговую сумму заказа
            order.TotalAmount = totalAmount;
            _db.Update(order);

            _db.Commit();

            Debug.Log($"Заказ #{orderId} создан. Пользователь: {user.Login}, Сумма: {totalAmount:C}");
            return orderId;
        }
        catch (Exception ex)
        {
            _db.Rollback();
            Debug.LogError($"Ошибка при создании заказа: {ex.Message}");
            return -1;
        }
    }

    // ============================================
    // ВСПОМОГАТЕЛЬНЫЙ МЕТОД: ХЕШИРОВАНИЕ ПАРОЛЯ
    // ============================================

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    // ============================================
    // ПРИМЕР ИСПОЛЬЗОВАНИЯ
    // ============================================

    void Start()
    {
        // 1. Добавляем пользователя
        int userId = AddUser("Иван", "Петров", "+79161234567", "ivan@mail.ru", "ivan_p", "secret123");

        // 2. Добавляем товары
        int chairId = AddFurniture("Стул офисный", "Удобный стул на колёсиках", "/models/chair.ab", "/thumbs/chair.png", 3500m);
        int tableId = AddFurniture("Стол письменный", "Дубовый стол 120x60", "/models/table.ab", "/thumbs/table.png", 12000m);
        int lampId = AddFurniture("Лампа настольная", "Светодиодная с регулировкой", "/models/lamp.ab", "/thumbs/lamp.png", 1800m);

        // 3. Создаём заказ
        var positions = new List<OrderPosition>
        {
            new OrderPosition { FurnitureId = chairId, Count = 2 },
            new OrderPosition { FurnitureId = tableId, Count = 1 },
            new OrderPosition { FurnitureId = lampId, Count = 1 }
        };

        int orderId = CreateOrder(userId, positions, "Доставка в будни после 18:00");
    }

}


/*  command.CommandText = @"
        INSERT INTO Players (Name, Level, Experience) 
        VALUES (@name, @level, @exp);
                    
        SELECT last_insert_rowid();
    ";
                
    command.Parameters.AddWithValue("@name", playerName);
    command.Parameters.AddWithValue("@level", startLevel);
    command.Parameters.AddWithValue("@exp", 0);
*/