using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderViewFactory : MonoBehaviour
{
    [SerializeField] GameObject _orderViewPrefab;


    private void Start() => _orderViewPrefab.SetActive(false);

    public GameObject Create(OrderViewData order, Action<OrderViewData, OrderStatus> onStatusChangeRequested)
    {
        GameObject viewObject = Instantiate(_orderViewPrefab, _orderViewPrefab.transform.parent);
        viewObject.SetActive(true);

        InitializeViewData(viewObject, order, onStatusChangeRequested);

        return viewObject;
    }

    private void InitializeViewData(GameObject viewGO, OrderViewData order, Action<OrderViewData, OrderStatus> onStatusChangeRequested)
    {
        var layout = viewGO.transform.Find("Layout");

        FillText(layout, "Group1/OrderId", $"#{order.Id}");
        FillText(layout, "Group2/Username Text (TMP)", order.CustomerFullName);
        FillText(layout, "Group3/Date Text (TMP)", order.CreatedAt);

        TMP_Dropdown dropdown = layout.GetChild(2).GetComponentInChildren<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(index => dropdown.captionText.color = dropdown.options[index].color);
        dropdown.onValueChanged?.Invoke(order.Status.GetHashCode());
        dropdown.value = order.Status.GetHashCode();

        layout.transform.Find("Group4/SaveOrderButton").GetComponent<Button>().onClick.AddListener(() => onStatusChangeRequested?.Invoke(order, (OrderStatus)dropdown.value));

        var itemContainer = layout.Find("Group4/Scroll View/Viewport/Content/ItemContainer");
        foreach (var item in order.Items)
        {
            GameObject itemInstance = Instantiate(itemContainer.gameObject, itemContainer.parent);
            FillText(itemInstance.transform, "ItemName Text (TMP)", item);
        }

        Destroy(itemContainer.gameObject);
    }

    private void FillText(Transform transform, string name, string value) => transform.Find(name).GetComponent<TMP_Text>().text = value;
}