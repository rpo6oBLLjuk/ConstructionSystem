using System;
using Coffee.UIEffects;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class FurnitureViewFactory : MonoBehaviour
{
    [Inject] FurnitureDataSaver _furnitureDataSaver;

    [SerializeField] private GameObject _furnitureViewPrefab;


    private void Awake()
    {
        if (_furnitureViewPrefab != null)
            _furnitureViewPrefab.SetActive(false);
    }

    public GameObject Create(FurnitureViewData furniture, Transform parent, Action<FurnitureViewData> onFurnitureSelected)
    {
        GameObject viewObject = Instantiate(_furnitureViewPrefab, parent);
        viewObject.SetActive(true);

        InitializeViewData(viewObject, furniture, onFurnitureSelected);

        return viewObject;
    }
    public void Refresh(GameObject viewObject, FurnitureViewData furniture)
    {
        Transform layout = viewObject.transform;

        FillText(layout, "ItemName", furniture.Name);
        FillText(layout, "ItemType", furniture.FurnitureTypeName);
        FillText(layout, "ItemColor", furniture.ColorTypeName);

        FillPreview(layout, "ItemPreviewContainer/ItemPreview", furniture.Id, furniture);
    }

    private void InitializeViewData(GameObject viewObject, FurnitureViewData furniture, Action<FurnitureViewData> onFurnitureSelected)
    {
        furniture.ViewObject = viewObject;
        Refresh(viewObject, furniture);

        if (viewObject.TryGetComponent<Button>(out var selectButton))
            selectButton.onClick.AddListener(() => onFurnitureSelected?.Invoke(furniture));
        else
            DebugWrapper.LogWarning(this, "Button component not found on furniture view prefab");
    }

    private void FillText(Transform root, string objectName, string value) => root.Find(objectName).GetComponent<TMP_Text>().text = value;
    private void FillPreview(Transform root, string objectName, int furnitureId, FurnitureViewData furniture)
    {
        Image image = root.Find(objectName).GetComponent<Image>();
        _furnitureDataSaver.LoadPreviewSprite(furnitureId, onComplete: sprite =>
        {
            image.sprite = sprite;
            furniture.Preview = image.sprite;
        }, onError: error => this.FastLog(error)).Forget();
    }
}