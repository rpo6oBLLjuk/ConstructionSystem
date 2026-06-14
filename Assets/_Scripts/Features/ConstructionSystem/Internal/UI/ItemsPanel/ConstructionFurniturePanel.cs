using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionFurniturePanel : BaseSlidePanel
    {
        [Header("References")]
        [Inject] private ConstructionManager _constructionManager;
        [Inject] private FurnitureModule _furnitureModule;
        [Inject] private NotificationService _notificationService;

        [SerializeField] private ConstructionFurnitureItemFactory _factory;

        [Header("Filters")]
        [SerializeField] private TMP_InputField _searchInput;
        [SerializeField] private TMP_Dropdown _typeDropdown;
        [SerializeField] private TMP_Dropdown _colorDropdown;

        [Header("Pagination")]
        [SerializeField] private TMP_Text _pageText;
        [SerializeField] private Button _previousPageButton;
        [SerializeField] private Button _nextPageButton;
        [SerializeField] private int _pageSize = 4;

        private int _currentPage = 1;
        private int _totalPages = 1;

        private string _search;
        private int? _furnitureTypeId;
        private int? _colorTypeId;

        private List<FurnitureType> _types = new();
        private List<ColorType> _colors = new();

        private readonly List<int> _typeIds = new();
        private readonly List<int> _colorIds = new();

        private ConstructionFurnitureItemView _selectedView;


        protected override void OnEnable()
        {
            base.OnEnable();

            _previousPageButton.onClick.AddListener(PreviousPageButtonClickHandler);
            _nextPageButton.onClick.AddListener(NextPageButtonClickHandler);

            _searchInput.onEndEdit.AddListener(SearchEndEditHandler);
            _typeDropdown.onValueChanged.AddListener(TypeDropdownChangedHandler);
            _colorDropdown.onValueChanged.AddListener(ColorDropdownChangedHandler);

            _constructionManager.FurniturePrototypeDeselected += DeselectFurnitureHandler;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            _previousPageButton.onClick.RemoveListener(PreviousPageButtonClickHandler);
            _nextPageButton.onClick.RemoveListener(NextPageButtonClickHandler);

            _searchInput.onEndEdit.RemoveListener(SearchEndEditHandler);
            _typeDropdown.onValueChanged.RemoveListener(TypeDropdownChangedHandler);
            _colorDropdown.onValueChanged.RemoveListener(ColorDropdownChangedHandler);

            _constructionManager.FurniturePrototypeDeselected += DeselectFurnitureHandler;
        }

        protected override void OnPanelInitialized() => Initialize().Forget();


        private async UniTaskVoid Initialize()
        {
            _types = await _furnitureModule.GetFurnitureTypes();
            _colors = await _furnitureModule.GetColorTypes();

            FillTypeDropdown();
            FillColorDropdown();

            await LoadPage(1);
        }
        private async UniTask LoadPage(int page)
        {
            int totalCount = await _furnitureModule.GetFurnitureCount(_search, _furnitureTypeId, _colorTypeId);

            _totalPages = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)_pageSize));
            _currentPage = Mathf.Clamp(page, 1, _totalPages);

            int offset = (_currentPage - 1) * _pageSize;

            List<Furniture> furniture = await _furnitureModule.GetFurniturePage(offset, _pageSize, _search, _furnitureTypeId, _colorTypeId);

            SetFurniture(furniture);
            UpdatePagination();
        }

        private void SetFurniture(List<Furniture> furniture)
        {
            _factory.Clear();

            foreach (Furniture item in furniture)
            {
                string typeName = GetFurnitureTypeName(item.FurnitureTypeId);
                string colorName = GetColorTypeName(item.ColorTypeId);

                _factory.Create(item, typeName, colorName, FurnitureClickHandler);
            }
        }
        private void FurnitureClickHandler(ConstructionFurnitureItemView view, Furniture furniture)
        {
            if (furniture == null)
                return;

            SelectView(view);

            if (furniture.IsAvailable)
            {
                _constructionManager.SelectFurniturePrototype(furniture);
                return;
            }

            _notificationService.ShowDialog(
                $"Item <b>{furniture.Name}</b> unavailable for order.\nDo you want to select and place it?",
                "Unavailable item",
                new List<(string, Action)>()
                {
                    ("Cancel", null),
                    ("Continue", () => _constructionManager.SelectFurniturePrototype(furniture))
                }
            );
        }

        private void SelectView(ConstructionFurnitureItemView view)
        {
            if (_selectedView != null)
                _selectedView.SetSelected(false);

            _selectedView = view;

            if (_selectedView != null)
                _selectedView.SetSelected(true);
        }

        private void UpdatePagination()
        {
            _pageText.text = $"{_currentPage}/{_totalPages}";

            _previousPageButton.interactable = _currentPage > 1;
            _nextPageButton.interactable = _currentPage < _totalPages;
        }
        private void PreviousPageButtonClickHandler()
        {
            if (_currentPage <= 1)
                return;

            LoadPage(_currentPage - 1).Forget();
        }
        private void NextPageButtonClickHandler()
        {
            if (_currentPage >= _totalPages)
                return;

            LoadPage(_currentPage + 1).Forget();
        }

        private void SearchEndEditHandler(string value)
        {
            _search = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            LoadPage(1).Forget();
        }
        private void TypeDropdownChangedHandler(int index)
        {
            _furnitureTypeId = GetNullableId(_typeIds, index);
            LoadPage(1).Forget();
        }
        private void ColorDropdownChangedHandler(int index)
        {
            _colorTypeId = GetNullableId(_colorIds, index);
            LoadPage(1).Forget();
        }

        private void DeselectFurnitureHandler(Furniture furniture, GameObject _) => SelectView(null);

        private void FillTypeDropdown()
        {
            _typeIds.Clear();
            _typeDropdown.ClearOptions();

            List<string> options = new() { "All" };
            _typeIds.Add(0);

            foreach (FurnitureType type in _types)
            {
                options.Add(type.Name);
                _typeIds.Add(type.Id);
            }

            _typeDropdown.AddOptions(options);
            _typeDropdown.SetValueWithoutNotify(0);
        }
        private void FillColorDropdown()
        {
            if (_colorDropdown == null)
                return;

            _colorIds.Clear();
            _colorDropdown.ClearOptions();

            List<string> options = new() { "All" };
            _colorIds.Add(0);

            foreach (ColorType color in _colors)
            {
                options.Add(color.Name);
                _colorIds.Add(color.Id);
            }

            _colorDropdown.AddOptions(options);
            _colorDropdown.SetValueWithoutNotify(0);
        }

        private int? GetNullableId(List<int> ids, int index)
        {
            if (index < 0 || index >= ids.Count)
                return null;

            int id = ids[index];

            return id <= 0 ? null : id;
        }

        private string GetFurnitureTypeName(int id)
        {
            FurnitureType type = _types.FirstOrDefault(item => item.Id == id);
            return type != null ? type.Name : "Unknown";
        }
        private string GetColorTypeName(int id)
        {
            ColorType color = _colors.FirstOrDefault(item => item.Id == id);
            return color != null ? color.Name : "Unknown";
        }
    }
}