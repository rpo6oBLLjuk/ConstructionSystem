using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BlueprintFileWindow : MonoBehaviour
{
    [Inject] private BlueprintManager _blueprintManager;
    [Inject] private ActiveProjectService _activeProjectService;
    [Inject] private ProjectDataSaver _projectSaver;
    [Inject] private UserModule _userModule;
    [Inject] private NotificationService _notificationService;
    [Inject] private SceneTransitionController _sceneTransitionController;

    [Header("Open Button")]
    [SerializeField] private Button _fileButton;

    [Header("Window")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Buttons")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _nextSceneButton;
    [SerializeField] private Button _closeButton;

    [Header("Scene")]
    [SerializeField] private AppScene _nextScene;

    [Header("Settings")]
    [SerializeField] private bool _saveBeforeNextScene = true;

    private bool _isVisible;


    private void Awake() => Hide();

    private void OnEnable()
    {
        _fileButton.onClick.AddListener(Toggle);
        _saveButton.onClick.AddListener(SaveButtonClickHandler);
        _nextSceneButton.onClick.AddListener(NextSceneButtonClickHandler);
        _closeButton.onClick.AddListener(Hide);
    }
    private void OnDisable()
    {
        _fileButton.onClick.RemoveListener(Toggle);
        _saveButton.onClick.RemoveListener(SaveButtonClickHandler);
        _nextSceneButton.onClick.RemoveListener(NextSceneButtonClickHandler);
        _closeButton.onClick.RemoveListener(Hide);
    }

    private void Toggle()
    {
        if (_isVisible)
            Hide();
        else
            Show();
    }
    private void Show()
    {
        _isVisible = true;

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
    private void Hide()
    {
        _isVisible = false;

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void SaveButtonClickHandler() => SaveCurrentProject(showSuccessMessage: true, onSaved: null);
    private void NextSceneButtonClickHandler()
    {
        if (_saveBeforeNextScene)
        {
            SaveCurrentProject(showSuccessMessage: false, onSaved: () => _sceneTransitionController.LoadScene(_nextScene));
            return;
        }

        _sceneTransitionController.LoadScene(_nextScene);
    }

    private void SaveCurrentProject(bool showSuccessMessage, Action onSaved)
    {
        ProjectData projectData = _activeProjectService.SelectedProject;
        ApplyCurrentBlueprintToProject(projectData);

        _projectSaver.Save(_activeProjectService.UserProject, projectData,
            message =>
            {
                if (showSuccessMessage)
                    _notificationService.ShowPopup("Project has been saved successfully.", "Project saved", NotificationType.Success);

                onSaved?.Invoke();
            },
            error => _notificationService.ShowPopup(error, "Project saving error", NotificationType.Error)
        );
    }
    private void ApplyCurrentBlueprintToProject(ProjectData projectData) => projectData.points = _blueprintManager.BlueprintPoints.ToArray();
}