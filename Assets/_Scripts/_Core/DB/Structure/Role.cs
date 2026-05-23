using SQLite;

[Table("Roles")]
public class Role : IDBEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string Name { get; set; }
}