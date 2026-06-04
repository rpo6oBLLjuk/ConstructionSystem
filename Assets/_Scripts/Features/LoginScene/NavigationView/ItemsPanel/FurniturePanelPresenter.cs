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

    [SerializeField] ModelPreviewCameraController _previewCameraController;
    [SerializeField] private FurniturePanelView _view;

    [field: SerializeField] private int _pageSize = 6;

    private int _currentPage = 1;
    private int _totalPages = 1;

    private string _currentSearch;

    private List<FurnitureType> _types = new();
    private List<ColorType> _colors = new();

    private FurnitureViewData _selectedFurniture;


    protected override void OnEnable()
    {
        base.OnEnable();

        _view.OnFurnitureSelected += HandleFurnitureSelected;

        _view.OnNextPageRequested += HandleNextPageRequested;
        _view.OnPreviousPageRequested += HandlePreviousPageRequested;

        _view.OnSearchRequested += HandleSearchRequested;

        _view.OnFurnitureSaveRequested += HandleFurnitureSaveRequested;
        _view.OnOpenModelRequested += HandleOpenModelRequested;
        _view.OnChangeModelRequested += HandleChangeModelRequested;
        _view.OnChangePreviewRequested += HandleChangePreviewRequested;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        _view.OnFurnitureSelected -= HandleFurnitureSelected;

        _view.OnNextPageRequested -= HandleNextPageRequested;
        _view.OnPreviousPageRequested -= HandlePreviousPageRequested;

        _view.OnSearchRequested -= HandleSearchRequested;

        _view.OnFurnitureSaveRequested -= HandleFurnitureSaveRequested;
        _view.OnOpenModelRequested -= HandleOpenModelRequested;
        _view.OnChangeModelRequested -= HandleChangeModelRequested;
        _view.OnChangePreviewRequested -= HandleChangePreviewRequested;
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

        bool canEdit = _userModule.CurrentUser != null &&
                       _userModule.CurrentUser.RoleId == 3;

        _view.SetEditMode(canEdit);

        await LoadPage(1);
    }
    private bool CanEditFurniture() => _userModule.CurrentUser.RoleId >= 3;

    private void HandleFurnitureSelected(FurnitureViewData furniture)
    {
        _selectedFurniture = furniture;
        _view.ShowSelectedFurniture(furniture);
    }

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

    private void HandleSearchRequested(string search)
    {
        _currentSearch = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        LoadPage(1).Forget();
    }

    private async UniTask LoadPage(int page)
    {
        int totalCount = await _furnitureModule.GetFurnitureCount(_currentSearch);

        _totalPages = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)_pageSize));
        _currentPage = Mathf.Clamp(page, 1, _totalPages);

        int offset = (_currentPage - 1) * _pageSize;

        List<Furniture> furniture = await _furnitureModule.GetFurniturePage(
            offset,
            _pageSize,
            _currentSearch
        );

        List<FurnitureViewData> viewData = new();

        foreach (Furniture item in furniture)
            viewData.Add(ConvertToViewData(item));

        _view.SetFurniture(viewData);
        _view.SetPagination(_currentPage, _totalPages);

        //if (viewData.Count > 0)
        //    HandleFurnitureSelected(viewData[0]);
        //else
        //    HandleFurnitureSelected(null);
    }
    private async UniTask SaveFurnitureChanges(FurnitureViewData furnitureViewData)
    {
        if (!IsFurnitureDataValid(furnitureViewData))
            return;

        Furniture source = furnitureViewData.IsNew ? new() : furnitureViewData.SourceFurniture;

        bool changed =
            source.Name != furnitureViewData.Name ||
            source.Description != furnitureViewData.Description ||
            source.FurnitureTypeId != furnitureViewData.FurnitureTypeId ||
            source.ColorTypeId != furnitureViewData.ColorTypeId ||
            source.Manufacturer != furnitureViewData.Manufacturer ||
            !Mathf.Approximately(source.Width, furnitureViewData.Width) ||
            !Mathf.Approximately(source.Height, furnitureViewData.Height) ||
            !Mathf.Approximately(source.Depth, furnitureViewData.Depth) ||
            source.FilePath != furnitureViewData.FilePath ||
            source.ThumbnailPath != furnitureViewData.ThumbnailPath ||
            !Approximately(source.Price, furnitureViewData.Price) ||
            source.IsAvailable != furnitureViewData.IsAvailable;

        if (!changed)
        {
            _notificationService.ShowPopup(
                "The furniture data is identical to the current one.",
                "Furniture save",
                NotificationType.Info
            );

            return;
        }

        string originalModelPath = furnitureViewData.FilePath;
        string originalPreviewPath = furnitureViewData.ThumbnailPath;

        string modelFileName = _furnitureDataSaver.GetFileName(originalModelPath);
        string previewFileName = _furnitureDataSaver.GetFileName(originalPreviewPath);

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

        source.FilePath = modelFileName;
        source.ThumbnailPath = previewFileName;

        if (furnitureViewData.IsNew)
        {
            int id = await _furnitureModule.CreateFurniture(source);

            furnitureViewData.Id = id;
            furnitureViewData.IsNew = false;
            furnitureViewData.SourceFurniture = source;

            furnitureViewData.CreatedAt = GetFormatDate(source.CreatedAt);
        }
        else
            await _furnitureModule.UpdateFurniture(source);

        furnitureViewData.UpdatedAt = GetFormatDate(source.UpdatedAt);

        if (!string.IsNullOrWhiteSpace(originalModelPath) && File.Exists(originalModelPath))
        {
            string savedModelName = _furnitureDataSaver.SaveModelFile(
                source.Id,
                originalModelPath,
                message => DebugWrapper.FastLog(this, message),
                error => DebugWrapper.LogError(this, error)
            );

            if (!string.IsNullOrWhiteSpace(savedModelName))
            {
                source.FilePath = savedModelName;
                furnitureViewData.FilePath = savedModelName;
            }
        }
        if (!string.IsNullOrWhiteSpace(originalPreviewPath) && File.Exists(originalPreviewPath))
        {
            string savedPreviewName = _furnitureDataSaver.SavePreviewFile(
                source.Id,
                originalPreviewPath,
                message => DebugWrapper.FastLog(this, message),
                error => DebugWrapper.LogError(this, error)
            );

            if (!string.IsNullOrWhiteSpace(savedPreviewName))
            {
                source.ThumbnailPath = savedPreviewName;
                furnitureViewData.ThumbnailPath = savedPreviewName;
            }
        }

        await LoadPage(_currentPage);
        _view.RefreshFurniture(furnitureViewData);

        _notificationService.ShowPopup(
            "Furniture data has been saved successfully",
            "Furniture saved",
            NotificationType.Success
        );
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

        string fullModelPath = _furnitureDataSaver.GetModelPath(
            furniture.Id,
            furniture.FilePath
        );

        _furnitureDataSaver.LoadModelGameObject(furniture.Id, furniture.FilePath, _previewCameraController.ModelContainer, onComplete: _ => _previewCameraController.Show()).Forget();
    }
    private void HandleChangeModelRequested(FurnitureViewData furniture)
    {
        if (!CanEditFurniture() || furniture == null)
            return;

        string selectedPath = OpenModelFileDialog();

        if (string.IsNullOrWhiteSpace(selectedPath))
            return;

        furniture.FilePath = selectedPath;

        _view.ShowSelectedFurniture(furniture);
    }
    private void HandleChangePreviewRequested(FurnitureViewData furniture)
    {
        if (!CanEditFurniture() || furniture == null)
            return;

        string selectedPath = OpenPreviewFileDialog();

        if (string.IsNullOrWhiteSpace(selectedPath))
            return;

        furniture.ThumbnailPath = selectedPath;

        _view.ShowSelectedFurniture(furniture);
        _view.RefreshFurniture(furniture);
    }

    private bool IsFurnitureDataValid(FurnitureViewData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            _notificationService.ShowPopup("Furniture name cannot be empty.", "Input warning", NotificationType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(data.FilePath))
        {
            _notificationService.ShowPopup("Model file path cannot be empty.", "Input warning", NotificationType.Warning);
            return false;
        }

        if (data.Width < 0 || data.Height < 0 || data.Depth < 0)
        {
            _notificationService.ShowPopup("Furniture size cannot contain negative values.", "Input warning", NotificationType.Warning);
            return false;
        }

        if (data.Price < 0)
        {
            _notificationService.ShowPopup("Furniture price cannot be negative.", "Input warning", NotificationType.Warning);
            return false;
        }

        return true;
    }

    private string GetFurnitureTypeName(int typeId)
    {
        FurnitureType type = _types.FirstOrDefault(type => type.Id == typeId);
        return type == null ? null : type.Name;
    }
    private string GetColorTypeName(int colorTypeId)
    {
        ColorType color = _colors.FirstOrDefault(color => color.Id == colorTypeId);
        return color == null ? null : color.Name;
    }
    private string GetFormatDate(DateTime dateTime) => $"{dateTime:dd.MM.yyyy} - {dateTime:HH:mm:ss}";

    private bool Approximately(double a, double b)
    {
        return System.Math.Abs(a - b) < 0.001d;
    }
    private FurnitureViewData ConvertToViewData(Furniture furniture)
    {
        return new FurnitureViewData
        {
            Id = furniture.Id,

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

            FilePath = furniture.FilePath,
            ThumbnailPath = furniture.ThumbnailPath,

            Price = furniture.Price,

            IsAvailable = furniture.IsAvailable,

            CreatedAt = GetFormatDate(furniture.CreatedAt),
            UpdatedAt = GetFormatDate(furniture.UpdatedAt),

            SourceFurniture = furniture
        };
    }

    private string OpenModelFileDialog() => StandaloneFileBrowser.OpenFilePanel("Select 3D model", "", new ExtensionFilter[] { new("3D model", _furnitureDataSaver.ModelExtensions) }, false).FirstOrDefault();
    private string OpenPreviewFileDialog() => StandaloneFileBrowser.OpenFilePanel("Select preview image", "", new ExtensionFilter[] { new("Image", _furnitureDataSaver.PreviewExtensions) }, false).FirstOrDefault();
}