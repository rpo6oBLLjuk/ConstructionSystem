using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ProjectDataSaver : AbstractSaver
{
    private const string DataFileName = "projectData.json";
    private const string PreviewFileName = "preview.jpg";

    public ProjectDataSaver() : base("S3/ProjectsData") { }

    public bool Save(UserProject project, ProjectData data, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        if (!IsProjectValid(project, OnError))
            return false;

        if (data == null)
        {
            OnError?.Invoke("Project data is empty.");
            return false;
        }

        string path = GetProjectDataPath(project);

        try
        {
            string json = JsonUtility.ToJson(data, false);
            bool saved = SaveText(path, json, null, OnError);

            if (saved)
                OnMessage?.Invoke($"Project <b>{project.ProjectName}</b> saved successfully.");

            return saved;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Save failed: <b>{e.Message}</b>");
            return false;
        }
    }
    public ProjectData Load(UserProject project, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        if (!IsProjectValid(project, OnError))
            return default;

        string path = GetProjectDataPath(project);
        string json = LoadText(path, OnError);

        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
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
        if (!IsProjectValid(project, OnError))
            return false;

        string path = GetProjectDirectory(project);

        return DeleteDirectory(path, message => OnMessage?.Invoke($"Project <b>{project.ProjectName}</b> deleted."), OnError);
    }

    public bool SavePreviewBytes(UserProject project, byte[] bytes, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        if (!IsProjectValid(project, OnError))
            return false;

        string path = GetProjectPreviewPath(project);
        return SaveBytes(path, bytes, OnMessage, OnError);
    }
    public bool SavePreviewTexture(UserProject project, Texture2D texture, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        if (texture == null)
        {
            OnError?.Invoke("Preview texture is empty.");
            return false;
        }

        byte[] bytes = texture.EncodeToJPG(90);
        return SavePreviewBytes(project, bytes, OnMessage, OnError);
    }
    public bool SavePreviewSprite(UserProject project, Sprite sprite, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        if (sprite == null || sprite.texture == null)
        {
            OnError?.Invoke("Preview sprite is empty.");
            return false;
        }

        byte[] bytes = sprite.texture.EncodeToJPG(90);
        return SavePreviewBytes(project, bytes, OnMessage, OnError);
    }

    public async UniTask LoadPreviewSprite(UserProject project, Action<Sprite> onComplete = null, Action<string> onError = null)
    {
        if (!IsProjectValid(project, onError))
            return;

        if (!HasPreview(project))
        {
            onError?.Invoke("Project preview not found.");
            return;
        }

        await LoadSpriteFromPath(GetProjectPreviewPath(project), onComplete, onError);
    }

    public bool HasPreview(UserProject project) => IsProjectValid(project) && Exists(GetProjectPreviewPath(project));

    private string GetProjectDirectory(UserProject project) => GetDirectory(true, project.UserId.ToString(), project.Id.ToString());
    private string GetProjectDataPath(UserProject project) => GetFilePath(DataFileName, true, project.UserId.ToString(), project.Id.ToString());
    private string GetProjectPreviewPath(UserProject project) => GetFilePath(PreviewFileName, true, project.UserId.ToString(), project.Id.ToString());

    private bool IsProjectValid(UserProject project, Action<string> OnError = null)
    {
        if (project == null)
        {
            OnError?.Invoke("Project is null.");
            return false;
        }

        if (project.UserId <= 0)
        {
            OnError?.Invoke("Project user id is incorrect.");
            return false;
        }

        if (project.Id <= 0)
        {
            OnError?.Invoke("Project id is incorrect.");
            return false;
        }

        return true;
    }
}