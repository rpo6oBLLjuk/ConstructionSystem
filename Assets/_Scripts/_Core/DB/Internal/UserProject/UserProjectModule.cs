using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SQLite;
using UnityEngine;

public class UserProjectModule
{
    private readonly UserProjectRepository _projectRepository;

    private string _projectExists = "Project with name {0} already exists";


    public UserProjectModule(UserProjectRepository projectRepository) => _projectRepository = projectRepository;

    public async UniTask<List<UserProject>> GetProjectsByUserId(int userId) => await _projectRepository.GetProjectsByUserId(userId);

    public async UniTask CreateProject(int userId, string projectName, Action<UserProject> OnComplete = null, Action<string> OnError = null)
    {
        if (await _projectRepository.ExistsByProjectName(userId, projectName))
        {
            OnError?.Invoke(string.Format(_projectExists, projectName));
            return;
        }

        UserProject project = new()
        {
            UserId = userId,
            ProjectName = projectName,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        try
        {
            await _projectRepository.Insert(project);
            OnComplete?.Invoke(project);
        }
        catch (SQLiteException ex)
        {
            OnError?.Invoke(ex.ToString());
        }
    }
    public async UniTask RenameProject(UserProject project, string newName, Action<UserProject> OnComplete = null, Action<string> OnError = null)
    {
        if (await _projectRepository.ExistsByProjectName(project.UserId, newName))
        {
            OnError?.Invoke(string.Format(_projectExists, newName));
            return;
        }

        string oldName = project.ProjectName;

        project.ProjectName = newName;
        project.UpdatedAt = DateTime.Now;

        try
        {
            await UpdateProject(project);

            OnComplete?.Invoke(project);
        }
        catch (SQLiteException ex)
        {
            project.ProjectName = oldName;
            OnError?.Invoke(ex.ToString());
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

    public async UniTask DeleteProject(UserProject project) => await _projectRepository.Delete(project);
}
