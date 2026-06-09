using Coffee.UIEffects;

public class UserViewData
{
    public int Id { get; set; }

    public string FullName { get; set; }
    public string Login { get; set; }

    public int RoleId { get; set; }
    public string RoleName { get; set; }

    public string PhoneNumber { get; set; }
    public string Email { get; set; }

    public bool OrderingEnabled { get; set; }

    public string CreatedAt { get; set; }
    public string LastLoginAt { get; set; }

    public User SourceUser { get; set; }
    public UIEffect UIEffect { get; set; }
}