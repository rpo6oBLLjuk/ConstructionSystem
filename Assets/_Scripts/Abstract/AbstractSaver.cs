using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Zenject;

public abstract class AbstractSaver<T>
{
    public event Action<string> OnMessage;
    public event Action<string> OnError;

    protected string BaseDirectory { get; private set; }
    protected string Format { get; private set; } = "json";


    protected AbstractSaver(string directory) //Saves/SaveType/...
    {
        BaseDirectory = Path.Combine(Application.persistentDataPath, directory);

        if (!Directory.Exists(BaseDirectory))
            Directory.CreateDirectory(BaseDirectory);
    }

    public bool ExistsByPath(string fullPath) => File.Exists(fullPath);
    public bool Exists(string saveName) => ExistsByPath(GetPath(saveName));

    public virtual bool Save(T obj, string saveName)
    {
        if (string.IsNullOrWhiteSpace(saveName))
        {
            OnError?.Invoke("Save name is empty");
            return false;
        }

        string path = GetPath(saveName);

        try
        {
            string json = JsonUtility.ToJson(obj, false);
            File.WriteAllText(path, json);

            OnMessage?.Invoke($"File <b>{saveName}</b> saved successfully.");
            return true;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Save failed: <b>{e.Message}</b>");
            return false;
        }
    }
    public virtual T Load(string saveName)
    {
        string path = GetPath(saveName);

        if (!ExistsByPath(path))
        {
            OnError?.Invoke($"Save not found: <b>{saveName}</b>");
            return default;
        }

        try
        {
            string json = File.ReadAllText(path);
            T data = JsonUtility.FromJson<T>(json);

            OnMessage?.Invoke($"File <b>{saveName}</b> loaded.");
            return data;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Load failed: <b>{e.Message}</b>");
            return default;
        }
    }
    public virtual bool DeleteSave(string saveName)
    {
        string path = GetPath(saveName);

        if (!ExistsByPath(path))
            return false;

        try
        {
            File.Delete(path);
            OnMessage?.Invoke($"File <b>{saveName}</b> deleted.");
            return true;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Delete failed: <b>{e.Message}</b>");
            return false;
        }
    }

    public virtual bool Rename(string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return false;

        string oldPath = GetPath(oldName);
        string newPath = GetPath(newName);

        if (ExistsByPath(oldPath) && !ExistsByPath(newPath))
        {
            File.Move(oldPath, newPath);
            OnMessage?.Invoke($"Renamed <b>{oldName}</b> to <b>{newName}</b>");
            return true;
        }

        OnError?.Invoke("Rename failed: Source doesn't exist or target already exists.");
        return false;
    }

    public List<string> GetAllSaveNames()
    {
        if (!Directory.Exists(BaseDirectory))
            return new List<string>();

        return Directory.GetFiles(BaseDirectory, $"*.{Format}")
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();
    }
    public List<T> FindSaves(Predicate<T> match)
    {
        var allNames = GetAllSaveNames();
        var results = new List<T>();

        foreach (var name in allNames)
        {
            T data = Load(name);
            if (data != null && match(data))
                results.Add(data);
        }
        return results;
    }

    protected string GetPath(string name) => Path.Combine(BaseDirectory, $"{name}.{Format}");
}