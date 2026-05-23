using SQLite;

[Table("FurnitureTypes")]
public class FurnitureType : IDBEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string Name { get; set; }
}