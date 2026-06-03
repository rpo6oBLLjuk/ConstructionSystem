using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class OrderPanelPresenter : BaseLayoutPresenter
{
    private const int PageSize = 20;

    [Inject] private OrderModule _orderModule;
    [Inject] private UserModule _userModule;
    [Inject] private NotificationService _notificationService;

    [SerializeField] private OrderPanelView _view;

    private int _currentPage = 1;
    private int _totalPages = 1;


    protected override void OnEnable()
    {
        base.OnEnable();

        _view.OnStatusChangeRequested += HandleStatusChangeRequested;
        _view.OnNextPageRequested += HandleNextPageRequested;
        _view.OnPreviousPageRequested += HandlePreviousPageRequested;

        _orderModule.OrderUpdated += HandleStatusUpdated;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        _view.OnStatusChangeRequested -= HandleStatusChangeRequested;
        _view.OnNextPageRequested -= HandleNextPageRequested;
        _view.OnPreviousPageRequested -= HandlePreviousPageRequested;

        _orderModule.OrderUpdated -= HandleStatusUpdated;
    }

    public override void Show()
    {
        LoadPage(1).Forget();
        base.Show();
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

    private void HandleStatusChangeRequested(OrderViewData orderViewData, OrderStatus newStatus)
    {
        if (orderViewData == null)
            return;

        if (newStatus == orderViewData.Status)
        {
            _notificationService.ShowPopup(
                "The order status is identical to the current one",
                "Status change",
                NotificationType.Warning
            );
            return;
        }

        ChangeOrderStatus(orderViewData, newStatus).Forget();
    }
    private void HandleStatusUpdated(Order order) => _notificationService.ShowPopup($"Order '{order.Id}' status has been updated to <b>{Enum.GetName(typeof(OrderStatus), order.Status)}</b>", "Status changed", NotificationType.Info);

    private async UniTask LoadPage(int page)
    {
        int totalCount = await _orderModule.GetOrdersCount();

        _totalPages = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)PageSize));
        _currentPage = Mathf.Clamp(page, 1, _totalPages);

        int offset = (_currentPage - 1) * PageSize;

        List<Order> orders = await _orderModule.GetOrdersPage(offset, PageSize);

        if (orders == null || orders.Count == 0)
        {
            _view.SetOrders(new List<OrderViewData>());
            _view.SetPagination(_currentPage, _totalPages);
            return;
        }

        List<int> userIds = orders.Select(order => order.UserId).Distinct().ToList();
        List<int> orderIds = orders.Select(order => order.Id).Distinct().ToList();

        List<User> users = await _userModule.GetUsersByIds(userIds);
        List<OrderItem> orderItems = await _orderModule.GetOrderItemsByOrderIds(orderIds);

        Dictionary<int, User> usersById = users.GroupBy(user => user.Id).ToDictionary(group => group.Key, group => group.First());
        Dictionary<int, List<OrderItem>> itemsByOrderId = orderItems.GroupBy(item => item.OrderId).ToDictionary(group => group.Key, group => group.ToList());

        List<OrderViewData> viewData = new();

        foreach (Order order in orders)
        {
            usersById.TryGetValue(order.UserId, out User customer);
            itemsByOrderId.TryGetValue(order.Id, out List<OrderItem> items);

            viewData.Add(ConvertToViewData(order, customer, items));
        }

        _view.SetOrders(viewData);
        _view.SetPagination(_currentPage, _totalPages);
    }
    private async UniTask ChangeOrderStatus(OrderViewData orderViewData, OrderStatus newStatus)
    {
        OrderStatus previousStatus = orderViewData.Status;

        await _orderModule.UpdateOrderStatus(orderViewData.SourceOrder, newStatus, OnError: error =>
        {
            orderViewData.Status = previousStatus;
            orderViewData.SourceOrder.Status = previousStatus;

            _notificationService.ShowPopup(
                error,
                "Ошибка изменения статуса заказа",
                NotificationType.Error
            );
        });

        orderViewData.Status = newStatus;
        orderViewData.SourceOrder.Status = newStatus;

        await LoadPage(_currentPage);
    }

    private OrderViewData ConvertToViewData(Order order, User customer, List<OrderItem> orderItems)
    {
        return new OrderViewData
        {
            Id = order.Id,
            CustomerFullName = ConvertUsername(customer),
            Status = order.Status,
            CreatedAt = $"{order.CreatedAt:dd.MM.yyyy}\n{order.CreatedAt:HH:mm:ss}",
            Items = ConvertItemsToDisplayString(orderItems),
            SourceOrder = order
        };
    }
    private List<string> ConvertItemsToDisplayString(List<OrderItem> items)
    {
        if (items == null || items.Count == 0)
            return new List<string> { "Нет товаров" };

        return items.ConvertAll(item =>
            $"'{item.FurnitureId}' – {item.Count} x {item.UnitPrice} = {item.Count * item.UnitPrice}"
        );
    }
    private string ConvertUsername(User user)
    {
        if (user == null)
            return "Неизвестный пользователь";

        return $"{user.LastName} {user.FirstName}".Trim();
    }
}