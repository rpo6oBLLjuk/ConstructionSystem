using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionFurnitureModelPool : MonoBehaviour
    {
        [Inject] private NotificationService _notificationService;
        [Inject] private FurnitureDataSaver _furnitureDataSaver;

        [SerializeField] private Transform _poolRoot;
        [SerializeField] private Transform _loadedRoot;

        private readonly Dictionary<int, GameObject> _loadedModels = new();
        private List<GameObject> _createdObjects = new();


        public async UniTask LoadPrototype(Furniture furniture, Action<GameObject> onComplete = null)
        {
            if (_loadedModels.TryGetValue(furniture.Id, out GameObject loadedModel))
            {
                onComplete?.Invoke(loadedModel);
                return;
            }

            loadedModel = await LoadInternal(furniture);
            if (loadedModel == null)
            {
                this.InactiveLog($"Furniture <b>{furniture.Name}</b> model can't loaded.");
                return;
            }

            onComplete?.Invoke(loadedModel);
        }
        public void DisableLoadedPrototype(GameObject instance) => instance.SetActive(false);


        public GameObject GetInstance(GameObject prototype) => InstantiateCopy(prototype);
        public void RemoveInstance(GameObject instance)
        {
            _createdObjects.Remove(instance);
            Destroy(instance);
        }

        public void ClearAll()
        {
            _loadedModels.Values.ToList().ForEach(loadedModel => Destroy(loadedModel));
            _loadedModels.Clear();

            _createdObjects.ForEach(created => Destroy(created));
            _createdObjects.Clear();
        }

        private GameObject InstantiateCopy(GameObject gameObject)
        {
            GameObject copy = Instantiate(gameObject, _poolRoot);
            copy.SetActive(true);
            copy.name = $"{gameObject.name} (Instance)";
            _createdObjects.Add(copy);

            return copy;
        }
        private async UniTask<GameObject> LoadInternal(Furniture furniture)
        {
            GameObject loadedModel = null;

            await _furnitureDataSaver.LoadModelGameObject(furniture.Id, _loadedRoot, onComplete: model => loadedModel = model, onError: error => _notificationService.ShowPopup(error, "Model upload error", NotificationType.Error));

            if (loadedModel == null)
                return null;

            loadedModel.name = $"Furniture_{furniture.Id}_{furniture.Name}";
            loadedModel.SetActive(false);

            _loadedModels[furniture.Id] = loadedModel;

            this.InactiveLog($"Furniture model loaded to pool: <b>{furniture.Name}</b> [{furniture.Id}]");

            return loadedModel;
        }
    }
}