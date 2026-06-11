using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserPanelView : MonoBehaviour
{
    public event Action<UserViewData> OnUserSelected;

    public event Action OnNextPageRequested;
    public event Action OnPreviousPageRequested;

    public event Action<string> OnIdSearchRequested;
    public event Action<string> OnFullNameSearchRequested;
    public event Action<int?> OnRoleFilterChanged;

    public event Action<UserViewData, int, bool> OnUserSaveRequested;

    [Header("Layout")]
    [SerializeField] private Transform _contentParent;
    [SerializeField] private UserViewFactory _factory;

    [Header("Layout UI")]
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _inactiveColor;

    [Header("Search")]
    [SerializeField] private TMP_InputField _idSearchInputField;
    [SerializeField] private TMP_InputField _fullNameSearchInputField;

    [Header("Role filter")]
    [SerializeField] private TMP_Dropdown _roleFilterDropdown;

    [Header("Pagination")]
    [SerializeField] private Button _previousPageButton;
    [SerializeField] private Button _nextPageButton;
    [SerializeField] private TMP_Text _pageText;

    [Header("Selected user panel")]
    [SerializeField] private TMP_Text _selectedIdText;
    [SerializeField] private TMP_Text _selectedFullNameText;
    [SerializeField] private TMP_Text _selectedLoginText;
    [SerializeField] private TMP_Text _selectedEmailText;
    [SerializeField] private TMP_Text _selectedPhoneText;
    [SerializeField] private TMP_Text _selectedCreatedAtText;
    [SerializeField] private TMP_Text _selectedLastLoginAtText;

    [Header("Selected user controls")]
    [SerializeField] private TMP_Dropdown _selectedRoleDropdown;
    [SerializeField] private Toggle _ordersAllowedToggle;
    [SerializeField] private Button _saveButton;

    private readonly List<GameObject> _createdViews = new();
    private readonly Dictionary<int, GameObject> _createdViewsByUserId = new();
    private UserViewData _selectedUser;


    private void OnEnable()
    {
        _previousPageButton.onClick.AddListener(PreviousPageButtonClickHandler);
        _nextPageButton.onClick.AddListener(NextPageButtonClickHandler);

        _idSearchInputField.onEndEdit.AddListener(IdSearchEndEditHandler);
        _fullNameSearchInputField.onEndEdit.AddListener(FullNameSearchEndEditHandler);

        _roleFilterDropdown.onValueChanged.AddListener(RoleFilterChangedHandler);

        _saveButton.onClick.AddListener(SaveButtonClickHandler);

        _selectedRoleDropdown.onValueChanged.AddListener(SetSelectDropdownValue);
    }
    private void OnDisable()
    {
        _previousPageButton.onClick.RemoveListener(PreviousPageButtonClickHandler);
        _nextPageButton.onClick.RemoveListener(NextPageButtonClickHandler);

        _idSearchInputField.onEndEdit.RemoveListener(IdSearchEndEditHandler);
        _fullNameSearchInputField.onEndEdit.RemoveListener(FullNameSearchEndEditHandler);

        _roleFilterDropdown.onValueChanged.RemoveListener(RoleFilterChangedHandler);

        _saveButton.onClick.RemoveListener(SaveButtonClickHandler);

        _selectedRoleDropdown.onValueChanged.RemoveListener(SetSelectDropdownValue);
    }

    public void SetUsers(List<UserViewData> users)
    {
        ClearList();

        if (users == null)
            return;

        foreach (UserViewData user in users)
        {
            GameObject view = _factory.Create(
                user,
                _contentParent,
                UserSelectionHandler
            );

            _createdViews.Add(view);
            _createdViewsByUserId[user.Id] = view;
        }
    }
    public void ShowSelectedUser(UserViewData user, bool self = false)
    {
        if (_selectedUser != null)
        {
            _selectedUser.UIEffect.edgeColor = _inactiveColor;
            _selectedUser.UIEffect.edgeWidth = 0.3f;
        }

        _selectedUser = user;

        if (user == null)
        {
            ClearSelectedUserPanel();
            return;
        }

        _selectedUser.UIEffect.edgeColor = _selectedColor;
        _selectedUser.UIEffect.edgeWidth = 0.4f;

        _ordersAllowedToggle.interactable = !self;
        _selectedRoleDropdown.interactable = !self;

        _selectedIdText.text = $"#{user.Id}";
        _selectedFullNameText.text = user.FullName;
        _selectedLoginText.text = user.Login;
        _selectedEmailText.text = user.Email;
        _selectedPhoneText.text = user.PhoneNumber;
        _selectedCreatedAtText.text = user.CreatedAt;
        _selectedLastLoginAtText.text = user.LastLoginAt;

        int roleDropdownIndex = Mathf.Clamp(user.RoleId - 1, 0, _selectedRoleDropdown.options.Count - 1);
        SetSelectDropdownValue(roleDropdownIndex);

        _ordersAllowedToggle.SetIsOnWithoutNotify(user.OrderingEnabled);
    }

    public void SetPagination(int currentPage, int totalPages)
    {
        _pageText.text = $"{currentPage}/{totalPages}";

        _previousPageButton.interactable = currentPage > 1;
        _nextPageButton.interactable = currentPage < totalPages;
    }
    public void RefreshUser(UserViewData user)
    {
        if (_createdViewsByUserId.TryGetValue(user.Id, out GameObject viewObject))
            _factory.Refresh(viewObject, user);
    }

    private void UserSelectionHandler(UserViewData user)
    {
        ShowSelectedUser(user);
        OnUserSelected?.Invoke(user);
    }

    private void IdSearchEndEditHandler(string value) => OnIdSearchRequested?.Invoke(value);
    private void FullNameSearchEndEditHandler(string value) => OnFullNameSearchRequested?.Invoke(value);

    private void RoleFilterChangedHandler(int dropdownIndex)
    {
        int? roleId = dropdownIndex == 0
            ? null
            : dropdownIndex;

        OnRoleFilterChanged?.Invoke(roleId);
    }
    private void SaveButtonClickHandler()
    {
        if (_selectedUser == null)
            return;

        int selectedRoleId = _selectedRoleDropdown.value + 1;
        bool ordersAllowed = _ordersAllowedToggle.isOn;

        OnUserSaveRequested?.Invoke(_selectedUser, selectedRoleId, ordersAllowed);
    }

    private void PreviousPageButtonClickHandler() => OnPreviousPageRequested?.Invoke();
    private void NextPageButtonClickHandler() => OnNextPageRequested?.Invoke();

    private void ClearSelectedUserPanel()
    {
        _selectedIdText.text = "";
        _selectedFullNameText.text = string.Empty;
        _selectedLoginText.text = string.Empty;
        _selectedEmailText.text = string.Empty;
        _selectedPhoneText.text = string.Empty;
        _selectedCreatedAtText.text = string.Empty;
        _selectedLastLoginAtText.text = string.Empty;

        _selectedRoleDropdown.onValueChanged?.Invoke(0);
        _selectedRoleDropdown.value = 0;
        _ordersAllowedToggle.SetIsOnWithoutNotify(false);
    }
    private void ClearList()
    {
        _createdViews.ForEach(view => Destroy(view));

        _createdViews.Clear();
        _createdViewsByUserId.Clear();
    }

    private void SetSelectDropdownValue(int value)
    {
        _selectedRoleDropdown.value = value;
        _selectedRoleDropdown.SetValueWithoutNotify(value);
        _selectedRoleDropdown.captionText.color = _selectedRoleDropdown.options[value].color;
    }
}