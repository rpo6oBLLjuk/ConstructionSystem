using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using SQLite;
using UnityEngine;

public class OrderModule
{
    private readonly OrderRepository _orderRepository;          //Репозиторий для работы с DB, таблица Заказов
    private readonly OrderItemRepository _orderItemRepository;  //Репозиторий для работы с DB, таблица Товаров в заказах

    public event Action<Order> OrderCreated;
    public event Action<Order> OrderUpdated;

    private string _orderNotFound = "Order with ID <b>{0}<b> not found in database";


    public OrderModule(OrderRepository orderRepository, OrderItemRepository orderItemRepository)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
    }

#if UNITY_EDITOR
    public async UniTask ClearOrders() => await _orderRepository.DeleteMany(await _orderRepository.GetAll());
#endif

    public async UniTask<Order> GetOrderById(int orderId) => await _orderRepository.GetById(orderId);
    public async UniTask<List<Order>> GetOrdersByIds(List<int> ids) => await _orderRepository.GetByIds(ids);
    public async UniTask<List<Order>> GetOrdersByOrderId(int userId) => await _orderRepository.GetOrdersByUserId(userId);

    public async UniTask<List<OrderItem>> GetOrderItemsByOrderId(int orderId) => await _orderItemRepository.GetItemsByOrderId(orderId);
    public async UniTask<List<OrderItem>> GetOrderItemsByOrderIds(List<int> orderIds) => await _orderItemRepository.GetOrderItemsByOrderIds(orderIds);

    public async UniTask<int> GetOrdersCount() => await _orderRepository.Count();
    public async UniTask<int> GetOrdersCountByUserId(int userId) => await _orderRepository.CountByUserId(userId);

    public async UniTask<List<Order>> GetOrdersPage(int offset, int count) => await _orderRepository.Paging(offset, count);
    public async UniTask<List<Order>> GetOrdersPageByUserId(int userId, int offset, int count) => await _orderRepository.PagingByUserId(userId, offset, count);

    public async UniTask CreateOrder(int userId, int userProjectId, List<(int furnitureId, int count, double unitPrice)> items, string comment, Action<Order> OnComplete = null, Action<string> OnError = null)
    {
        if (items == null || items.Count == 0)
        {
            OnError?.Invoke("Cannot create an empty order.");
            return;
        }

        double totalAmount = GetOrderTotalAmount(items);

        Order order = new()
        {
            UserId = userId,
            UserProjectId = userProjectId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Status = OrderStatus.New,
            TotalAmount = totalAmount,
            Comment = comment
        };

        try
        {
            await _orderRepository.Insert(order);

            List<OrderItem> orderItems = new();

            items.ForEach(item => orderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                FurnitureId = item.furnitureId,
                Count = item.count,
                UnitPrice = item.unitPrice
            }));

            await _orderItemRepository.InsertMany(orderItems);

            OrderCreated?.Invoke(order);
            OnComplete?.Invoke(order);
        }
        catch (SQLiteException ex)
        {
            OnError?.Invoke(ex.ToString());
        }
    }

    public async UniTask UpdateOrderStatus(int orderId, OrderStatus newStatus, Action<string> OnError = null)
    {
        Order order = await _orderRepository.GetById(orderId);
        if (order == null)
        {
            OnError?.Invoke(string.Format(_orderNotFound, orderId));
            return;
        }

        await UpdateOrderStatus(order, newStatus, OnError);
    }
    public async UniTask UpdateOrderStatus(Order order, OrderStatus newStatus, Action<string> OnError = null)
    {
        if (order == null)
        {
            OnError?.Invoke("Order is null");
            return;
        }

        OrderStatus previousStatus = order.Status;
        DateTime previousUpdatedAt = order.UpdatedAt;

        order.Status = newStatus;
        order.UpdatedAt = DateTime.Now;

        try
        {
            await _orderRepository.Update(order);
            OrderUpdated?.Invoke(order);
        }
        catch (SQLiteException ex)
        {
            order.Status = previousStatus;
            order.UpdatedAt = previousUpdatedAt;

            Debug.LogError(ex.ToString());
            OnError?.Invoke(ex.Message);
        }
    }

    public async UniTask UpdateOrderItems(int orderId, List<(int furnitureId, int count, double unitPrice)> newItems, Action<string> OnError = null)
    {
        Order order = await _orderRepository.GetById(orderId);
        if (order == null)
        {
            OnError?.Invoke(string.Format(_orderNotFound, orderId));
            return;
        }

        await UpdateOrderItems(order, newItems, OnError);
    }
    public async UniTask UpdateOrderItems(Order order, List<(int furnitureId, int count, double unitPrice)> newItems, Action<string> OnError = null)
    {
        List<OrderItem> currentItems = await _orderItemRepository.GetItemsByOrder(order);

        // Списки для пакетных операций в БД
        List<OrderItem> itemsToInsert = new();
        List<OrderItem> itemsToUpdate = new();
        List<OrderItem> itemsToDelete = new(currentItems); //Удалится всё, что не найдено по ID

        var currentItemsMap = currentItems.ToDictionary(x => x.FurnitureId, x => x);

        // Проход по списку новых предметов
        foreach (var newItem in newItems)
        {
            if (currentItemsMap.TryGetValue(newItem.furnitureId, out var currentItem)) // Если предмет есть в обоих списках
            {
                if (currentItem.Count != newItem.count || Math.Abs(currentItem.UnitPrice - newItem.unitPrice) > 0.01) // Надо проверить на изменение его данных
                {
                    currentItem.Count = newItem.count;
                    currentItem.UnitPrice = newItem.unitPrice;
                    itemsToUpdate.Add(currentItem);
                }
                itemsToDelete.Remove(currentItem); // И удалить из списка на удаление, т.к. ID найден
            }
            else // Если предмета не было
            {
                // Добавить предмет
                itemsToInsert.Add(new OrderItem
                {
                    OrderId = order.Id,
                    FurnitureId = newItem.furnitureId,
                    Count = newItem.count,
                    UnitPrice = newItem.unitPrice
                });
            }
        }

        // Кэш данных для отката
        double previousTotalAmount = order.TotalAmount;
        DateTime previousUpdatedAt = order.UpdatedAt;

        // Обновление Db
        try
        {
            if (itemsToDelete.Count > 0)
                await _orderItemRepository.DeleteMany(itemsToDelete);

            if (itemsToUpdate.Count > 0)
                await _orderItemRepository.UpdateMany(itemsToUpdate);

            if (itemsToInsert.Count > 0)
                await _orderItemRepository.InsertMany(itemsToInsert);

            double newTotalAmount = GetOrderTotalAmount(newItems);

            order.TotalAmount = newTotalAmount;
            order.UpdatedAt = DateTime.Now;

            await _orderRepository.Update(order);

            OrderUpdated?.Invoke(order);
        }
        catch (SQLiteException ex)
        {
            // Откат заказа
            order.TotalAmount = previousTotalAmount;
            order.UpdatedAt = previousUpdatedAt;

            Debug.LogError(ex.ToString());
            OnError?.Invoke(ex.Message);
        }
    }

    public async UniTask DeleteOrder(Order order, Action<Order> OnComplete = null, Action<string> OnError = null)
    {
        if (order == null)
        {
            OnError?.Invoke("Order is null.");
            return;
        }

        try
        {
            List<OrderItem> items = await _orderItemRepository.GetItemsByOrderId(order.Id);

            if (items != null && items.Count > 0)
                await _orderItemRepository.DeleteMany(items);

            await _orderRepository.Delete(order);

            OnComplete?.Invoke(order);
        }
        catch (SQLiteException ex)
        {
            Debug.LogError(ex.ToString());
            OnError?.Invoke(ex.Message);
        }
    }

    private double GetOrderTotalAmount(List<(int furnitureId, int count, double unitPrice)> items) => items.Sum(item => item.unitPrice * item.count);
}
