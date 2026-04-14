using System.Data.SqlTypes;
using SQLite;
using UnityEngine;

public class db_furniture
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }

    [NotNull]
    public string FilePath { get; set; }

    [Unique]
    public string Login { get; set; }
    public string Password { get; set; }

    public SqlDateTime CreatedAt { get; set; }
    public SqlDateTime LastLoginAt { get; set; }
}