using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using SFB;
using UnityEngine;
using Zenject;

public class FurniturePanelPresenter : BaseLayoutPresenter
{
    [Inject] private FurnitureModule _furnitureModule;
    [Inject] private UserModule _userModule;
    [Inject] private NotificationService _notificationService;
    [Inject] private FurnitureDataSaver _furnitureDataSaver;

    [Inject] private ModelPreviewController _previewModelController;

    [SerializeField] private FurniturePanelView _view;

    [field: SerializeField] private int _pageSize = 6;

    private int _currentPage = 1;
    private int _totalPages = 1;

    private string _currentSearch;
    private int? _currentFurnitureTypeId;
    private int? _currentColorTypeId;

    private List<FurnitureType> _types = new();
    private List<ColorType> _colors = new();

    private FurnitureViewData _selectedFurniture;

    //For set preview/model without save
    private FurnitureViewData _pendingFurniture;
    private string _pendingModelPath;
    private string _pendingPreviewPath;
    private byte[] _pendingPreviewBytes;


    protected override void OnEnable()
    {
        base.OnEnable();

        _view.OnFurnitureSelected += HandleFurnitureSelected;

        _view.OnNextPageRequested += HandleNextPageRequested;
        _view.OnPreviousPageRequested += HandlePreviousPageRequested;

        _view.OnSearchRequested += HandleSearchRequested;
        _view.OnFurnitureTypeFilterChanged += HandleFurnitureTypeFilterChanged;
        _view.OnColorTypeFilterChanged += HandleColorTypeFilterChanged;

        _view.OnFurnitureSaveRequested += HandleFurnitureSaveRequested;
        _view.OnOpenModelRequested += HandleOpenModelRequested;
        _view.OnChangeModelRequested += HandleChangeModelRequested;
        _view.OnChangePreviewRequested += HandleChangePreviewRequested;
        _view.OnRemoveFurnitureRequested += HandleRemoveFurniture;
        _view.OnAddFurnitureRequested += HandleFurnitureAdd;

        _previewModelController.PreviewSaveRequested += HandlePreviewSave;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        _view.OnFurnitureSelected -= HandleFurnitureSelected;

        _view.OnNextPageRequested -= HandleNextPageRequested;
        _view.OnPreviousPageRequested -= HandlePreviousPageRequested;

        _view.OnSearchRequested -= HandleSearchRequested;
        _view.OnFurnitureTypeFilterChanged -= HandleFurnitureTypeFilterChanged;
        _view.OnColorTypeFilterChanged -= HandleColorTypeFilterChanged;

        _view.OnFurnitureSaveRequested -= HandleFurnitureSaveRequested;
        _view.OnOpenModelRequested -= HandleOpenModelRequested;
        _view.OnChangeModelRequested -= HandleChangeModelRequested;
        _view.OnChangePreviewRequested -= HandleChangePreviewRequested;
        _view.OnRemoveFurnitureRequested -= HandleRemoveFurniture;
        _view.OnAddFurnitureRequested -= HandleFurnitureAdd;

        _previewModelController.PreviewSaveRequested -= HandlePreviewSave;
    }

    public override void Show()
    {
        base.Show();
        InitializePanel().Forget();
    }
    private async UniTask InitializePanel()
    {
        _types = await _furnitureModule.GetFurnitureTypes();
        _colors = await _furnitureModule.GetColorTypes();

        _view.SetFurnitureTypes(_types);
        _view.SetColorTypes(_colors);

        _view.SetEditMode(CanEditFurniture());

        await LoadPage(1);
    }

    private bool CanEditFurniture() => _userModule.CurrentUser.RoleId >= 3;

    private void HandleFurnitureSelected(FurnitureViewData furniture)
    {
        if (_selectedFurniture != furniture)
            ClearPendingData(resetFurnitureState: true);

        _selectedFurniture = furniture;
        _view.ShowSelectedFurniture(furniture);
    }

    private void HandleNextPageRequested()
    {
        if (_currentPage >= _totalPages)
            return;

        ClearPendingData(resetFurnitureState: true);
        LoadPage(_currentPage + 1).Forget();
    }
    private void HandlePreviousPageRequested()
    {
        if (_currentPage <= 1)
            return;

        ClearPendingData(resetFurnitureState: true);
        LoadPage(_currentPage - 1).Forget();
    }

