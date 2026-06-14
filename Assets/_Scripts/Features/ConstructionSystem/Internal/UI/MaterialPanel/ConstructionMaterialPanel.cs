using System;
using UnityEngine;
using UnityEngine.UI;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionMaterialPanel : BaseSlidePanel
    {
        [Header("Material Panels")]
        [SerializeField] private RoomMaterialTabData _floor;
        [SerializeField] private RoomMaterialTabData _ceil;
        [SerializeField] private RoomMaterialTabData _walls;

        [SerializeField] private ConstructionMaterialOptionFactory _factory;


        protected override void OnEnable()
        {
            base.OnEnable();

            _floor.TabButton.onClick.AddListener(() => OpenTabLayout(_floor));
            _ceil.TabButton.onClick.AddListener(() => OpenTabLayout(_ceil));
            _walls.TabButton.onClick.AddListener(() => OpenTabLayout(_walls));
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            _floor.TabButton.onClick.RemoveAllListeners();
            _ceil.TabButton.onClick.RemoveAllListeners();
            _walls.TabButton.onClick.RemoveAllListeners();
        }

        protected override void OnPanelInitialized() => BuildList(_walls);

        public void OpenTabLayout(RoomMaterialTabData tabData) => BuildList(tabData);

        private void BuildList(RoomMaterialTabData tabData)
        {
            _factory.Clear();
            tabData.Config.Materials.ForEach(materialData => _factory.Create(materialData, () => ApplyMaterial(materialData, tabData)));
        }

        private void ApplyMaterial(RoomMaterialData data, RoomMaterialTabData tabData)
        {
            ApplyBaseTexture(data, tabData);
            ApplyNormalTexture(data, tabData);
        }

        private void ApplyBaseTexture(RoomMaterialData data, RoomMaterialTabData tabData)
        {
            if (!tabData.TargetMaterial.HasProperty("_BaseMap"))
                return;

            tabData.TargetMaterial.SetTexture("_BaseMap", data.BaseTexture);
        }
        private void ApplyNormalTexture(RoomMaterialData data, RoomMaterialTabData tabData)
        {
            if (!tabData.TargetMaterial.HasProperty("_BumpMap"))
                return;

            tabData.TargetMaterial.SetTexture("_BumpMap", data.NormalTexture);

            if (tabData.TargetMaterial.HasProperty("_BumpScale"))
                tabData.TargetMaterial.SetFloat("_BumpScale", data.NormalStrength);

            if (data.NormalTexture != null)
                tabData.TargetMaterial.EnableKeyword("_NORMALMAP");
            else
                tabData.TargetMaterial.DisableKeyword("_NORMALMAP");
        }

        [Serializable]
        public class RoomMaterialTabData
        {
            public Button TabButton;
            public RoomMaterialListConfig Config;
            public Material TargetMaterial;
        }
    }
}
