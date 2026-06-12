using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FurnitureDataSaver : AbstractSaver
{
    private const string ModelFileName = "model";
    private const string PreviewFileName = "preview";

    private readonly GltfModelLoader _gltfModelLoader = new();

    public string[] ModelExtensions { get; } = { "glb" };
    public string[] PreviewExtensions { get; } = { "jpg", "jpeg", "png" };


    public FurnitureDataSaver() : base("S3/FurnitureData") { }

    public bool SaveModelFile(int furnitureId, string sourcePath, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        string extension = Path.GetExtension(sourcePath);

        if (string.IsNullOrWhiteSpace(extension))
        {
            OnError?.Invoke("Model extension is empty.");
            return false;
        }

        if (!IsExtensionAllowed(extension, ModelExtensions))
        {
            OnError?.Invoke($"Model extension <b>{extension}</b> is not supported.");
            return false;
        }

        string targetPath = GetFilePath($"{ModelFileName}{NormalizeExtension(extension)}", true, furnitureId.ToString());
        return CopyFile(sourcePath, targetPath, OnMessage, OnError);
    }
    public bool SavePreviewFile(int furnitureId, string sourcePath, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        string extension = Path.GetExtension(sourcePath);

        if (string.IsNullOrWhiteSpace(extension))
        {
            OnError?.Invoke("Preview extension is empty.");
            return false;
        }

        if (!IsExtensionAllowed(extension, PreviewExtensions))
        {
            OnError?.Invoke($"Preview extension <b>{extension}</b> is not supported.");
            return false;
        }

        return CopyFile(sourcePath, GetPreviewSavePath(furnitureId), OnMessage, OnError);
    }

    public bool SavePreviewBytes(int furnitureId, byte[] bytes, Action<string> OnMessage = null, Action<string> OnError = null) => SaveBytes(GetPreviewSavePath(furnitureId), bytes, OnMessage, OnError);

    public async UniTask LoadPreviewSprite(int furnitureId, Action<Sprite> onComplete = null, Action<string> onError = null)
    {
        await LoadSpriteFromPath(GetPreviewPath(furnitureId), onComplete, onError);
    }
    public async UniTask LoadPreviewByAbsolutePath(string path, Action<Sprite> onComplete = null, Action<string> onError = null)
    {
        await LoadSpriteByAbsolutePath(path, onComplete, onError);
    }

    public async UniTask LoadModelGameObject(int furnitureId, Transform parent, Action<GameObject> onComplete = null, Action<string> onError = null)
    {
        string modelPath = GetModelPath(furnitureId);

        if (string.IsNullOrWhiteSpace(modelPath))
        {
            onError?.Invoke("Model file not found.");
            return;
        }

        await LoadModelByAbsolutePath(modelPath, parent, onComplete, onError);
    }
    public async UniTask LoadModelByAbsolutePath(string modelPath, Transform parent, Action<GameObject> onComplete = null, Action<string> onError = null)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            onError?.Invoke("Model path is empty.");
            return;
        }

        if (!File.Exists(modelPath))
        {
            onError?.Invoke($"Model file not found: <b>{Path.GetFileName(modelPath)}</b>");
            return;
        }

        await _gltfModelLoader.LoadModel(modelPath, parent, onComplete, onError);
    }

    public bool DeleteFurnitureData(int furnitureId, Action<string> OnMessage = null, Action<string> OnError = null)
    {
        string path = GetFurnitureDirectory(furnitureId);
        return DeleteDirectory(path, OnMessage, OnError);
    }

    public string GetFurnitureDirectory(int furnitureId) => GetDirectory(true, furnitureId.ToString());

    public string GetPreviewSavePath(int furnitureId) => GetFilePath($"{PreviewFileName}{NormalizeExtension(PreviewExtensions[0])}", true, furnitureId.ToString());
    public string GetPreviewPath(int furnitureId)
    {
        string directory = GetFurnitureDirectory(furnitureId);

        if (PreviewExtensions == null || PreviewExtensions.Length == 0)
        {
            string path = Path.Combine(directory, PreviewFileName);
            return File.Exists(path) ? path : null;
        }

        foreach (string extension in PreviewExtensions)
        {
            string path = Path.Combine(directory, $"{PreviewFileName}{NormalizeExtension(extension)}");

            if (File.Exists(path))
                return path;
        }

        return null;
    }
    public string GetModelPath(int furnitureId)
    {
        string directory = GetFurnitureDirectory(furnitureId);

        foreach (string extension in ModelExtensions)
        {
            string path = Path.Combine(directory, $"{ModelFileName}{NormalizeExtension(extension)}");

            if (File.Exists(path))
                return path;
        }

        return null;
    }

    private bool IsExtensionAllowed(string extension, string[] allowedExtensions)
    {
        extension = NormalizeExtension(extension).ToLowerInvariant();

        if (allowedExtensions == null || allowedExtensions.Length == 0)
            return false;

        foreach (string allowedExtension in allowedExtensions)
        {
            if (NormalizeExtension(allowedExtension).ToLowerInvariant() == extension)
                return true;
        }

        return false;
    }
}