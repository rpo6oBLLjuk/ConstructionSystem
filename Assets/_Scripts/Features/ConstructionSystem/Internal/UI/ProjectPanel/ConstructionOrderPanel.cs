using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GLTFast.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionOrderPanel : MonoBehaviour
    {
        [Header("References")]
        [Inject] private OrderModule _orderModule;
        [Inject] private ActiveProjectService _activeProjectService;
        [Inject] private FurnitureDataSaver _furnitureDataSaver;
        [Inject] private NotificationService _notificationService;

        [Header("Items")]
        [SerializeField] private Transform _itemsRoot;
        [SerializeField] private GameObject _itemPrefab;

        [Header("Info")]
        [SerializeField] private TMP_Text _totalText;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _closeButton;

        private readonly List<GameObject> _createdItems = new();
        private readonly List<(int furnitureId, int count, double unitPrice)> _orderItems = new();

        private double _totalPrice;


        private void Awake()
        {
            _itemPrefab.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _confirmButton.onClick.AddListener(ConfirmOrder);
            _closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(ConfirmOrder);
            _closeButton.onClick.RemoveListener(Close);
        }

        public void Open(IReadOnlyList<(Furniture furniture, GameObject instance)> placedFurniture)
        {
            Clear();

            List<Furniture> blockedFurniture = new();

            var groupedFurniture = placedFurniture
                .Where(item => item.furniture != null && item.instance != null)
                .GroupBy(item => item.furniture.Id);

            foreach (var group in groupedFurniture)
            {
                Furniture furniture = group.First().furniture;
                int count = group.Count();

                if (!furniture.IsAvailable)
                {
                    blockedFurniture.Add(furniture);
                    continue;
                }

                CreateOrderItem(furniture, count);
            }

            RefreshTotal();
            ShowWarning(blockedFurniture);

            gameObject.SetActive(true);
        }

        private void CreateOrderItem(Furniture furniture, int count)
        {
            GameObject item = Instantiate(_itemPrefab, _itemsRoot);
            item.SetActive(true);

            _createdItems.Add(item);

            RawImage preview = item.GetComponentInChildren<RawImage>();
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();

            double price = furniture.Price;
            double total = price * count;

            texts[0].text = furniture.Name;
            texts[1].text = $"x{count}";
            texts[2].text = $"{price:N2} ₽";
            texts[3].text = $"{total:N2} ₽";

            _orderItems.Add((furniture.Id, count, price));
            _totalPrice += total;

            LoadPreview(preview, furniture.Id).Forget();
        }

        private async UniTaskVoid LoadPreview(RawImage preview, int furnitureId)
        {
            await _furnitureDataSaver.LoadPreviewSprite(
                furnitureId,
                onComplete: sprite => preview.texture = sprite,
                onError: error => this.FastLog(error)
            );
        }

        private void RefreshTotal()
        {
            _totalText.text = $"Total: {_totalPrice:N2} ₽";
        }

        private void ShowWarning(List<Furniture> blockedFurniture)
        {
            if (blockedFurniture.Count == 0)
            {
                return;
            }

            _notificationService.ShowPopup(
                $"Some selected items are unavailable and were excluded from the order.{string.Join(", ", blockedFurniture.Select(item => item.Name))}",
                "Order warning",
                NotificationType.Warning
            );
        }

        private async void ConfirmOrder()
        {
            if (_orderItems.Count == 0)
            {
                _notificationService.ShowPopup("The order list is empty.", "Order warning", NotificationType.Warning);
                return;
            }

            UserProject project = _activeProjectService.UserProject;

            if (project == null)
            {
                _notificationService.ShowPopup("Active project is missing.", "Order error", NotificationType.Error);
                return;
            }

            await _orderModule.CreateOrder(
                project.UserId,
                project.Id,
                _orderItems,
                "Order from construction scene",
                OnComplete: order =>
                {
                    _notificationService.ShowPopup(
                        $"The order #{order.Id} is confirmed.",
                        "Order success",
                        NotificationType.Success
                    );

                    Close();
                },
                OnError: error =>
                {
                    _notificationService.ShowPopup(
                        error,
                        "Order error",
                        NotificationType.Error
                    );
                }
            );
        }

        private void Close()
        {
            Clear();
            gameObject.SetActive(false);
        }

        private void Clear()
        {
            foreach (GameObject item in _createdItems)
                Destroy(item);

            _createdItems.Clear();
            _orderItems.Clear();

            _totalPrice = 0;

            if (_totalText != null)
                _totalText.text = $"Total: {0:N2}";
        }
    }
}