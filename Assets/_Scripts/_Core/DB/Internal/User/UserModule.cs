using System;
using System.Collections.Generic;
using System.Linq;
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

    public async UniTask<int> GetUsersCount() => await _userRepository.Count();
    public async UniTask<int> GetUsersCount(int? id, string login, int? roleId)
    {
        List<User> users = await GetFilteredUsers(id, login, roleId);
        return users.Count;
    }

    public async UniTask<List<User>> GetUsersPage(int offset, int count, int? id, string login, int? roleId)
    {
        List<User> users = await GetFilteredUsers(id, login, roleId);

        return users
            .OrderBy(user => user.RoleId)
            .ThenBy(user => user.Id)
            .Skip(offset)
            .Take(count)
            .ToList();
    }

    public async UniTask<bool> CreateUser(User user, Action<string> onUserAlreadyExists)
    {
        user.PasswordHash = HashPassword(user.PasswordHash);
        user.CreatedAt = DateTime.Now;

        if (await GetUsersCount() == 0) //If users count is null, first user is admin
        {
            await _userRepository.AddRoleTypes();
            user.RoleId = 3;
        }

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

    public async UniTask UpdateUser(User user) => await _userRepository.Update(user);

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

    private async UniTask<List<User>> GetFilteredUsers(int? id, string fullNameSearch, int? roleId)
    {
        List<User> users;

        if (id.HasValue)
            users = await _userRepository.GetWhere(user => user.Id == id.Value);
        else if (roleId.HasValue)
            users = await _userRepository.GetWhere(user => user.RoleId == roleId.Value);
        else
            users = await _userRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(fullNameSearch))
        {
            string search = NormalizeSearch(fullNameSearch);

            users = users
                .Where(user =>
                    NormalizeSearch($"{user.FirstName} {user.LastName}").Contains(search) ||
                    NormalizeSearch($"{user.LastName} {user.FirstName}").Contains(search))
                .ToList();
        }

        if (roleId.HasValue && id.HasValue)
        {
            users = users
                .Where(user => user.RoleId == roleId.Value)
                .ToList();
        }

        return users;
    }

    private string NormalizeSearch(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
}

