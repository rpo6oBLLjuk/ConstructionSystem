using System.Data.SqlTypes;
using SQLite;

public class db_users
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string PhoneNumber { get; set; }
    public string Email { get; set; }

    [Unique]
    public string Login { get; set; }
    public string Password { get; set; }

    public SqlDateTime CreatedAt {  get; set; }
    public SqlDateTime LastLoginAt { get; set; }
}
