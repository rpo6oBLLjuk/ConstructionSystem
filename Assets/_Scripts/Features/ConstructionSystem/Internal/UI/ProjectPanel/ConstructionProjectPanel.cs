using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionProjectPanel : BaseSlidePanel
    {
        [Header("References")]
        [Inject] private ConstructionManager _constructionManager;
        [Inject] private SceneTransitionController _sceneTransitionController;
        [Inject] private ActiveProjectService _activeProjectService;
        [Inject] private ProjectDataSaver _projectDataSaver;
        [Inject] private NotificationService _notificationService;

        public IReadOnlyList<(Furniture furniture, GameObject instance)> PlacedFurniture => _placedFurniture;

        [SerializeField] private ConstructionOrderPanel _orderPanel;

        [Header("Buttons")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _orderButton;
        [SerializeField] private Button _exitButton;

        private readonly List<(Furniture furniture, GameObject instance)> _placedFurniture = new();


        protected override void OnEnable()
        {
            base.OnEnable();
            _constructionManager.FurnitureInstanceCreated += FurnitureInstanceCreatedHandler;
            _constructionManager.FurnitureInstanceRemoved += FurnitureInstanceRemovedHandler;

            _saveButton.onClick.AddListener(SaveProject);
            _orderButton.onClick.AddListener(OpenOrderPanel);
            _exitButton.onClick.AddListener(ExitToLoginScene);
        }

        protected override void OnDisable()
        {
            _constructionManager.FurnitureInstanceCreated -= FurnitureInstanceCreatedHandler;
            _constructionManager.FurnitureInstanceRemoved -= FurnitureInstanceRemovedHandler;

            _saveButton.onClick.RemoveListener(SaveProject);
            _orderButton.onClick.RemoveListener(OpenOrderPanel);
            _exitButton.onClick.RemoveListener(ExitToLoginScene);

            base.OnDisable();
        }

        private void FurnitureInstanceCreatedHandler(Furniture furniture, GameObject instance) => _placedFurniture.Add((furniture, instance));
        private void FurnitureInstanceRemovedHandler(Furniture furniture, GameObject instance) => _placedFurniture.RemoveAll(item => item.instance == instance || item.instance == null);

        private void SaveProject()
        {
            _placedFurniture.RemoveAll(item => item.furniture == null || item.instance == null);

            ProjectData projectData = _activeProjectService.ProjectData;
            projectData.Furniture = new PlacedFurnitureData[_placedFurniture.Count];

            for (int i = 0; i < _placedFurniture.Count; i++)
            {
                Furniture furniture = _placedFurniture[i].furniture;
                GameObject instance = _placedFurniture[i].instance;

                projectData.Furniture[i] = new PlacedFurnitureData()
                {
                    itemId = furniture.Id,
                    position = instance.transform.position,
                    rotation = instance.transform.rotation.eulerAngles
                };
            }

            UserProject project = _activeProjectService.UserProject;

            bool saved = _projectDataSaver.Save(
                project,
                projectData,
                OnMessage: message => _notificationService.ShowPopup(message, "Save", NotificationType.Success),
                OnError: error => _notificationService.ShowPopup(error, "Save error", NotificationType.Error)
            );
        }

        private void OpenOrderPanel()
        {
            _orderPanel.gameObject.SetActive(true);
            _orderPanel.Open(_placedFurniture);
        }

        private void ExitToLoginScene() => _sceneTransitionController.LoadScene(AppScene.Login);
    }
}