using System;
using System.IO;
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
            await _db.CreateTablesAsync<User, Furniture, Order, OrderItem>();
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
}