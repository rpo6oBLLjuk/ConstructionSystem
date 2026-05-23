using System;
using SQLite;

[Table("UserProjects")]
public class UserProject : IDBEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    [NotNull]
    public string ProjectName { get; set; }

    [NotNull]
    public string FilePath { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}