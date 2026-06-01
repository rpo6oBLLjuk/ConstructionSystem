using SQLite;

[Table("ColorTypes")]
public class ColorType : IDBEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string Name { get; set; }
}