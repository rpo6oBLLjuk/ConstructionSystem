using System;
using Coffee.UIEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionFurnitureItemView : MonoBehaviour
    {
        [Inject] NotificationService _notificationService;

        [Header("UI")]
        [SerializeField] private Button _button;
        [SerializeField] private RawImage _preview;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _type;
        [SerializeField] private TMP_Text _color;
        [SerializeField] private TMP_Text _price;

        [Header("Effect")]
        [SerializeField] private UIEffect _uiEffect;
        [SerializeField] private Color _defaultEdgeColor = Color.white;
        [SerializeField] private Color _selectedEdgeColor = Color.cyan;
        [SerializeField] private Color _unavailableEdgeColor = Color.red;

        private bool _isAvaliable = true;


        private void OnDisable() => _button.onClick.RemoveAllListeners();

        public void Initialize(Furniture furniture, string typeName, string colorName, Texture preview, Action<ConstructionFurnitureItemView, Furniture> selectHandler)
        {
            _button.onClick.AddListener(() => selectHandler?.Invoke(this, furniture));

            _preview.texture = preview;

            _name.text = furniture.Name;
            _type.text = typeName;
            _color.text = colorName;
            _price.text = $"{furniture.Price.ToString("N2", System.Globalization.CultureInfo.CurrentCulture)} ₽";

            _isAvaliable = furniture.IsAvailable;

            RefreshState();
        }
        
        public void SetSelected(bool value) => RefreshState(value);
        public void SetPreview(Texture texture) => _preview.texture = texture;

        private void RefreshState(bool selected = false)
        {
            _uiEffect.edgeColor = _defaultEdgeColor;

            if (!_isAvaliable)
                _uiEffect.edgeColor = _unavailableEdgeColor;

            if (selected)
                _uiEffect.edgeColor = _selectedEdgeColor;

        }
    }
}