using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UserPanelPresenter : BaseLayoutPresenter
{
    [Inject] private UserModule _userModule;
    [Inject] private NotificationService _notificationService;

    [SerializeField] private UserPanelView _view;

    [field: SerializeField] int _pageSize = 10;


    private int _currentPage = 1;
    private int _totalPages = 1;

    private int? _currentUserIdSearch;
    private string _currentFullNameSearch;
    private int? _currentRoleFilter;


    protected override void OnEnable()
    {
        base.OnEnable();

        _view.OnUserSelected += HandleUserSelected;

        _view.OnNextPageRequested += HandleNextPageRequested;
        _view.OnPreviousPageRequested += HandlePreviousPageRequested;

        _view.OnIdSearchRequested += HandleIdSearchRequested;
        _view.OnFullNameSearchRequested += HandleFullNameSearchRequested;
        _view.OnRoleFilterChanged += HandleRoleFilterChanged;

        _view.OnUserSaveRequested += HandleUserSaveRequested;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        _view.OnUserSelected -= HandleUserSelected;

        _view.OnNextPageRequested -= HandleNextPageRequested;
        _view.OnPreviousPageRequested -= HandlePreviousPageRequested;

        _view.OnIdSearchRequested -= HandleIdSearchRequested;
        _view.OnFullNameSearchRequested -= HandleFullNameSearchRequested;
        _view.OnRoleFilterChanged -= HandleRoleFilterChanged;

        _view.OnUserSaveRequested -= HandleUserSaveRequested;
    }

    public override void Show()
    {
        LoadPage(1).Forget();
        base.Show();
    }

    private void HandleUserSelected(UserViewData user) => _view.ShowSelectedUser(user, _userModule.CurrentUser.Id == user.SourceUser.Id);

    private void HandleNextPageRequested()
    {
        if (_currentPage >= _totalPages)
            return;

        LoadPage(_currentPage + 1).Forget();
    }
    private void HandlePreviousPageRequested()
    {
        if (_currentPage <= 1)
            return;

        LoadPage(_currentPage - 1).Forget();
    }

    private void HandleIdSearchRequested(string idText)
    {
        _currentUserIdSearch = null;

        if (!string.IsNullOrWhiteSpace(idText))
        {
            if (!int.TryParse(idText.Trim(), out int id))
            {
                _notificationService.ShowPopup(
                    "The user's ID must be a number.",
                    "Input warning",
                    NotificationType.Warning
                );

                return;
            }

            _currentUserIdSearch = id;
        }

        LoadPage(1).Forget();
    }
    private void HandleFullNameSearchRequested(string fullName)
    {
        _currentFullNameSearch = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();

        LoadPage(1).Forget();
    }
    private void HandleRoleFilterChanged(int? roleId)
    {
        _currentRoleFilter = roleId;
        LoadPage(1).Forget();
    }

    private void HandleUserSaveRequested(UserViewData userViewData, int newRoleId, bool orderingEnabled)
    {
        if (userViewData == null)
            return;

        SaveUserChanges(userViewData, newRoleId, orderingEnabled).Forget();
    }

    private async UniTask LoadPage(int page)
    {
        int totalCount = await _userModule.GetUsersCount(_currentUserIdSearch, _currentFullNameSearch, _currentRoleFilter);

        _totalPages = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)_pageSize));
        _currentPage = Mathf.Clamp(page, 1, _totalPages);

        int offset = (_currentPage - 1) * _pageSize;

        List<User> users = await _userModule.GetUsersPage(offset, _pageSize, _currentUserIdSearch, _currentFullNameSearch, _currentRoleFilter);

        List<UserViewData> viewData = new();

        foreach (User user in users)
            viewData.Add(ConvertToViewData(user));

        _view.SetUsers(viewData);
        _view.SetPagination(_currentPage, _totalPages);

        HandleUserSelected(viewData.Count > 0 ? viewData[0] : null);
    }
    private async UniTask SaveUserChanges(UserViewData userViewData, int newRoleId, bool orderingEnabled)
    {
        if (_userModule.CurrentUser.Id == userViewData.SourceUser.Id)
        {
            _notificationService.ShowPopup("You cannot change your data.", "User save cancelled", NotificationType.Warning);
            return;
        }
        bool changed = userViewData.RoleId != newRoleId || userViewData.OrderingEnabled != orderingEnabled;

        if (!changed)
        {
            _notificationService.ShowPopup("The user's data is identical to the current one", "User save", NotificationType.Info);
            return;
        }

        userViewData.RoleId = newRoleId;
        userViewData.RoleName = GetRoleName(newRoleId);
        userViewData.OrderingEnabled = orderingEnabled;

        userViewData.SourceUser.RoleId = newRoleId;
        userViewData.SourceUser.OrderingEnabled = orderingEnabled;

        await _userModule.UpdateUser(userViewData.SourceUser);
        _notificationService.ShowPopup("User's data has been saved successfully", "User saved", NotificationType.Success);

        _view.RefreshUser(userViewData);
    }

    private UserViewData ConvertToViewData(User user)
    {
        return new UserViewData
        {
            Id = user.Id,
            FullName = GetUserFullName(user),
            Login = user.Login,
            RoleId = user.RoleId,
            RoleName = GetRoleName(user.RoleId),
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            OrderingEnabled = user.OrderingEnabled,
            CreatedAt = $"{user.CreatedAt:dd.MM.yyyy}\n{user.CreatedAt:HH:mm:ss}",
            LastLoginAt = $"{user.LastLoginAt:dd.MM.yyyy}\n{user.LastLoginAt:HH:mm:ss}",
            SourceUser = user
        };
    }

    private string GetUserFullName(User user) => user == null ? "Unknown User" : $"{user.FirstName} {user.LastName}".Trim();
    private string GetRoleName(int roleId) => roleId switch
    {
        1 => "Client",
        2 => "Manager",
        3 => "Admin",
        _ => "Unknown Role"
    };
}