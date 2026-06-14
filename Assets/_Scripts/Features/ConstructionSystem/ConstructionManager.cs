using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionManager : MonoBehaviour
    {
        [Inject] private ActiveProjectService _activeProjectService;

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
            LoadProject(_activeProjectService.ProjectData);
        }


        private void LoadProject(ProjectData projectData) => ProjectLoaded?.Invoke(projectData);

        public void SelectFurniturePrototype(Furniture furniture)
        {
            if (furniture == null)
            {
                this.LogError("Selected furniture is null.");
                return;
            }

            _furnitureModelPool.LoadPrototype(furniture, onComplete: instance =>
            {
                _selectedFurniture = furniture;
                FurniturePrototypeSelected?.Invoke(furniture, instance);
            }).Forget();
        }
        public void DecelectFurniturePrototype(Furniture furniture, GameObject instance)
        {
            _furnitureModelPool.DisableLoadedPrototype(instance);
            FurniturePrototypeDeselected?.Invoke(furniture, instance);
        }

        public void CreateFurnitureInstanceByPrototype(GameObject prototype)
        {
            GameObject instance = _furnitureModelPool.GetInstance(prototype);
            FurnitureInstanceCreated?.Invoke(_selectedFurniture, instance);
        }

        public void RemoveFurnitureInstance(GameObject furniture)
        {
            FurnitureInstanceRemoved?.Invoke(_selectedFurniture, furniture);
            _furnitureModelPool.RemoveInstance(furniture);
        }
    }
}