    private void HandleSearchRequested(string search)
    {
        ClearPendingData(resetFurnitureState: true);

        _currentSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        LoadPage(1).Forget();
    }
    private void HandleFurnitureTypeFilterChanged(int? furnitureTypeId)
    {
        ClearPendingData(resetFurnitureState: true);

        _currentFurnitureTypeId = furnitureTypeId;
        LoadPage(1).Forget();
    }
    private void HandleColorTypeFilterChanged(int? colorTypeId)
    {
        ClearPendingData(resetFurnitureState: true);

        _currentColorTypeId = colorTypeId;
        LoadPage(1).Forget();
    }

    private async UniTask LoadPage(int page, int selectedItemId = -1)
    {
        int totalCount = await _furnitureModule.GetFurnitureCount(_currentSearch, _currentFurnitureTypeId, _currentColorTypeId);

        _totalPages = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)_pageSize));
        _currentPage = Mathf.Clamp(page, 1, _totalPages);

        int offset = (_currentPage - 1) * _pageSize;

        List<Furniture> furniture = await _furnitureModule.GetFurniturePage(offset, _pageSize, _currentSearch, _currentFurnitureTypeId, _currentColorTypeId);
        List<FurnitureViewData> viewData = new();

        foreach (Furniture item in furniture)
            viewData.Add(ConvertToViewData(item));

        _view.SetFurniture(viewData);
        _view.SetPagination(_currentPage, _totalPages);

        if (selectedItemId != -1)
            HandleFurnitureSelected(viewData.Where(data => data.Id == selectedItemId).FirstOrDefault());
    }
    private async UniTask SaveFurnitureChanges(FurnitureViewData furnitureViewData)
    {
        if (!IsFurnitureDataValid(furnitureViewData))
            return;

        if (!IsFurnitureDataChanged(furnitureViewData))
            return;

        Furniture source = furnitureViewData.IsNew ? new() : furnitureViewData.SourceFurniture;
        source.Id = furnitureViewData.Id;

        source.Name = furnitureViewData.Name;
        source.Description = furnitureViewData.Description;

        source.FurnitureTypeId = furnitureViewData.FurnitureTypeId;
        source.ColorTypeId = furnitureViewData.ColorTypeId;

        source.Manufacturer = furnitureViewData.Manufacturer;

        source.Width = furnitureViewData.Width;
        source.Height = furnitureViewData.Height;
        source.Depth = furnitureViewData.Depth;

        source.Price = furnitureViewData.Price;
        source.IsAvailable = furnitureViewData.IsAvailable;

        source.HasModel = furnitureViewData.HasModel;
        source.HasPreview = furnitureViewData.HasPreview;

        if (!TrySavePendingFiles(furnitureViewData, source))
        {
            _notificationService.ShowPopup("Furniture files could not be saved.", "Saving error", NotificationType.Error);
            return;
        }

        if (furnitureViewData.IsNew)
        {
            await _furnitureModule.CreateFurnitureWithCustomId(source);

            furnitureViewData.IsNew = false;
            furnitureViewData.CreatedAt = GetFormatDate(source.CreatedAt);
        }
        else
        {
            await _furnitureModule.UpdateFurniture(source);
        }

        furnitureViewData.SourceFurniture = source;
        furnitureViewData.UpdatedAt = GetFormatDate(source.UpdatedAt);
        furnitureViewData.ModelOrPreviewChanged = false;

        ClearPendingData(resetFurnitureState: false);

        await LoadPage(_currentPage, furnitureViewData.Id);

        if (_selectedFurniture != null)
            _selectedFurniture.Preview = furnitureViewData.Preview;

        HandleFurnitureSelected(_selectedFurniture);

        _notificationService.ShowPopup($"Furniture <b>{furnitureViewData.Name}</b> data has been saved successfully", "Furniture saved", NotificationType.Success);
    }

    private void HandleFurnitureSaveRequested(FurnitureViewData furnitureViewData)
    {
        if (!CanEditFurniture())
            return;

        SaveFurnitureChanges(furnitureViewData).Forget();
    }
    private void HandleOpenModelRequested(FurnitureViewData furniture)
    {
        if (furniture == null)
            return;

        ShowModelPreview(furniture);

    }
    private void HandleChangeModelRequested(FurnitureViewData furniture)
    {
        if (!CanEditFurniture() || furniture == null)
            return;

        string selectedPath = OpenModelFileDialog();

        if (string.IsNullOrWhiteSpace(selectedPath))
            return;

        SetPendingFurniture(furniture);

        _pendingModelPath = selectedPath;

        furniture.HasModel = true;
        furniture.ModelOrPreviewChanged = true;

        ShowModelPreview(furniture);

        _view.SetEditMode(CanEditFurniture());

        _notificationService.ShowPopup("Model has been selected. Press Save to apply changes.", "Model selected", NotificationType.Info);
    }

    private void HandleChangePreviewRequested(FurnitureViewData furniture)
    {
        if (!CanEditFurniture() || furniture == null)
            return;

        string selectedPath = OpenPreviewFileDialog();

        if (string.IsNullOrWhiteSpace(selectedPath))
            return;

        SetPendingFurniture(furniture);

        _pendingPreviewPath = selectedPath;
        _pendingPreviewBytes = null;

        furniture.HasPreview = true;
        furniture.ModelOrPreviewChanged = true;

        _furnitureDataSaver.LoadPreviewByAbsolutePath(selectedPath,
            sprite =>
            {
                furniture.Preview = sprite;
                _view.UpdateFurniturePreview(furniture, sprite);
            }
        ).Forget();

        _notificationService.ShowPopup("Preview has been selected. Press Save to apply changes.", "Preview selected", NotificationType.Info);
    }
    private void HandlePreviewSave(byte[] bytes, Texture2D texture2d)
    {
        if (!CanEditFurniture() || _selectedFurniture == null)
            return;

        SetPendingFurniture(_selectedFurniture);

        _pendingPreviewPath = null;
        _pendingPreviewBytes = bytes;

        _selectedFurniture.HasPreview = true;
        _selectedFurniture.ModelOrPreviewChanged = true;

        _selectedFurniture.Preview = texture2d;
        _view.UpdateFurniturePreview(_selectedFurniture, _selectedFurniture.Preview);

        _notificationService.ShowPopup("Preview has been generated. Press Save to apply changes.", "Preview generated", NotificationType.Info);
    }

    private async void HandleFurnitureAdd()
    {
        ClearPendingData(resetFurnitureState: true);

        int newId = await _furnitureModule.GetNextId();

        int defaultTypeId = _types.Count > 0 ? _types[0].Id : 0;
        int defaultColorId = _colors.Count > 0 ? _colors[0].Id : 0;

        FurnitureViewData newFurniture = new()
        {
            Id = newId,
            IsNew = true,

            Name = string.Empty,
            Description = string.Empty,
            Manufacturer = string.Empty,

            FurnitureTypeId = defaultTypeId,
            FurnitureTypeName = GetFurnitureTypeName(defaultTypeId),

            ColorTypeId = defaultColorId,
            ColorTypeName = GetColorTypeName(defaultColorId),

            Width = 0,
            Height = 0,
            Depth = 0,

            Price = 0,

            HasModel = false,
            HasPreview = false,

            IsAvailable = true,

            CreatedAt = "-",
            UpdatedAt = "-",

            SourceFurniture = null
        };

        HandleFurnitureSelected(newFurniture);
    }
    private async void HandleRemoveFurniture(FurnitureViewData furniture)
    {
        if (furniture == null)
            return;

        ClearPendingData(resetFurnitureState: false);

        if (furniture.SourceFurniture != null)
            await _furnitureModule.DeleteFurniture(furniture.SourceFurniture);

        _furnitureDataSaver.DeleteFurnitureData(furniture.Id);

        _notificationService.ShowPopup("Deletion completed successfully", "Delete success", NotificationType.Success);

        _selectedFurniture = null;
        _view.ClearSelectedFurniturePanel();

        await LoadPage(_currentPage);
    }

    private void SetPendingFurniture(FurnitureViewData furniture)
    {
        if (_pendingFurniture != null && _pendingFurniture != furniture)
            ClearPendingData(resetFurnitureState: true);

        _pendingFurniture = furniture;
    }

    private bool HasPendingFor(FurnitureViewData furniture) => furniture != null && _pendingFurniture == furniture;
    private bool HasPendingModelFor(FurnitureViewData furniture) => HasPendingFor(furniture) && !string.IsNullOrWhiteSpace(_pendingModelPath);
    private bool TrySavePendingFiles(FurnitureViewData furnitureViewData, Furniture source)
    {
        if (!HasPendingFor(furnitureViewData))
            return true;

        if (!string.IsNullOrWhiteSpace(_pendingModelPath))
        {
            bool modelSaved = _furnitureDataSaver.SaveModelFile(source.Id, _pendingModelPath, message => DebugWrapper.InactiveLog(this, message), error => DebugWrapper.LogError(this, error));

            if (!modelSaved)
                return false;

            source.HasModel = true;
            furnitureViewData.HasModel = true;
        }

        if (_pendingPreviewBytes != null)
        {
            bool previewSaved = _furnitureDataSaver.SavePreviewBytes(source.Id, _pendingPreviewBytes, message => DebugWrapper.InactiveLog(this, message), error => DebugWrapper.LogError(this, error));

            if (!previewSaved)
                return false;

            source.HasPreview = true;
            furnitureViewData.HasPreview = true;
        }
        else if (!string.IsNullOrWhiteSpace(_pendingPreviewPath))
        {
            bool previewSaved = _furnitureDataSaver.SavePreviewFile(source.Id, _pendingPreviewPath, message => DebugWrapper.InactiveLog(this, message), error => DebugWrapper.LogError(this, error));

            if (!previewSaved)
                return false;

            source.HasPreview = true;
            furnitureViewData.HasPreview = true;
        }

        return true;
    }

    private void ClearPendingData(bool resetFurnitureState)
    {
        if (resetFurnitureState && _pendingFurniture != null)
            ResetPendingFurnitureState(_pendingFurniture);

        _pendingFurniture = null;

        _pendingModelPath = null;

        _pendingPreviewPath = null;
        _pendingPreviewBytes = null;
    }
    private void ResetPendingFurnitureState(FurnitureViewData furniture)
    {
        furniture.ModelOrPreviewChanged = false;

        if (furniture.SourceFurniture == null)
        {
            _view.UpdateFurniturePreview(furniture, null);
            return;
        }

        furniture.HasModel = furniture.SourceFurniture.HasModel;
        furniture.HasPreview = furniture.SourceFurniture.HasPreview;

        if (!furniture.HasPreview)
        {
            furniture.Preview = null;
            _view.UpdateFurniturePreview(furniture, null);
            return;
        }

        _furnitureDataSaver.LoadPreviewSprite(furniture.Id,
            sprite =>
            {
                furniture.Preview = sprite;
                _view.UpdateFurniturePreview(furniture, sprite);
            },
            error =>
            {
                furniture.Preview = null;
                _view.UpdateFurniturePreview(furniture, null);
                DebugWrapper.LogError(this, error);
            }
        ).Forget();
    }

    private bool IsFurnitureDataValid(FurnitureViewData data)
    {
        if (data == null)
            return false;

        if (string.IsNullOrWhiteSpace(data.Name))
        {
            _notificationService.ShowPopup("Furniture <u>name</u> cannot be empty.", "Input warning", NotificationType.Warning);
            return false;
        }

        if (!data.HasModel)
        {
            _notificationService.ShowPopup("<u>Model</u> cannot be empty.", "Input warning", NotificationType.Warning);
            return false;
        }

        if (data.Width < 0 || data.Height < 0 || data.Depth < 0)
        {
            _notificationService.ShowPopup("Furniture <u>size<u> cannot contain <b>negative</b> values.", "Input warning", NotificationType.Warning);
            return false;
        }

        if (data.Price < 0)
        {
            _notificationService.ShowPopup("Furniture <u>price</u> cannot be negative.", "Input warning", NotificationType.Warning);
            return false;
        }

        return true;
    }
    private bool IsFurnitureDataChanged(FurnitureViewData data)
    {
        if (data.IsNew)
            return true;

        if (data.SourceFurniture == null)
            return true;

        Furniture source = data.SourceFurniture;

        bool changed =
            data.Name?.Trim() != source.Name?.Trim() ||
            data.Description?.Trim() != source.Description?.Trim() ||
            data.Manufacturer?.Trim() != source.Manufacturer?.Trim() ||

            data.FurnitureTypeId != source.FurnitureTypeId ||
            data.ColorTypeId != source.ColorTypeId ||

            Mathf.Abs(data.Width - source.Width) > 0.001f ||
            Mathf.Abs(data.Height - source.Height) > 0.001f ||
            Mathf.Abs(data.Depth - source.Depth) > 0.001f ||

            Math.Abs(data.Price - source.Price) > 0.001 ||

            data.HasModel != source.HasModel ||
            data.HasPreview != source.HasPreview ||
            data.IsAvailable != source.IsAvailable ||
            data.ModelOrPreviewChanged;

        if (!changed)
            _notificationService.ShowPopup("Furniture data has <u>not been changed</u>", "Saving canceled", NotificationType.Info);

        return changed;
    }

    private string GetFurnitureTypeName(int typeId) => _types.FirstOrDefault(type => type.Id == typeId)?.Name;
    private string GetColorTypeName(int colorTypeId) => _colors.FirstOrDefault(color => color.Id == colorTypeId)?.Name;

    private string GetFormatDate(DateTime dateTime) => $"{dateTime:dd.MM.yyyy} - {dateTime:HH:mm:ss}";

    private FurnitureViewData ConvertToViewData(Furniture furniture) => new()
    {
        Id = furniture.Id,
        IsNew = false,

        Name = furniture.Name,
        Description = furniture.Description,

        FurnitureTypeId = furniture.FurnitureTypeId,
        FurnitureTypeName = GetFurnitureTypeName(furniture.FurnitureTypeId),

        ColorTypeId = furniture.ColorTypeId,
        ColorTypeName = GetColorTypeName(furniture.ColorTypeId),

        Manufacturer = furniture.Manufacturer,

        Width = furniture.Width,
        Height = furniture.Height,
        Depth = furniture.Depth,

        HasModel = furniture.HasModel,
        HasPreview = furniture.HasPreview,

        Price = furniture.Price,

        IsAvailable = furniture.IsAvailable,

        CreatedAt = GetFormatDate(furniture.CreatedAt),
        UpdatedAt = GetFormatDate(furniture.UpdatedAt),

        SourceFurniture = furniture
    };

    private string OpenModelFileDialog()
    {
        string path = StandaloneFileBrowser.OpenFilePanel("Select 3D model", "", new ExtensionFilter[] { new("3D model", _furnitureDataSaver.ModelExtensions) }, false).FirstOrDefault();

        this.InactiveLog($"File dialog for <b>3d model</b> opened, selected path: '<u>{path}</u>'");
        return path;
    }
    private string OpenPreviewFileDialog()
    {
        string path = StandaloneFileBrowser.OpenFilePanel("Select preview image", "", new ExtensionFilter[] { new("Image", _furnitureDataSaver.PreviewExtensions) }, false).FirstOrDefault();

        this.InactiveLog($"File dialog for <b>Preview</b> opened, selected path: '<u>{path}</u>'");
        return path;
    }

    private void ShowModelPreview(FurnitureViewData furniture)
    {
        if (furniture == null)
            return;

        _previewModelController.SetUserAccess(CanEditFurniture());

        if (HasPendingModelFor(furniture))
            _furnitureDataSaver.LoadModelByAbsolutePath(_pendingModelPath, _previewModelController.ModelContainer, onComplete: _ => _previewModelController.Show(), onError: error => DebugWrapper.LogError(this, error)).Forget();
        else
            _furnitureDataSaver.LoadModelGameObject(furniture.Id, _previewModelController.ModelContainer, onComplete: _ => _previewModelController.Show(), onError: error => DebugWrapper.LogError(this, error)).Forget();
    }
}