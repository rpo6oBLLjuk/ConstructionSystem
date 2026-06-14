using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionManager : MonoBehaviour
    {
        [Inject] private ActiveProjectService _activeProjectService;
        [Inject] private FurnitureModule _furnitureModule;

        public event Action<ProjectData> ProjectLoaded;

        public event Action<Furniture, GameObject> FurniturePrototypeSelected;
        public event Action<Furniture, GameObject> FurniturePrototypeDeselected;

        public event Action<Furniture, GameObject> FurnitureInstanceCreated;
        public event Action<Furniture, GameObject> FurnitureInstanceRemoved;

        public Furniture SelectedFurniture => _selectedFurniture;

        [Header("References")]
        [SerializeField] ConstructionFurnitureModelPool _furnitureModelPool;
        private Furniture _selectedFurniture;


        private void Start()
        {
#if UNITY_EDITOR
            if (_activeProjectService.ProjectData == null)
            {
                this.FastLog("<b><u> ––– START PLAY FROM LOGIN SCENE TO LOAD PROJECT BLUEPRINT</u></b> –––");
                UnityEditor.EditorApplication.isPlaying = false;
                return;
            }
#endif
            LoadSavedFurniture(_activeProjectService.ProjectData).Forget();
        }
        private async UniTask LoadSavedFurniture(ProjectData projectData)
        {
            if (projectData == null || projectData.Furniture == null)
                return;

            foreach (PlacedFurnitureData placedFurniture in projectData.Furniture)
            {
                Furniture furniture = await _furnitureModule.GetFurnitureById(placedFurniture.itemId);

                if (furniture == null)
                {
                    this.LogError($"Furniture with id {placedFurniture.itemId} not found.");
                    continue;
                }

                GameObject instance = await _furnitureModelPool.CreateInstance(furniture);

                if (instance == null)
                {
                    this.LogError($"Furniture instance with id {placedFurniture.itemId} can't be created.");
                    continue;
                }

                instance.transform.position = placedFurniture.position;
                instance.transform.rotation = Quaternion.Euler(placedFurniture.rotation);

                FurnitureInstanceCreated?.Invoke(furniture, instance);
            }

            ProjectLoaded?.Invoke(projectData);

            Physics.SyncTransforms();
        }


        public void SelectFurniturePrototype(Furniture furniture)
        {
            if (furniture == null)
            {
                this.InactiveLog("Selected furniture is null.");
                return;
            }
            this.InactiveLog($"Prototupe <b>P{furniture.Name}</b> selected");
            _furnitureModelPool.LoadPrototype(furniture, onComplete: instance =>
            {
                _selectedFurniture = furniture;
                FurniturePrototypeSelected?.Invoke(furniture, instance);
            }).Forget();
        }
        public void DecelectFurniturePrototype(Furniture furniture, GameObject instance)
        {
            this.InactiveLog($"Prototupe <b>P{furniture.Name}</b> deselected");
            _furnitureModelPool.DisableLoadedPrototype(instance);
            FurniturePrototypeDeselected?.Invoke(furniture, instance);
        }

        public void CreateFurnitureInstanceByPrototype(GameObject prototype)
        {
            this.InactiveLog($"Furniture <b>P{prototype.name}</b> instantiated");
            GameObject instance = _furnitureModelPool.GetInstance(prototype);
            FurnitureInstanceCreated?.Invoke(_selectedFurniture, instance);
        }

        public void RemoveFurnitureInstance(GameObject furniture)
        {
            this.InactiveLog($"Furniture <b>P{furniture.name}</b> removed");
            FurnitureInstanceRemoved?.Invoke(_selectedFurniture, furniture);
            _furnitureModelPool.RemoveInstance(furniture);
        }
    }
}