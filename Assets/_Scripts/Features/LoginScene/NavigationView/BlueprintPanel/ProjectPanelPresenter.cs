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
        _projectGridView.OnNewFileSelected += HandleNewFileRequest;

        _blueprintPreviewPanel.OnBlueprintDelete += OnProjectDeleteRequested;
        _blueprintPreviewPanel.OnBlueprintOpen += OnProjectOpenRequested;

        _blueprintPreviewPanel.OnBlueprintRename += HandleProjectRename;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        _projectGridView.EventsContext.OnBlueprintSelected -= HandleProjectSelection;
        _projectGridView.OnNewFileSelected -= HandleNewFileRequest;

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
        _previousSelectedProject = _currentSelectedProject;

        if (_previousSelectedProject != null)
            _projectGridView.SetProjectActive(_previousSelectedProject, false);

        _currentSelectedProject = userProject;
        _projectGridView.SetProjectActive(userProject, true);

        _blueprintPreviewPanel.ShowBlueprintPreview(_projectSaver.Load(userProject.FilePath), userProject);
    }
    private void HandleNewFileRequest()
    {
        _notificationService.ShowInputDialog("Enter blueprint name...", "Create New Blueprint", (name) =>
        {
            if (!_projectSaver.Exists(name))
                CreateNewFile(name).Forget();
            else
                _notificationService.ShowDialog($"A  file named <b>{name}</b> already exists. Do you want to replace it?", "Overwrite File", new List<(string, Action)>
                {
                    ("No", null),
                    ("Yes", () => CreateNewFile(name).Forget())
                });
        });
    }

    private void OnProjectOpenRequested()
    {
        ProjectData projectData = _projectSaver.Load(_currentSelectedProject.FilePath);
        _activeBlueprintService.SetActiveProject(projectData);

        _sceneTransitionController.LoadScene(AppScene.Blueprint);
    }

    private void OnProjectDeleteRequested()
    {
        _userProjectModule.DeleteProject(_currentSelectedProject).Forget();

        _projectSaver.DeleteSave(_currentSelectedProject.FilePath);
        _projectGridView.RemoveUIElement(_currentSelectedProject);

        HandleProjectSelection(_previousSelectedProject);
    }

    private async UniTask LoadProjects()
    {
        List<UserProject> projects = await _userProjectModule.GetProjectsByUserId(_userModule.CurrentUser.Id);

        _projectGridView.UpdateDataContext(projects);
    }

    private void HandleProjectRename(string newName)
    {
        UserProject userProject = _currentSelectedProject;
        string newFileName = _projectSaver.GetSaveNameByUserId(userProject.Id, newName);

        _projectSaver.Rename(userProject.FilePath, newFileName);
        _userProjectModule.RenameProject(userProject, newName, newFileName).Forget();

        _projectGridView.RefreshUIElement(userProject);
    }

    private async UniTask CreateNewFile(string name)
    {
        int userId = _userModule.CurrentUser.Id;
        string fileName = _projectSaver.GetSaveNameByUserId(userId, name);

        UserProject userProject = await _userProjectModule.CreateProject(userId, name, fileName);
        ProjectData projectData = new ProjectData(userId);

        _projectSaver.Save(projectData, fileName);

        _projectGridView.CreateUIElement(userProject);
        HandleProjectSelection(userProject);
    }
}
