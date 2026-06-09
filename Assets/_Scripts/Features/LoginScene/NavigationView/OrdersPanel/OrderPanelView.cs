using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderPanelView : MonoBehaviour
{
    public event Action<OrderViewData, OrderStatus> OnStatusChangeRequested;
    public event Action OnNextPageRequested;
    public event Action OnPreviousPageRequested;

    [Header("List")]
    [SerializeField] private OrderViewFactory _factory;
    [SerializeField] ScrollRect _scrollRect;

    [Header("Pagination")]
    [SerializeField] private Button _previousPageButton;
    [SerializeField] private Button _nextPageButton;
    [SerializeField] private TMP_Text _pageText;

    [Space]
    [SerializeField] private GameObject _noOrdersPanel;

    private readonly List<GameObject> _createdViews = new();


    private void OnEnable()
    {
        _previousPageButton.onClick.AddListener(PreviousPageButtonClickHandler);
        _nextPageButton.onClick.AddListener(NextPageButtonClickHandler);
    }
    private void OnDisable()
    {
        _previousPageButton.onClick.RemoveListener(PreviousPageButtonClickHandler);
        _nextPageButton.onClick.RemoveListener(NextPageButtonClickHandler);
    }

    public void SetOrders(List<OrderViewData> orders)
    {
        Clear();

        _noOrdersPanel.SetActive(orders.Count == 0);

        foreach (OrderViewData order in orders)
        {
            GameObject view = _factory.Create(
                order,
                StatusChangeRequestHandler
            );

            _createdViews.Add(view);
        }
    }
    public void SetPagination(int currentPage, int totalPages)
    {
        _pageText.text = $"{currentPage}/{totalPages}";

        _previousPageButton.interactable = currentPage > 1;
        _nextPageButton.interactable = currentPage < totalPages;
    }

    private void Clear()
    {
        _scrollRect.verticalNormalizedPosition = 1f;

        foreach (GameObject view in _createdViews)
        {
            if (view != null)
                Destroy(view);
        }

        _createdViews.Clear();
    }

    private void StatusChangeRequestHandler(OrderViewData order, OrderStatus newStatus) => OnStatusChangeRequested?.Invoke(order, newStatus);

    private void PreviousPageButtonClickHandler() => OnPreviousPageRequested?.Invoke();
    private void NextPageButtonClickHandler() => OnNextPageRequested?.Invoke();
}