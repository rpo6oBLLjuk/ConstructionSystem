using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionMaterialOptionView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private RawImage _previewImage;
        [SerializeField] private TMP_Text _nameText;

        private Action _selectHandler;


        private void OnEnable() => _button.onClick.AddListener(ClickHandler);
        private void OnDisable() => _button.onClick.RemoveListener(ClickHandler);

        public void Initialize(RoomMaterialData data, Action selectHandler)
        {
            _selectHandler = selectHandler;

            _nameText.text = data.MaterialName;
            _previewImage.texture = data.BaseTexture;
        }

        private void ClickHandler() => _selectHandler?.Invoke();
    }
}
