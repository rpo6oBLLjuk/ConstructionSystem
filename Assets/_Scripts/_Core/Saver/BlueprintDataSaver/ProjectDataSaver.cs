using System;
using System.IO;
using UnityEngine;

public class ProjectDataSaver
{
    private string BaseDirectory { get; } = "S3/ProjectsData";
    private string Format { get; } = "json";


    public ProjectDataSaver()
    {
        BaseDirectory = Path.Combine(Application.persistentDataPath, BaseDirectory);
        if (!Directory.Exists(BaseDirectory))
            Directory.CreateDirectory(BaseDirectory);
    }

    public bool Save(UserProject project, ProjectData data, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        if (string.IsNullOrWhiteSpace(project.ProjectName))
        {
            OnError?.Invoke("Save name is empty");
            return false;
        }

        string path = GetPath(project);
        try
        {
            string json = JsonUtility.ToJson(data, false);
            File.WriteAllText(path, json);

            OnMessage?.Invoke($"Project <b>{project.ProjectName}</b> saved successfully.");
            return true;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Save failed: <b>{e.Message}</b>");
            return false;
        }
    }
    public ProjectData Load(UserProject project, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        string path = GetPath(project);
        if (!Exists(path))
        {
            OnError?.Invoke($"Save not found: <b>{project.ProjectName}</b>");
            return default;
        }

        try
        {
            string json = File.ReadAllText(path);
            ProjectData data = JsonUtility.FromJson<ProjectData>(json);

            OnMessage?.Invoke($"Project <b>{project.ProjectName}</b> loaded.");
            return data;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Load failed: <b>{e.Message}</b>");
            return default;
        }
    }
    public bool Delete(UserProject project, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        string path = GetPath(project);
        if (!Exists(path))
        {
            OnError?.Invoke($"Save not found: <b>{project.ProjectName}</b>");
            return false;
        }

        try
        {
            this.FastLog(path);
            File.Delete(path);
            OnMessage?.Invoke($"Project <b>{project.ProjectName}</b> deleted.");
            return true;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Delete failed: <b>{e.Message}</b>");
            return false;
        }
    }

    public bool Rename(UserProject project, string newName, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return false;

        string oldPath = GetPath(project);
        string newPath = GetPath(project.UserId, newName);

        if (Exists(oldPath) && !Exists(newPath))
        {
            File.Move(oldPath, newPath);
            OnMessage?.Invoke($"Project <b>{project.ProjectName}</b> renamed to <b>{newName}</b>.");
            return true;
        }

        OnError?.Invoke("You have already created a project with an identical name");
        return false;
    }

    //private bool Exists(int id, string name) => Exists(GetPath(id, name));
    private bool Exists(string path) => File.Exists(path);

    private string GetPath(UserProject project) => GetPath(project.UserId, project.ProjectName);
    private string GetPath(int id, string name)
    {
        string pathWithUserId = Path.Combine(BaseDirectory, id.ToString());
        if (!Directory.Exists(pathWithUserId))
            Directory.CreateDirectory(pathWithUserId);

        return Path.Combine(pathWithUserId, $"{name}.{Format}");
    }
}
