using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionFurnitureItemFactory : MonoBehaviour
    {
        [Inject] private FurnitureDataSaver _furnitureDataSaver;
        [Inject] private DiContainer _container;

        public readonly Dictionary<Furniture, ConstructionFurnitureItemView> CreatedItems = new();

        [SerializeField] private ConstructionFurnitureItemView _prefab;
        [SerializeField] private Transform _root;
        [SerializeField] private Texture _defaultPreview;


        private void Awake() => _prefab.gameObject.SetActive(false);

        public ConstructionFurnitureItemView Create(Furniture furniture, string typeName, string colorName, Action<ConstructionFurnitureItemView, Furniture> selectHandler)
        {
            ConstructionFurnitureItemView view = _container.InstantiatePrefabForComponent<ConstructionFurnitureItemView>(_prefab, _root);
            view.gameObject.SetActive(true);

            CreatedItems.Add(furniture, view);
            view.Initialize(furniture, typeName, colorName, _defaultPreview, selectHandler);

            if (furniture.HasPreview)
                LoadPreview(view, furniture).Forget();

            return view;
        }

        public void Clear()
        {
            CreatedItems.Values.ToList().ForEach(item => Destroy(item.gameObject));
            CreatedItems.Clear();
        }

        private async UniTaskVoid LoadPreview(ConstructionFurnitureItemView view, Furniture furniture)
        {
            await _furnitureDataSaver.LoadPreviewSprite(furniture.Id,
                onComplete: texture => view.SetPreview(texture),
                onError: error => this.InactiveLog(error)
            );
        }
    }
}