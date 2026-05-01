using System;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;

public class UserModule
{
    private readonly UserRepository _userRepository;

    public User CurrentUser { get; private set; }

    private string _incorrectPassword = "Incorrect password";
    private string _userNotFound = "User '{0}' not found";


    public UserModule(UserRepository userRepository) => _userRepository = userRepository;

    public async UniTask<bool> CreateUser(User user)
    {
        user.Password = HashPassword(user.Password);
        user.CreatedAt = DateTime.Now;

        try
        {
            await _userRepository.Insert(user);
            await SetCurrentSession(user);

            return true;
        }
        catch (Exception ex)
        {
            DebugWrapper.LogError(this, ex.Message);
            return false;
        }
    }
    public async UniTask<bool> CreateUser(string login, string password)
    {
        User user = new()
        {
            Login = login,
            Password = password
        };

        return await CreateUser(user);
    }

    public async UniTask LogIn(string login, string password, Action<User> onLoginSuccess, Action<string> onLoginFailed)
    {
        User user = await _userRepository.GetByLogin(login);

        if (user == null)
        {
            onLoginFailed?.Invoke(string.Format(_userNotFound, login));
            return;
        }

        if (!VerifyPassword(password, user.Password))
        {
            onLoginFailed?.Invoke(_incorrectPassword);
            return;
        }

        await SetCurrentSession(user);
        onLoginSuccess?.Invoke(user);
    }
    public void LogOut() => CurrentUser = null;

    private async UniTask SetCurrentSession(User user)
    {
        CurrentUser = user;
        user.LastLoginAt = DateTime.Now;

        await _userRepository.Update(user);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

        StringBuilder builder = new StringBuilder();
        foreach (var b in bytes)
            builder.Append(b.ToString("x2"));

        return builder.ToString();
    }
    private bool VerifyPassword(string password, string hash) => HashPassword(password) == hash;
}

