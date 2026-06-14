using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class OrderPanelPresenter : BaseLayoutPresenter
{
    [Inject] private OrderModule _orderModule;
    [Inject] private UserModule _userModule;
    [Inject] private NotificationService _notificationService;

    [SerializeField] private OrderPanelView _view;
    [SerializeField] private int _pageSize = 10;

    private int _currentPage = 1;
    private int _totalPages = 1;


    protected override void OnEnable()
    {
        base.OnEnable();

        _view.OnStatusChangeRequested += HandleStatusChangeRequested;
        _view.OnDeleteRequested += HandleDeleteRequested;

        _view.OnNextPageRequested += HandleNextPageRequested;
        _view.OnPreviousPageRequested += HandlePreviousPageRequested;

        _orderModule.OrderUpdated += HandleStatusUpdated;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        _view.OnStatusChangeRequested -= HandleStatusChangeRequested;
        _view.OnDeleteRequested -= HandleDeleteRequested;

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

        if (!orderViewData.CanChangeStatus)
        {
            _notificationService.ShowPopup(
                "You do not have permission to change the order status.",
                "Status change",
                NotificationType.Warning
            );
            return;
        }

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

    private void HandleDeleteRequested(OrderViewData orderViewData)
    {
        if (orderViewData == null)
            return;

        if (!orderViewData.CanDelete)
        {
            _notificationService.ShowPopup(
                "You do not have permission to delete this order.",
                "Delete order",
                NotificationType.Warning
            );
            return;
        }

        DeleteOrder(orderViewData).Forget();
    }

    private void HandleStatusUpdated(Order order)
    {
        _notificationService.ShowPopup(
            $"Order '{order.Id}' status has been updated to <b>{Enum.GetName(typeof(OrderStatus), order.Status)}</b>",
            "Status changed",
            NotificationType.Success
        );
    }

    private async UniTask LoadPage(int page)
    {
        bool loadOnlyCurrentUserOrders = _userModule.CurrentUser.RoleId < 2;
        int? targetUserId = loadOnlyCurrentUserOrders ? _userModule.CurrentUser.Id : null;

        int totalCount = targetUserId.HasValue
            ? await _orderModule.GetOrdersCountByUserId(targetUserId.Value)
            : await _orderModule.GetOrdersCount();

        _totalPages = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)_pageSize));
        _currentPage = Mathf.Clamp(page, 1, _totalPages);

        int offset = (_currentPage - 1) * _pageSize;

        List<Order> orders = targetUserId.HasValue
            ? await _orderModule.GetOrdersPageByUserId(targetUserId.Value, offset, _pageSize)
            : await _orderModule.GetOrdersPage(offset, _pageSize);

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

        Dictionary<int, User> usersById = users
            .GroupBy(user => user.Id)
            .ToDictionary(group => group.Key, group => group.First());

        Dictionary<int, List<OrderItem>> itemsByOrderId = orderItems
            .GroupBy(item => item.OrderId)
            .ToDictionary(group => group.Key, group => group.ToList());

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
                "Status change error",
                NotificationType.Error
            );
        });

        orderViewData.Status = newStatus;
        orderViewData.SourceOrder.Status = newStatus;

        await LoadPage(_currentPage);
    }

    private async UniTask DeleteOrder(OrderViewData orderViewData)
    {
        await _orderModule.DeleteOrder(
            orderViewData.SourceOrder,
            OnComplete: order =>
            {
                _notificationService.ShowPopup(
                    $"Order '{order.Id}' has been deleted.",
                    "Delete order",
                    NotificationType.Success
                );
            },
            OnError: error =>
            {
                _notificationService.ShowPopup(
                    error,
                    "Delete order error",
                    NotificationType.Error
                );
            }
        );

        await LoadPage(_currentPage);
    }

    private OrderViewData ConvertToViewData(Order order, User customer, List<OrderItem> orderItems)
    {
        int currentUserId = _userModule.CurrentUser.Id;
        int currentUserRoleId = _userModule.CurrentUser.RoleId;

        bool canChangeStatus = currentUserRoleId >= 2;
        bool canDelete = currentUserRoleId >= 3 || order.UserId == currentUserId;

        return new OrderViewData
        {
            Id = order.Id,
            CustomerFullName = ConvertUsername(customer),
            Status = order.Status,
            CreatedAt = $"{order.CreatedAt:dd.MM.yyyy}\n{order.CreatedAt:HH:mm:ss}",
            Items = ConvertItemsToDisplayString(orderItems),

            CanChangeStatus = canChangeStatus,
            CanDelete = canDelete,

            SourceOrder = order
        };
    }

    private List<string> ConvertItemsToDisplayString(List<OrderItem> items)
    {
        if (items == null || items.Count == 0)
            return new List<string> { "Without items" };

        return items.ConvertAll(item =>
            $"'{item.FurnitureId}' – {item.Count} x {item.UnitPrice} = {item.Count * item.UnitPrice}"
        );
    }

    private string ConvertUsername(User user)
    {
        if (user == null)
            return "Unknown user";

        return $"{user.LastName} {user.FirstName}".Trim();
    }
}