using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SQLite;

public class UserProjectModule
{
    private readonly ProjectDataSaver projectDataSaver;
    private readonly UserProjectRepository _projectRepository;

    public event Action<UserProject> ProjectCreated;
    public event Action<UserProject> ProjectDeleted;
    public event Action<UserProject> ProjectRenamed;

    private string _projectNotFound = "Project not found in database";


    public UserProjectModule(UserProjectRepository projectRepository)
        => _projectRepository = projectRepository;

    public async UniTask<List<UserProject>> GetProjectsByUserId(int userId)
        => await _projectRepository.GetProjectsByUserId(userId);

    public async UniTask<UserProject> CreateProject(int userId, string projectName, string filePath)
    {
        UserProject project = new()
        {
            UserId = userId,
            ProjectName = projectName,
            FilePath = projectName,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        try
        {
            await _projectRepository.Insert(project);

            ProjectCreated?.Invoke(project);
            return project;
        }
        catch (SQLiteException ex)
        {
            return null;
        }
    }

    public async UniTask RenameProject(UserProject project, string newName, string filePath)
    {
        string oldName = project.ProjectName;
        project.ProjectName = newName;
        project.UpdatedAt = DateTime.Now;

        try
        {
            await UpdateProject(project);

            ProjectRenamed?.Invoke(project);
        }
        catch (SQLiteException ex)
        {
            project.ProjectName = oldName;
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
