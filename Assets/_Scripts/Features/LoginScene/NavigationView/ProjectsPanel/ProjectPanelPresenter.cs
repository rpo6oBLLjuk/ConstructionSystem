using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class ProjectPanelPresenter : BaseLayoutPresenter
{
    [Inject] ProjectDataSaver _projectSaver;
    [Inject] UserProjectModule _userProjectModule;
    [Inject] UserModule _userModule;

    [Inject] NotificationService _notificationService;
    [Inject] SceneTransitionController _sceneTransitionController;
    [Inject] ActiveProjectService _activeBlueprintService;

    [SerializeField] ProjectGridView _projectGridView;
    [SerializeField] ProjectPreviewPanel _blueprintPreviewPanel;

    private UserProject _currentSelectedProject;
    private UserProject _previousSelectedProject;


    protected override void OnEnable()
    {
        base.OnEnable();

        _projectGridView.EventsContext.OnBlueprintSelected += HandleProjectSelection;
        _projectGridView.OnNewProjectSelected += HandleNewProjectRequest;

        _blueprintPreviewPanel.OnBlueprintDelete += OnProjectDeleteRequested;
        _blueprintPreviewPanel.OnBlueprintOpen += OnProjectOpenRequested;

        _blueprintPreviewPanel.OnBlueprintRename += HandleProjectRename;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        _projectGridView.EventsContext.OnBlueprintSelected -= HandleProjectSelection;
        _projectGridView.OnNewProjectSelected -= HandleNewProjectRequest;

        _blueprintPreviewPanel.OnBlueprintDelete -= OnProjectDeleteRequested;
        _blueprintPreviewPanel.OnBlueprintOpen -= OnProjectOpenRequested;

        _blueprintPreviewPanel.OnBlueprintRename -= HandleProjectRename;
    }

    public override void Show()
    {
        LoadProjects().Forget();
        base.Show();
    }

    private void HandleProjectSelection(UserProject userProject)
    {
        if (userProject == null)
            return;

        var projectData = _projectSaver.Load(userProject, OnError: (error) => _notificationService.ShowPopup(error, "Receiving data error", NotificationType.Error));

        _previousSelectedProject = _currentSelectedProject;

        if (_previousSelectedProject != null)
            _projectGridView.SetProjectActive(_previousSelectedProject, false);

        _currentSelectedProject = userProject;
        _projectGridView.SetProjectActive(userProject, true);

        _blueprintPreviewPanel.ShowBlueprintPreview(projectData, userProject);
    }
    private void HandleNewProjectRequest() => _notificationService.ShowInputDialog("Enter blueprint name...", "Create New Blueprint", (newName) => CreateNewProject(newName).Forget());

    private void OnProjectOpenRequested()
    {
        if (!CheckProjectSelection())
            return;

        ProjectData projectData = _projectSaver.Load(_currentSelectedProject, OnError: error => _notificationService.ShowPopup(error, "Project opening error", NotificationType.Error));
        _activeBlueprintService.SetActiveProject(_currentSelectedProject, projectData);

        _sceneTransitionController.LoadScene(AppScene.Blueprint);
    }
    private void OnProjectDeleteRequested()
    {
        if (!CheckProjectSelection())
            return;

        _notificationService.ShowDialog($"Delete blueprint <b>{_currentSelectedProject.ProjectName}</b>?", "Confirmation of deletion", new List<(string, Action)>
        {
            ("Cancel", null),
            ("OK", () =>
            {
                _userProjectModule.DeleteProject(_currentSelectedProject).Forget();
                _projectGridView.RemoveUIElement(_currentSelectedProject);

                _projectSaver.Delete(_currentSelectedProject, notification =>
                {
                    _currentSelectedProject = null;
                    if (_previousSelectedProject != null)
                        HandleProjectSelection(_previousSelectedProject);

                    _notificationService.ShowPopup(notification, "Project deleted",NotificationType.Success);
                }, error => _notificationService.ShowPopup(error, "Project deletion error",NotificationType.Error));
            })
        });
    }

    private async UniTask LoadProjects()
    {
        List<UserProject> projects = await _userProjectModule.GetProjectsByUserId(_userModule.CurrentUser.Id);

        _projectGridView.UpdateDataContext(projects);
    }
    private async UniTask CreateNewProject(string name)
    {
        int userId = _userModule.CurrentUser.Id;

        await _userProjectModule.CreateProject(userId, name, OnComplete: (userProject) =>
        {
            ProjectData projectData = new();

            _projectSaver.Save(userProject, projectData, notification =>
            {
                _projectGridView.CreateUIElement(userProject);
                HandleProjectSelection(userProject);

                _notificationService.ShowPopup(notification, "Project created", NotificationType.Success);
            }, error => _notificationService.ShowPopup(error, "Project creation error", NotificationType.Error));
        },
        OnError: (error) => _notificationService.ShowPopup(error, "Project creation error", NotificationType.Error));
    }

    private void HandleProjectRename(string newName)
    {
        if (!CheckProjectSelection())
            return;

        if (_currentSelectedProject.ProjectName == newName)
        {
            _notificationService.ShowPopup("You have already created a project with an identical name", "Project renaming error", NotificationType.Info);
            return;
        }

        UserProject userProject = _currentSelectedProject;
        _userProjectModule.RenameProject(userProject, newName, OnComplete: _ =>
        {
            _projectGridView.RefreshUIElement(userProject);
            _notificationService.ShowPopup($"Project renamed to <b>{newName}</b>.", "Project renamed", NotificationType.Success);
        }).Forget();
    }

    private bool CheckProjectSelection()
    {
        if (_currentSelectedProject == null)
        {
            _notificationService.ShowPopup($"Select or create project", "Project is not selected", NotificationType.Info);
            return false;
        }

        return true;
    }
}
