using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SQLite;
using UnityEngine;

public class UserProjectModule
{
    private readonly ProjectDataSaver projectDataSaver;
    private readonly UserProjectRepository _projectRepository;

    public event Action<UserProject> ProjectCreated;
    public event Action<UserProject> ProjectDeleted;
    public event Action<UserProject> ProjectRenamed;

    private string _projectNotFound = "Project not found in database";
    private string _projectExists = "Project with name {0} already exists";


    public UserProjectModule(UserProjectRepository projectRepository)
        => _projectRepository = projectRepository;

    public async UniTask<List<UserProject>> GetProjectsByUserId(int userId)
        => await _projectRepository.GetProjectsByUserId(userId);

    public async UniTask CreateProject(int userId, string projectName, string filePath, Action<UserProject> OnComplete, Action<string> OnError = null)
    {
        if (await _projectRepository.ExistsByProjectName(projectName))
            OnError?.Invoke(string.Format(_projectExists, projectName));

        UserProject project = new()
        {
            UserId = userId,
            ProjectName = projectName,
            FilePath = filePath,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        try
        {
            await _projectRepository.Insert(project);

            ProjectCreated?.Invoke(project);

            OnComplete?.Invoke(project);
        }
        catch (SQLiteException ex)
        {
            OnError?.Invoke(ex.ToString());
        }
    }

    public async UniTask RenameProject(UserProject project, string newName, string filePath)
    {
        string oldName = project.ProjectName;
        string oldPath = project.FilePath;

        project.ProjectName = newName;
        project.FilePath = filePath;
        project.UpdatedAt = DateTime.Now;

        try
        {
            await UpdateProject(project);

            ProjectRenamed?.Invoke(project);
        }
        catch (SQLiteException ex)
        {
            project.ProjectName = oldName;
            project.FilePath = oldPath;
            Debug.LogError(ex.ToString());
        }
    }

    public async UniTask UpdateProject(UserProject project)
    {
        DateTime previousDate = project.UpdatedAt;
        project.UpdatedAt = DateTime.Now;
        
        try
        {
            await _projectRepository.Update(project);
        }
        catch
        {
            project.UpdatedAt = previousDate;
        }
    }
    public async UniTask UpdateProjectTimestamp(UserProject project)
    {
        project.UpdatedAt = DateTime.Now;
        await _projectRepository.Update(project);
    }

    public async UniTask DeleteProject(UserProject project)
    {
        await _projectRepository.Delete(project);

        ProjectDeleted?.Invoke(project);
    }
}
