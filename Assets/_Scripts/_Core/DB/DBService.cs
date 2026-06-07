using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using SQLite;
using UnityEngine;
using Zenject;
using Directory = System.IO.Directory;

public class DBService : IInitializable, IDisposable
{
    private SQLiteAsyncConnection _db;
    private readonly string _dbFileName = "DB/SpaceCanvas";
    private readonly string _dbPath;


    public DBService() => _dbPath = Path.Combine(Application.persistentDataPath, $"{_dbFileName}.db");

    public async void Initialize()
    {
        string directory = Path.GetDirectoryName(_dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

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
    public async UniTask RecreateTable<T>() where T : IDBEntity, new()
    {
        await _db.DropTableAsync<T>();
        await _db.CreateTableAsync<T>();
    }
#endif
}