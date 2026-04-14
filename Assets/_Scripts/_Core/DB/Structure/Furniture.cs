using System;
using SQLite;

[Table("Furniture")]
public class Furniture
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }                   //Id товара

    public string Name { get; set; }              //Название
    public string Description { get; set; }       //Описание

    [NotNull]
    public string FilePath { get; set; }          //Путь к файлу модели

    public string ThumbnailPath { get; set; }     //Путь к превью
    public decimal Price { get; set; }            //Цена товара
    public DateTime CreatedAt { get; set; }       //Дата создания
    public bool IsAvailable { get; set; } = true; // Доступность покупки
}