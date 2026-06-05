using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using SQLite;
using UnityEngine;
using Zenject;

public class DBService : IInitializable, IDisposable
{
    private SQLiteAsyncConnection _db;
    private readonly string _dbFileName = "DB/SpaceCanvas";
    private readonly string _dbPath;


    public DBService() => _dbPath = Path.Combine(Application.persistentDataPath, $"{_dbFileName}.db");

    public async void Initialize()
    {
        _db = new SQLiteAsyncConnection(_dbPath);

        try
        {
            await _db.CreateTablesAsync(CreateFlags.None, new[]
            {
                typeof(User),
                typeof(UserProject),
                typeof(Furniture),
                typeof(Order),
                typeof(OrderItem),
                typeof(ColorType),
                typeof(FurnitureType),
                typeof(Role)
            });
            DebugWrapper.InactiveLog(this, "Database initialized and tables created");
        }
        catch (Exception ex)
        {
            DebugWrapper.LogError(this, $"Error during table creation: {ex.Message}");
        }
    }

    // Простой доступ к соединению для модулей
    public SQLiteAsyncConnection GetConnection() => _db;
    public void GetConnection(out SQLiteAsyncConnection asyncConnection) => asyncConnection = _db;

    public void Dispose() => CloseConnection();

    private void CloseConnection()
    {
        if (_db != null)
        {
            _db.CloseAsync();
            _db = null;

            DebugWrapper.InactiveLog(this, "Connection closed");
        }
    }

#if UNITY_EDITOR
    private async UniTask AddRoleTypes(SQLiteAsyncConnection conn)
    {
        List<Role> _defaultRoleTypes = new()
        {
            new Role { Name = "Client" },
            new Role { Name = "Manager" },
            new Role { Name = "Admin" }
        };
        await conn.InsertAllAsync(_defaultRoleTypes);
    }
    private async UniTask AddFunitureTypes(SQLiteAsyncConnection conn)
    {
        List<FurnitureType> _defaultFurnitureTypes = new()
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
        await conn.InsertAllAsync(_defaultFurnitureTypes);
    }
    private async UniTask AddColorTypes(SQLiteAsyncConnection conn)
    {
        List<ColorType> _defaultColorTypes = new()
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
        await conn.InsertAllAsync(_defaultColorTypes);
    }
    public async UniTask RecreateTable<T>() where T : IDBEntity, new()
    {
        await _db.DropTableAsync<T>();
        await _db.CreateTableAsync<T>();
    }
#endif
}