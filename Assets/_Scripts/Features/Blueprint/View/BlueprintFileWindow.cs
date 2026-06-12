using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BlueprintFileWindow : MonoBehaviour
{
    [Inject] private BlueprintManager _blueprintManager;
    [Inject] private ActiveProjectService _activeProjectService;
    [Inject] private ProjectDataSaver _projectSaver;
    [Inject] private NotificationService _notificationService;
    [Inject] private SceneTransitionController _sceneTransitionController;

    [Header("References")]
    [SerializeField] private BlueprintOverlayPreviewRenderer _previewRenderer;

    [Header("Window")]
    [SerializeField] private TMP_Text _title;
    [SerializeField] private Button _fileButton;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Image _previewImage;

    [Header("Buttons")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _saveBlockedButton;

    [SerializeField] private Button _nextSceneButton;
    [SerializeField] private Button _nextBlockedButton;

    [SerializeField] private Button _previousSceneButton;


    private void Awake() => Hide();

    private void OnEnable()
    {
        _fileButton.onClick.AddListener(Show);

        _saveButton.onClick.AddListener(SaveButtonClickHandler);
        _saveBlockedButton.onClick.AddListener(SaveBlockedButtonClickHandler);

        _nextSceneButton.onClick.AddListener(NextSceneButtonClickHandler);
        _nextBlockedButton.onClick.AddListener(NextBlockedButtonClickHandler);

        _previousSceneButton.onClick.AddListener(PreviousSceneButtonClickHandler);
        _closeButton.onClick.AddListener(Hide);
    }
    private void OnDisable()
    {
        _fileButton.onClick.RemoveListener(Show);

        _saveButton.onClick.RemoveListener(SaveButtonClickHandler);
        _saveBlockedButton.onClick.RemoveListener(SaveBlockedButtonClickHandler);

        _nextSceneButton.onClick.RemoveListener(NextSceneButtonClickHandler);
        _nextBlockedButton.onClick.RemoveListener(NextBlockedButtonClickHandler);

        _previousSceneButton.onClick.RemoveListener(PreviousSceneButtonClickHandler);
        _closeButton.onClick.RemoveListener(Hide);
    }

    private void Show()
    {
        _title.text = _activeProjectService.UserProject.ProjectName;

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        UpdatePreview().Forget();

        UpdateButtonsState();
    }
    private void Hide()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private async UniTaskVoid UpdatePreview()
    {
        if (_previewRenderer == null)
        {
            DebugWrapper.LogError(this, "Blueprint preview renderer is not assigned.");
            return;
        }

        if (_previewImage == null)
        {
            DebugWrapper.LogError(this, "Preview image is not assigned.");
            return;
        }

        _canvasGroup.alpha = 0f; // DisableBeforeScreen

        Sprite previewSprite = await _previewRenderer.RenderSprite();

        _canvasGroup.alpha = 1f; //Enable after screen


        if (previewSprite == null)
            return;

        _previewImage.sprite = previewSprite;
    }

    private void SaveButtonClickHandler() => SaveCurrentProject(UpdateButtonsState);
    private void SaveBlockedButtonClickHandler() => _notificationService.ShowPopup("Project is already <b>saved</b>. No changes were found.", "Already saved", NotificationType.Info);

    private void NextSceneButtonClickHandler() => _sceneTransitionController.LoadScene(AppScene.Construction);
    private void NextBlockedButtonClickHandler() => _notificationService.ShowPopup("Project has <u>unsaved changes</u>. <b>Save</b> project before go to the next scene.", "Unsaved changes", NotificationType.Warning);

    private void PreviousSceneButtonClickHandler() => _sceneTransitionController.LoadScene(AppScene.Login);

    private void SaveCurrentProject(Action onSaved)
    {
        if (!TryGetActiveProject(out UserProject userProject, out ProjectData projectData))
            return;

        projectData.Points = _blueprintManager.BlueprintPoints.ToArray();

        _projectSaver.Save(userProject, projectData,
            message =>
            {
                _projectSaver.SavePreviewSprite(userProject, _previewImage.sprite, OnMessage: _ =>
                {
                    _notificationService.ShowPopup("Project has been saved successfully.", "Project saved", NotificationType.Success);
                    onSaved?.Invoke();
                }, OnError: error => _notificationService.ShowPopup(error, "Preview saving error", NotificationType.Error));
            },
            error => _notificationService.ShowPopup(error, "Project saving error", NotificationType.Error)
        );
    }
    private void UpdateButtonsState()
    {
        bool projectChanged = IsProjectChanged() || !_projectSaver.HasPreview(_activeProjectService.UserProject);

        _saveButton.interactable = projectChanged;
        _saveBlockedButton.gameObject.SetActive(!projectChanged);

        _nextSceneButton.interactable = !projectChanged;
        _nextBlockedButton.gameObject.SetActive(projectChanged);
    }

    private bool IsProjectChanged()
    {
        Vector2[] savedPoints = _activeProjectService.ProjectData.Points;
        Vector2[] currentPoints = _blueprintManager.BlueprintPoints.ToArray();

        if (savedPoints.Length != currentPoints.Length)
            return true;

        for (int i = 0; i < savedPoints.Length; i++)
        {
            if (!Mathf.Approximately(savedPoints[i].x, currentPoints[i].x) ||
                !Mathf.Approximately(savedPoints[i].y, currentPoints[i].y))
                return true;
        }

        return false;
    }
    private bool TryGetActiveProject(out UserProject userProject, out ProjectData projectData)
    {
        userProject = _activeProjectService.UserProject;
        projectData = _activeProjectService.ProjectData;

        if (userProject == null)
        {
            _notificationService.ShowPopup("Active project info was not found.", "Project error", NotificationType.Error);
            return false;
        }

        if (projectData == null)
        {
            _notificationService.ShowPopup("Active project data was not found.", "Project error", NotificationType.Error);
            return false;
        }

        return true;
    }
}