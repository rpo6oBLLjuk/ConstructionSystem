using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Coffee.UIEffects;
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
    public event Action<int?> OnFurnitureTypeFilterChanged;
    public event Action<int?> OnColorTypeFilterChanged;

    public event Action<FurnitureViewData> OnFurnitureSaveRequested;
    public event Action<FurnitureViewData> OnOpenModelRequested;
    public event Action<FurnitureViewData> OnChangeModelRequested;
    public event Action<FurnitureViewData> OnChangePreviewRequested;
    public event Action<FurnitureViewData> OnRemoveFurnitureRequested;
    public event Action OnAddFurnitureRequested;

    [Header("List")]
    [SerializeField] private Transform _contentParent;
    [SerializeField] private FurnitureViewFactory _factory;

    [Header("Search")]
    [SerializeField] private TMP_InputField _searchInputField;

    [Header("Filters")]
    [SerializeField] private TMP_Dropdown _typeFilterDropdown;
    [SerializeField] private TMP_Dropdown _colorFilterDropdown;

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

    [SerializeField] private Sprite _defaultPreview;

    private readonly List<GameObject> _createdViews = new();
    private readonly Dictionary<int, GameObject> _createdViewsByFurnitureId = new();

    private readonly List<int> _typeIds = new();
    private readonly List<int> _colorIds = new();

    private FurnitureViewData _selectedFurniture;
    
    private bool _canEdit;


    private void OnEnable()
    {
        _previousPageButton.onClick.AddListener(PreviousPageButtonClickHandler);
        _nextPageButton.onClick.AddListener(NextPageButtonClickHandler);

        _searchInputField.onEndEdit.AddListener(SearchEndEditHandler);
        _typeFilterDropdown.onValueChanged.AddListener(TypeFilterChangedHandler);
        _colorFilterDropdown.onValueChanged.AddListener(ColorFilterChangedHandler);

        _saveButton.onClick.AddListener(SaveButtonClickHandler);
        _openModelButton.onClick.AddListener(OpenModelButtonClickHandler);
        _changeModelButton.onClick.AddListener(ChangeModelButtonClickHandler);
        _changePreviewButton.onClick.AddListener(ChangePreviewButtonClickHandler);
        _addFurnitureButton.onClick.AddListener(AddFurnitureButtonClickHandler);
        _removeFurnitureButton.onClick.AddListener(RemoveFurnitureButtonClickHandler);

        _priceInputField.onSelect.AddListener(value => _priceInputField.SetTextWithoutNotify(double.Parse(value).ToString()));
        _priceInputField.onEndEdit.AddListener((value) =>
        {
            _priceInputField.SetTextWithoutNotify(ConvertDoubleToCurrentCulture(double.Parse(value)));
            _priceInputField.DeactivateInputField();
        });

        ClearSelectedFurniturePanel();
    }
    private void OnDisable()
    {
        _previousPageButton.onClick.RemoveListener(PreviousPageButtonClickHandler);
        _nextPageButton.onClick.RemoveListener(NextPageButtonClickHandler);

        _searchInputField.onEndEdit.RemoveListener(SearchEndEditHandler);
        _typeFilterDropdown.onValueChanged.RemoveListener(TypeFilterChangedHandler);
        _colorFilterDropdown.onValueChanged.RemoveListener(ColorFilterChangedHandler);

        _saveButton.onClick.RemoveListener(SaveButtonClickHandler);
        _openModelButton.onClick.RemoveListener(OpenModelButtonClickHandler);
        _changeModelButton.onClick.RemoveListener(ChangeModelButtonClickHandler);
        _changePreviewButton.onClick.RemoveListener(ChangePreviewButtonClickHandler);
        _addFurnitureButton.onClick.RemoveListener(AddFurnitureButtonClickHandler);
        _removeFurnitureButton.onClick.RemoveListener(RemoveFurnitureButtonClickHandler);
    }

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

            SetUIEffectState(item.UIEffect, false);

            _createdViews.Add(view);
            _createdViewsByFurnitureId[item.Id] = view;
        }
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

        _priceInputField.SetTextWithoutNotify(ConvertDoubleToCurrentCulture(furniture.Price));

        _createdAtInputField.SetTextWithoutNotify(furniture.CreatedAt);
        _updatedAtInputField.SetTextWithoutNotify(furniture.UpdatedAt);

        _descriptionInputField.SetTextWithoutNotify(furniture.Description);

        _statusToggle.SetIsOnWithoutNotify(furniture.IsAvailable);

        _openModelButton.interactable = furniture.HasModel;
        _saveButton.interactable = _canEdit;
        _changeModelButton.interactable = _canEdit;
        _changePreviewButton.interactable = _canEdit;

        _preview.sprite = furniture.HasPreview ? furniture.Preview : _defaultPreview;

        SetEditMode(_canEdit);
    }

    public void SetFurnitureTypes(List<FurnitureType> types)
    {
        _typeDropdown.ClearOptions();
        _typeFilterDropdown.ClearOptions();
        _typeIds.Clear();

        List<string> editOptions = new();
        List<string> filterOptions = new() { "All" };

        foreach (FurnitureType type in types)
        {
            _typeIds.Add(type.Id);
            editOptions.Add(type.Name);
            filterOptions.Add(type.Name);
        }

        _typeDropdown.AddOptions(editOptions);
        _typeFilterDropdown.AddOptions(filterOptions);

        _typeDropdown.SetValueWithoutNotify(0);
        _typeFilterDropdown.SetValueWithoutNotify(0);
    }
    public void SetColorTypes(List<ColorType> colors)
    {
        _colorDropdown.ClearOptions();
        _colorFilterDropdown.ClearOptions();
        _colorIds.Clear();

        List<string> editOptions = new();
        List<string> filterOptions = new() { "All" };

        foreach (ColorType color in colors)
        {
            _colorIds.Add(color.Id);

            editOptions.Add(color.Name);
            filterOptions.Add(color.Name);
        }

        _colorDropdown.AddOptions(editOptions);
        _colorFilterDropdown.AddOptions(filterOptions);

        _colorDropdown.SetValueWithoutNotify(0);
        _colorFilterDropdown.SetValueWithoutNotify(0);
    }

    public void UpdateFurniturePreview(FurnitureViewData furniture, Sprite previewSprite)
    {
        if (furniture == null)
            return;

        furniture.Preview = previewSprite;

        if (_selectedFurniture != null && _selectedFurniture.Id == furniture.Id)
            _preview.sprite = previewSprite != null ? previewSprite : _defaultPreview;
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

        _priceInputField.SetTextWithoutNotify(ConvertDoubleToCurrentCulture(0));

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
        OnFurnitureSaveRequested?.Invoke(_selectedFurniture);
    }

    private void OpenModelButtonClickHandler() => OnOpenModelRequested?.Invoke(_selectedFurniture);
    private void ChangeModelButtonClickHandler() => OnChangeModelRequested?.Invoke(_selectedFurniture);
    private void ChangePreviewButtonClickHandler() => OnChangePreviewRequested?.Invoke(_selectedFurniture);
    private void AddFurnitureButtonClickHandler() => OnAddFurnitureRequested?.Invoke();
    private void RemoveFurnitureButtonClickHandler() => OnRemoveFurnitureRequested?.Invoke(_selectedFurniture);

    private void ApplyPreviewDataToSelectedFurniture()
    {
        _selectedFurniture.Name = _nameInputField.text.Trim();

        _selectedFurniture.FurnitureTypeId = GetDropdownSelectedId(_typeDropdown, _typeIds) ?? 0;
        _selectedFurniture.ColorTypeId = GetDropdownSelectedId(_colorDropdown, _colorIds) ?? 0;

        _selectedFurniture.FurnitureTypeName = GetDropdownText(_typeDropdown);
        _selectedFurniture.ColorTypeName = GetDropdownText(_colorDropdown);

        _selectedFurniture.Manufacturer = _manufacturerInputField.text.Trim();

        _selectedFurniture.Width = ParseFloat(_widthInputField.text);
        _selectedFurniture.Height = ParseFloat(_heightInputField.text);
        _selectedFurniture.Depth = ParseFloat(_depthInputField.text);

        _selectedFurniture.Price = double.Parse(_priceInputField.text);

        _selectedFurniture.IsAvailable = _statusToggle.isOn;
        _selectedFurniture.Description = _descriptionInputField.text.Trim();
    }

    private string GetDropdownText(TMP_Dropdown dropdown)
    {
        if (dropdown.options == null || dropdown.options.Count == 0)
            return null;

        return dropdown.options[dropdown.value].text;
    }
    private int? GetDropdownSelectedId(TMP_Dropdown dropdown, List<int> ids, bool hasAllOption = false)
    {
        if (dropdown == null || ids == null || ids.Count == 0)
            return null;

        int dropdownIndex = dropdown.value;

        if (hasAllOption)
        {
            if (dropdownIndex <= 0)
                return null;

            dropdownIndex--;
        }

        int idIndex = Mathf.Clamp(dropdownIndex, 0, ids.Count - 1);

        return ids[idIndex];
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

    private void SearchEndEditHandler(string value) => OnSearchRequested?.Invoke(value);
    private void TypeFilterChangedHandler(int index) => OnFurnitureTypeFilterChanged?.Invoke(GetDropdownSelectedId(_typeFilterDropdown, _typeIds, true));
    private void ColorFilterChangedHandler(int index) => OnColorTypeFilterChanged?.Invoke(GetDropdownSelectedId(_colorFilterDropdown, _colorIds, true));

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
        if (_selectedFurniture != null)
            SetUIEffectState(_selectedFurniture.UIEffect, false);

        if (newView != null)
            SetUIEffectState(newView.UIEffect, true);
    }
    private void SetUIEffectState(UIEffect reference, bool selected)
    {
        if (reference == null)
            return;

        reference.edgeWidth = selected ? _selectedEdgeWidth : _defaultEdgeWidth;
        reference.edgeColor = selected ? _selectedEdgeColor : _defaultEdgeColor;
    }

    private string ConvertDoubleToCurrentCulture(double value) => value.ToString("N2", CultureInfo.CurrentCulture);
}