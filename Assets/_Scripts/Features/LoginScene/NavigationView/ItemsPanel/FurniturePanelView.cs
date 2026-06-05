using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class FurniturePanelView : MonoBehaviour
{
    [Inject] FurnitureDataSaver _furnitureDataSaver;

    public event Action<FurnitureViewData> OnFurnitureSelected;

    public event Action OnNextPageRequested;
    public event Action OnPreviousPageRequested;

    public event Action<string> OnSearchRequested;

    public event Action<FurnitureViewData> OnFurnitureSaveRequested;
    public event Action<FurnitureViewData> OnOpenModelRequested;
    public event Action<FurnitureViewData> OnChangeModelRequested;
    public event Action<FurnitureViewData> OnChangePreviewRequested;
    public event Action<FurnitureViewData> OnRemoveFurnitureRequsted;
    public event Action OnAddFurnitureRequested;

    [Header("List")]
    [SerializeField] private Transform _contentParent;
    [SerializeField] private FurnitureViewFactory _factory;

    [Header("Search")]
    [SerializeField] private TMP_InputField _searchInputField;

    [Header("Pagination")]
    [SerializeField] private Button _previousPageButton;
    [SerializeField] private Button _nextPageButton;
    [SerializeField] private TMP_Text _pageText;

    [Header("Selected furniture fields")]
    [SerializeField] private Image _preview;

    [SerializeField] private TMP_InputField _idInputField;
    [SerializeField] private TMP_InputField _nameInputField;
    [SerializeField] private TMP_Dropdown _typeDropdown;
    [SerializeField] private TMP_Dropdown _colorDropdown;
    [SerializeField] private TMP_InputField _manufacturerInputField;

    [SerializeField] private TMP_InputField _widthInputField;
    [SerializeField] private TMP_InputField _heightInputField;
    [SerializeField] private TMP_InputField _depthInputField;
    [SerializeField] private TMP_InputField _priceInputField;

    [SerializeField] private Toggle _statusToggle;

    [SerializeField] private TMP_InputField _createdAtInputField;
    [SerializeField] private TMP_InputField _updatedAtInputField;

    [SerializeField] private TMP_InputField _descriptionInputField;

    [Header("Buttons")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _changeModelButton;
    [SerializeField] private Button _changePreviewButton;
    [SerializeField] private Button _openModelButton;
    [SerializeField] private Button _addFurnitureButton;
    [SerializeField] private Button _removeFurnitureButton;

    [Header("View settings")]
    [SerializeField] private float _defaultEdgeWidth = 0.1f;
    [SerializeField] private float _selectedEdgeWidth = 0.5f;

    [SerializeField] private Color _defaultEdgeColor = Color.gray;
    [SerializeField] private Color _selectedEdgeColor = Color.orange;

    private readonly List<GameObject> _createdViews = new();
    private readonly Dictionary<int, GameObject> _createdViewsByFurnitureId = new();

    private readonly List<int> _typeIds = new();
    private readonly List<int> _colorIds = new();

    private FurnitureViewData _selectedFurniture;
    private Sprite _defaultPreview;
    private bool _canEdit;

    private UIEffect _previousSelectedView;


    private void OnEnable()
    {
        _previousPageButton.onClick.AddListener(PreviousPageButtonClickHandler);
        _nextPageButton.onClick.AddListener(NextPageButtonClickHandler);

        _searchInputField.onEndEdit.AddListener(SearchEndEditHandler);

        _saveButton.onClick.AddListener(SaveButtonClickHandler);
        _openModelButton.onClick.AddListener(OpenModelButtonClickHandler);
        _changeModelButton.onClick.AddListener(ChangeModelButtonClickHandler);
        _changePreviewButton.onClick.AddListener(ChangePreviewButtonClickHandler);
        _addFurnitureButton.onClick.AddListener(AddFurnitureButtonClickHandler);
        _removeFurnitureButton.onClick.AddListener(RemoveFurnitureButtonClickHandler);
    }
    private void OnDisable()
    {
        _previousPageButton.onClick.RemoveListener(PreviousPageButtonClickHandler);
        _nextPageButton.onClick.RemoveListener(NextPageButtonClickHandler);

        _searchInputField.onEndEdit.RemoveListener(SearchEndEditHandler);

        _saveButton.onClick.RemoveListener(SaveButtonClickHandler);
        _openModelButton.onClick.RemoveListener(OpenModelButtonClickHandler);
        _changeModelButton.onClick.RemoveListener(ChangeModelButtonClickHandler);
        _changePreviewButton.onClick.RemoveListener(ChangePreviewButtonClickHandler);
        _addFurnitureButton.onClick.RemoveListener(AddFurnitureButtonClickHandler);
        _removeFurnitureButton.onClick.RemoveListener(RemoveFurnitureButtonClickHandler);
    }

    private void Start() => _defaultPreview = _preview.sprite;

    public void SetEditMode(bool canEdit)
    {
        _canEdit = canEdit;

        // Always blocked
        _idInputField.interactable = false;
        _createdAtInputField.interactable = false;
        _updatedAtInputField.interactable = false;

        // Editable fields
        _nameInputField.interactable = canEdit && _selectedFurniture != null;
        _typeDropdown.interactable = canEdit && _selectedFurniture != null;
        _colorDropdown.interactable = canEdit && _selectedFurniture != null;
        _manufacturerInputField.interactable = canEdit && _selectedFurniture != null;

        _widthInputField.interactable = canEdit && _selectedFurniture != null;
        _heightInputField.interactable = canEdit && _selectedFurniture != null;
        _depthInputField.interactable = canEdit && _selectedFurniture != null;
        _priceInputField.interactable = canEdit && _selectedFurniture != null;

        _statusToggle.interactable = canEdit && _selectedFurniture != null;
        _descriptionInputField.interactable = canEdit && _selectedFurniture != null;

        _saveButton.gameObject.SetActive(canEdit);
        _changeModelButton.gameObject.SetActive(canEdit);
        _changePreviewButton.gameObject.SetActive(canEdit);
        _removeFurnitureButton.gameObject.SetActive(canEdit);

        _saveButton.interactable = _selectedFurniture != null;
        _changeModelButton.interactable = _selectedFurniture != null;
        _changePreviewButton.interactable = _selectedFurniture != null;
        _removeFurnitureButton.interactable = _selectedFurniture != null && !_selectedFurniture.IsNew;

        _addFurnitureButton.gameObject.SetActive(canEdit);
        _openModelButton.interactable = _selectedFurniture != null && _selectedFurniture.HasModel;
    }

    public void SetFurniture(List<FurnitureViewData> furniture)
    {
        ClearList();

        if (furniture == null)
            return;

        foreach (FurnitureViewData item in furniture)
        {
            GameObject view = _factory.Create(
                item,
                _contentParent,
                FurnitureSelectionHandler
            );

            //DisablePreviousUIEffect(item);

            _createdViews.Add(view);
            _createdViewsByFurnitureId[item.Id] = view;
        }

        DisablePreviousUIEffect(null);
    }
    public void SetPagination(int currentPage, int totalPages)
    {
        _pageText.text = $"{currentPage}/{totalPages}";

        _previousPageButton.interactable = currentPage > 1;
        _nextPageButton.interactable = currentPage < totalPages;
    }

    public void ShowSelectedFurniture(FurnitureViewData furniture)
    {
        DisablePreviousUIEffect(furniture);

        _selectedFurniture = furniture;

        if (furniture == null)
        {
            ClearSelectedFurniturePanel();
            return;
        }

        _idInputField.SetTextWithoutNotify(furniture.Id.ToString());
        _nameInputField.SetTextWithoutNotify(furniture.Name);

        SetDropdownValueById(_typeDropdown, _typeIds, furniture.FurnitureTypeId);
        SetDropdownValueById(_colorDropdown, _colorIds, furniture.ColorTypeId);

        _manufacturerInputField.SetTextWithoutNotify(furniture.Manufacturer);

        _widthInputField.SetTextWithoutNotify(furniture.Width.ToString("0.##"));
        _heightInputField.SetTextWithoutNotify(furniture.Height.ToString("0.##"));
        _depthInputField.SetTextWithoutNotify(furniture.Depth.ToString("0.##"));

        _priceInputField.SetTextWithoutNotify(furniture.Price.ToString("0.##"));

        _createdAtInputField.SetTextWithoutNotify(furniture.CreatedAt);
        _updatedAtInputField.SetTextWithoutNotify(furniture.UpdatedAt);

        _descriptionInputField.SetTextWithoutNotify(furniture.Description);

        _statusToggle.SetIsOnWithoutNotify(furniture.IsAvailable);

        _openModelButton.interactable = true;
        _saveButton.interactable = _canEdit;
        _changeModelButton.interactable = _canEdit;
        _changePreviewButton.interactable = _canEdit;

        _preview.sprite = furniture.Preview != null ? furniture.Preview : _defaultPreview;

        SetEditMode(_canEdit);
    }

    public void SetFurnitureTypes(List<FurnitureType> types)
    {
        _typeDropdown.ClearOptions();
        _typeIds.Clear();

        List<string> options = new();

        foreach (FurnitureType type in types)
        {
            _typeIds.Add(type.Id);
            options.Add(type.Name);
        }

        _typeDropdown.AddOptions(options);
    }
    public void SetColorTypes(List<ColorType> colors)
    {
        _colorDropdown.ClearOptions();
        _colorIds.Clear();

        List<string> options = new();

        foreach (ColorType color in colors)
        {
            _colorIds.Add(color.Id);
            options.Add(color.Name);
        }

        _colorDropdown.AddOptions(options);
    }

    public void RefreshFurniture(FurnitureViewData furniture)
    {
        if (_createdViewsByFurnitureId.TryGetValue(furniture.Id, out GameObject viewObject))
            _factory.Refresh(viewObject, furniture);

        DisablePreviousUIEffect(furniture);
    }
    public void ClearSelectedFurniturePanel()
    {
        _selectedFurniture = null;

        _idInputField.SetTextWithoutNotify("");
        _nameInputField.SetTextWithoutNotify("");

        _manufacturerInputField.SetTextWithoutNotify("");

        _widthInputField.SetTextWithoutNotify("");
        _heightInputField.SetTextWithoutNotify("");
        _depthInputField.SetTextWithoutNotify("");

        _priceInputField.SetTextWithoutNotify("");

        _createdAtInputField.SetTextWithoutNotify("");
        _updatedAtInputField.SetTextWithoutNotify("");

        _descriptionInputField.SetTextWithoutNotify("");

        _typeDropdown.SetValueWithoutNotify(0);
        _colorDropdown.SetValueWithoutNotify(0);
        _statusToggle.SetIsOnWithoutNotify(false);

        _saveButton.interactable = false;
        _changeModelButton.interactable = false;
        _changePreviewButton.interactable = false;
        _openModelButton.interactable = false;

        _preview.sprite = _defaultPreview;

        SetEditMode(_canEdit);
        DisablePreviousUIEffect(null);
    }

    private void FurnitureSelectionHandler(FurnitureViewData furniture)
    {
        //ShowSelectedFurniture(furniture);
        OnFurnitureSelected?.Invoke(furniture);
    }
    private void SaveButtonClickHandler()
    {
        if (_selectedFurniture == null || !_canEdit)
            return;

        ApplyPreviewDataToSelectedFurniture();

        FurnitureViewData current = _selectedFurniture;
        OnFurnitureSaveRequested?.Invoke(_selectedFurniture);

        if (current != null)
            ShowSelectedFurniture(current);
    }

    private void OpenModelButtonClickHandler() => OnOpenModelRequested?.Invoke(_selectedFurniture);
    private void ChangeModelButtonClickHandler() => OnChangeModelRequested?.Invoke(_selectedFurniture);
    private void ChangePreviewButtonClickHandler() => OnChangePreviewRequested?.Invoke(_selectedFurniture);
    private void AddFurnitureButtonClickHandler() => OnAddFurnitureRequested?.Invoke();
    private void RemoveFurnitureButtonClickHandler() => OnRemoveFurnitureRequsted?.Invoke(_selectedFurniture);

    private void ApplyPreviewDataToSelectedFurniture()
    {
        _selectedFurniture.Name = _nameInputField.text.Trim();

        _selectedFurniture.FurnitureTypeId = GetSelectedId(_typeDropdown, _typeIds);
        _selectedFurniture.ColorTypeId = GetSelectedId(_colorDropdown, _colorIds);

        _selectedFurniture.FurnitureTypeName = GetDropdownText(_typeDropdown);
        _selectedFurniture.ColorTypeName = GetDropdownText(_colorDropdown);

        _selectedFurniture.Manufacturer = _manufacturerInputField.text.Trim();

        _selectedFurniture.Width = ParseFloat(_widthInputField.text);
        _selectedFurniture.Height = ParseFloat(_heightInputField.text);
        _selectedFurniture.Depth = ParseFloat(_depthInputField.text);

        _selectedFurniture.Price = ParseDouble(_priceInputField.text);

        _selectedFurniture.IsAvailable = _statusToggle.isOn;
        _selectedFurniture.Description = _descriptionInputField.text.Trim();
    }

    private string GetDropdownText(TMP_Dropdown dropdown)
    {
        if (dropdown.options == null || dropdown.options.Count == 0)
            return null;

        return dropdown.options[dropdown.value].text;
    }
    private int GetSelectedId(TMP_Dropdown dropdown, List<int> ids)
    {
        if (ids.Count == 0)
            return 0;

        int index = Mathf.Clamp(dropdown.value, 0, ids.Count - 1);
        return ids[index];
    }

    private void SetDropdownValueById(TMP_Dropdown dropdown, List<int> ids, int id)
    {
        int index = ids.IndexOf(id);

        if (index < 0)
            index = 0;

        dropdown.SetValueWithoutNotify(index);
    }

    private float ParseFloat(string value)
    {
        return float.TryParse(value.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float result)
            ? result
            : 0f;
    }
    private double ParseDouble(string value)
    {
        return double.TryParse(value.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result)
            ? result
            : 0d;
    }

    private void SearchEndEditHandler(string value) => OnSearchRequested?.Invoke(value);
    private void PreviousPageButtonClickHandler() => OnPreviousPageRequested?.Invoke();
    private void NextPageButtonClickHandler() => OnNextPageRequested?.Invoke();

    private void ClearList()
    {
        foreach (GameObject view in _createdViews)
        {
            if (view != null)
                Destroy(view);
        }

        _createdViews.Clear();
        _createdViewsByFurnitureId.Clear();
    }

    private void DisablePreviousUIEffect(FurnitureViewData newView)
    {
        if (_previousSelectedView != null)
            SetUIEffectState(_previousSelectedView, false);

        if (newView != null && newView.ViewObject != null)
        {
            _previousSelectedView = newView.ViewObject.GetComponentInChildren<UIEffect>();
            SetUIEffectState(_previousSelectedView, true);
        }
    }
    private void SetUIEffectState(UIEffect reference, bool selected)
    {
        if (reference == null)
            return;

        reference.edgeWidth = selected ? _selectedEdgeWidth : _defaultEdgeWidth;
        reference.edgeColor = selected ? _selectedEdgeColor : _defaultEdgeColor;
    }
}