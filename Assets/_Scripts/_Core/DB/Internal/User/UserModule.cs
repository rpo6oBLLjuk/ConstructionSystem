using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using SQLite;

public class UserModule
{
    private readonly UserRepository _userRepository;

    public User CurrentUser { get; private set; }

    public event Action<User> LoggedIn;
    public event Action<User> LoggedOut;

    private string _incorrectPassword = "Incorrect password";
    private string _userNotFound = "User <b>{0}</b> not found";
    private string _userAlreadyExists = "Login <b>{0}</b> already exists";

    private string _permissionDenied = "User <b>{0}</b> does not have sufficient permissions to perform this operation";


    public UserModule(UserRepository userRepository) => _userRepository = userRepository;

    public async UniTask<User> GetUserById(int id) => await _userRepository.GetById(id);
    public async UniTask<List<User>> GetUsersByIds(List<int> ids) => await _userRepository.GetByIds(ids);

    public async UniTask<bool> CreateUser(User user, Action<string> onUserAlreadyExists)
    {
        user.PasswordHash = HashPassword(user.PasswordHash);
        user.CreatedAt = DateTime.Now;

        try
        {
            await _userRepository.Insert(user);
            await SetCurrentSession(user);

            return true;
        }
        catch (SQLiteException ex) when (ex.Result == SQLite3.Result.Constraint)
        {
            string error = string.Format(_userAlreadyExists, user.Login);
            DebugWrapper.LogWarning(this, error);

            onUserAlreadyExists?.Invoke(error);

            return false;
        }
    }
    public async UniTask<bool> CreateUser(string login, string password, Action<string> onUserAlreadyExists)
    {
        User user = new()
        {
            Login = login,
            PasswordHash = password
        };

        return await CreateUser(user, onUserAlreadyExists);
    }

    public async UniTask LogIn(string login, string password, Action<User> onLoginSuccess, Action<string> onLoginFailed)
    {
        User user = await _userRepository.GetByLogin(login);

        if (user == null)
        {
            onLoginFailed?.Invoke(string.Format(_userNotFound, login));
            return;
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            onLoginFailed?.Invoke(_incorrectPassword);
            return;
        }

        await SetCurrentSession(user);
        onLoginSuccess?.Invoke(user);
    }
    public void LogOut()
    {
        if (CurrentUser == null)
            return;

        User loggedOutUser = CurrentUser;

        CurrentUser = null;

        LoggedOut?.Invoke(loggedOutUser);
    }

    public async UniTask SetUserRole(User user, int roleId, Action<User> onSuccess, Action<string> onFailed) // #1 user, #2 manager, #3 admin
    {
        if(CurrentUser.RoleId != 3)
        {
            onFailed?.Invoke(string.Format(_permissionDenied, CurrentUser.FirstName));
            return;
        }

        user.RoleId = roleId;

        await _userRepository.Update(user);
        onSuccess?.Invoke(user);
    }
    private async UniTask SetCurrentSession(User user)
    {
        user.LastLoginAt = DateTime.Now;
        CurrentUser = user;
   
        LoggedIn?.Invoke(user);
        
        await _userRepository.Update(user);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

        StringBuilder builder = new StringBuilder();
        foreach (var b in bytes)
            builder.Append(b.ToString("x2"));

        //DebugWrapper.FastLog(this, $"Password: {password}, Hash: {builder.ToString()}");

        return builder.ToString();
    }
    private bool VerifyPassword(string password, string hash) => HashPassword(password) == hash;
}

