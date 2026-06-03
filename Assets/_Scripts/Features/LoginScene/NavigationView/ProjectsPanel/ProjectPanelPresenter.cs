using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        var projectData = _projectSaver.Load(userProject.FilePath, OnError: (error) => _notificationService.ShowPopup(error, "Error receiving data", NotificationType.Error));

        _previousSelectedProject = _currentSelectedProject;

        if (_previousSelectedProject != null)
            _projectGridView.SetProjectActive(_previousSelectedProject, false);

        _currentSelectedProject = userProject;
        _projectGridView.SetProjectActive(userProject, true);

        _blueprintPreviewPanel.ShowBlueprintPreview(projectData, userProject);
    }
    private void HandleNewProjectRequest()
    {
        _notificationService.ShowInputDialog("Enter blueprint name...", "Create New Blueprint", (name) =>
        {
            if (CheckProjectNameExists(name))
                CreateNewProject(name).Forget();
        });
    }

    private void OnProjectOpenRequested()
    {
        if (!CheckProjectSelection())
            return;

        ProjectData projectData = _projectSaver.Load(_currentSelectedProject.FilePath);
        _activeBlueprintService.SetActiveProject(projectData);

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

                _projectSaver.DeleteSave(_currentSelectedProject.FilePath);
                _projectGridView.RemoveUIElement(_currentSelectedProject);

                _currentSelectedProject = null;
                if (_previousSelectedProject != null)
                    HandleProjectSelection(_previousSelectedProject);
            })
        });
    }

    private async UniTask LoadProjects()
    {
        List<UserProject> projects = await _userProjectModule.GetProjectsByUserId(_userModule.CurrentUser.Id);

        _projectGridView.UpdateDataContext(projects);
    }

    private void HandleProjectRename(string newName)
    {
        if (!CheckProjectSelection())
            return;

        if (!CheckProjectNameExists(newName))
            return;

        UserProject userProject = _currentSelectedProject;
        string newFileName = _projectSaver.GetSaveNameByUserId(_userModule.CurrentUser.Id, newName);

        _projectSaver.Rename(userProject.FilePath, newFileName, OnMessage: (msg) =>
        {
            _userProjectModule.RenameProject(userProject, newName, newFileName).Forget();
            _projectGridView.RefreshUIElement(userProject);
        }, OnError: (error) => _notificationService.ShowPopup(error, "Rename error", NotificationType.Error));
    }

    private async UniTask CreateNewProject(string name)
    {
        int userId = _userModule.CurrentUser.Id;
        string fileName = _projectSaver.GetSaveNameByUserId(userId, name);

        await _userProjectModule.CreateProject(userId, name, fileName, OnComplete: (userProject) =>
        {
            ProjectData projectData = new(userId);

            _projectSaver.Save(projectData, fileName, OnMessage: (msg) =>
            {
                _projectGridView.CreateUIElement(userProject);
                HandleProjectSelection(userProject);
            }, OnError: (error) => _notificationService.ShowPopup(error, "Save project error", NotificationType.Error));
        },
        OnError: (error) => _notificationService.ShowPopup(error, "Create project error", NotificationType.Error));
    }

    private bool CheckProjectSelection()
    {
        if (_currentSelectedProject == null)
        {
            _notificationService.ShowPopup($"Select or create project", "Project is not selected", NotificationType.Warning);
            return false;
        }

        return true;
    }

    private bool CheckProjectNameExists(string name)
    {
        if (!_projectSaver.Exists(_projectSaver.GetSaveNameByUserId(_userModule.CurrentUser.Id, name)))
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _notificationService.ShowPopup("File name cannot be empty.", "File create error", NotificationType.Error);
                return false;
            }

            name = name.Trim();

            char[] invalidChars = Path.GetInvalidFileNameChars().Append('.').ToArray();
            if (name.Any(c => invalidChars.Contains(c)))
            {
                string forbiddenSymbols = $"<b>{string.Join(" ", invalidChars.Where(c => !char.IsControl(c)))}</b>";
                _notificationService.ShowPopup($"Name contains forbidden characters. Do not use: {forbiddenSymbols}", "Invalid chars", NotificationType.Error);
                return false;
            }

            return true;
        }
        else
            _notificationService.ShowPopup("You have already created a project with an identical name", "Duplication of projects", NotificationType.Warning);

        return false;
    }

    //Переименование не проверяет наличие дубликата файла, в отличие от создания.
    //Сделать унифицированный метод проверки на манер CheckProjectSelection, подвязать в HandleRename и HandleCreate.
}
