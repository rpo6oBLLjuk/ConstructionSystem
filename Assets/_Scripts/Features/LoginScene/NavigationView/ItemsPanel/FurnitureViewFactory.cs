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
    [SerializeField] private Texture _defaultTexture;


    private void Awake()
    {
        _furnitureViewPrefab.SetActive(false);
        _defaultTexture = _furnitureViewPrefab.transform.Find("ItemPreviewContainer/ItemPreview").GetComponent<RawImage>().texture;
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
        FillText(layout, "ItemPrice", $"{furniture.Price.ToString("N2", System.Globalization.CultureInfo.CurrentCulture)} ₽");

        FillPreview(layout, "ItemPreviewContainer/ItemPreview", furniture.Id, furniture);

        if (layout.Find("Background").TryGetComponent(out UIEffect uiEffect))
            furniture.UIEffect = uiEffect;

        if (layout.Find("ItemToggle").TryGetComponent(out Toggle toggle))
            toggle.SetIsOnWithoutNotify(furniture.IsAvailable);
        else
            this.FastLog("Toggle Error");
    }

    private void InitializeViewData(GameObject viewObject, FurnitureViewData furniture, Action<FurnitureViewData> onFurnitureSelected)
    {
        furniture.ViewObject = viewObject;
        Refresh(viewObject, furniture);

        if (viewObject.TryGetComponent<Button>(out var selectButton))
            selectButton.onClick.AddListener(() => onFurnitureSelected?.Invoke(furniture));
    }

    private void FillText(Transform root, string objectName, string value) => root.Find(objectName).GetComponent<TMP_Text>().text = value;
    private void FillPreview(Transform root, string objectName, int furnitureId, FurnitureViewData furniture)
    {
        RawImage image = root.Find(objectName).GetComponent<RawImage>();
        image.texture = _defaultTexture;

        //if (!furniture.HasPreview)
        //return;

        _furnitureDataSaver.LoadPreviewSprite(furnitureId, onComplete: texture =>
        {
            furniture.HasPreview = true;

            image.texture = texture;
            furniture.Preview = texture;
        }, onError: error => this.InactiveLog($"Furniture [{furnitureId}] {error}")).Forget();
    }
}