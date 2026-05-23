using System;
using SQLite;

[Table("Furniture")]
public class Furniture : IDBEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; }
    public string Description { get; set; }

    [Indexed]
    public int FurnitureTypeId { get; set; }
    [Indexed]
    public int ColorTypeId { get; set; }

    public string Manufacturer { get; set; }

    public float Width { get; set; }
    public float Height { get; set; }
    public float Depth { get; set; }

    [NotNull]
    public string FilePath { get; set; }          // путь к 3D-модели
    public string ThumbnailPath { get; set; }     // путь к превью

    public double Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsAvailable { get; set; } = true;
}