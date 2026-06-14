using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public abstract class AbstractSaver
{
    protected string BaseDirectory { get; }


    protected AbstractSaver(string baseDirectory)
    {
        BaseDirectory = Path.Combine(Application.persistentDataPath, baseDirectory);

        if (!Directory.Exists(BaseDirectory))
            Directory.CreateDirectory(BaseDirectory);
    }

    //public Sprite ConvertTextureToSprite(Texture2D texture)
    //{
    //    if (texture == null)
    //        return null;

    //    Rect rect = new(0, 0, texture.width, texture.height);
    //    Vector2 pivot = new(0.5f, 0.5f);

    //    return Sprite.Create(texture, rect, pivot);
    //}

    protected string GetDirectory(bool create, params string[] parts)
    {
        string path = BaseDirectory;

        foreach (string part in parts)
            path = Path.Combine(path, part);

        if (create && !Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }
    protected string GetFilePath(string fileName, bool createDirectory, params string[] directoryParts)
    {
        string directory = GetDirectory(createDirectory, directoryParts);
        return Path.Combine(directory, fileName);
    }

    protected bool Exists(string path) => File.Exists(path);
    protected bool DirectoryExists(string path) => Directory.Exists(path);

    protected bool SaveBytes(string path, byte[] bytes, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        if (bytes == null || bytes.Length == 0)
        {
            OnError?.Invoke("File data is empty.");
            return false;
        }

        try
        {
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(path, bytes);
            OnMessage?.Invoke($"File saved: <b>{Path.GetFileName(path)}</b>");
            return true;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"File save failed: <b>{e.Message}</b>");
            return false;
        }
    }
    protected bool SaveText(string path, string text, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        try
        {
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, text);
            OnMessage?.Invoke($"File saved: <b>{Path.GetFileName(path)}</b>");
            return true;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"File save failed: <b>{e.Message}</b>");
            return false;
        }
    }

    protected string LoadText(string path, Action<string> OnError = null)
    {
        if (!Exists(path))
        {
            OnError?.Invoke($"File not found: <b>{Path.GetFileName(path)}</b>");
            return null;
        }

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"File load failed: <b>{e.Message}</b>");
            return null;
        }
    }

    protected bool CopyFile(string sourcePath, string targetPath, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            OnError?.Invoke("Source file not found.");
            return false;
        }

        try
        {
            string directory = Path.GetDirectoryName(targetPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.Copy(sourcePath, targetPath, true);
            OnMessage?.Invoke($"File copied: <b>{Path.GetFileName(targetPath)}</b>");
            return true;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"File copy failed: <b>{e.Message}</b>");
            return false;
        }
    }
    protected bool DeleteDirectory(string path, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        if (!Directory.Exists(path))
        {
            OnError?.Invoke("Directory not found.");
            return false;
        }

        try
        {
            Directory.Delete(path, true);
            OnMessage?.Invoke("Directory deleted.");
            return true;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Directory delete failed: <b>{e.Message}</b>");
            return false;
        }
    }

    protected async UniTask LoadSpriteFromPath(string path, Action<Texture> onComplete = null, Action<string> onError = null)
    {
        if (!Exists(path))
        {
            onError?.Invoke($"Image not found. Path: '<b>{path}</b>'");
            return;
        }

        using UnityWebRequest request = UnityWebRequestTexture.GetTexture($"file:///{path}");

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            return;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        //Sprite sprite = ConvertTextureToSprite(texture);

        onComplete?.Invoke(texture);
    }
    public async UniTask LoadSpriteByAbsolutePath(string path, Action<Texture> onComplete = null, Action<string> onError = null) => await LoadSpriteFromPath(path, onComplete, onError);

    protected string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return string.Empty;

        return extension.StartsWith(".") ? extension : $".{extension}";
    }
}