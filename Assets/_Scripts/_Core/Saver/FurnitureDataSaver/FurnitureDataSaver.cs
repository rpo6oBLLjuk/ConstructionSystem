using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class FurnitureDataSaver
{
    public string[] ModelExtensions => _modelExtensions.Select(ext => ext.TrimStart('.')).ToArray();
    public string[] PreviewExtensions => _previewExtensions.Select(ext => ext.TrimStart('.')).ToArray();

    private const string RootDirectory = "S3/FurnitureData";


    private readonly string[] _modelExtensions = { ".glb", ".fbx" };
    private readonly string[] _previewExtensions = { ".jpg", ".jpeg", ".png" };

    private string BaseDirectory => Path.Combine(Application.persistentDataPath, RootDirectory);


    public FurnitureDataSaver() => EnsureBaseDirectory();

    public string SaveModelFile(int furnitureId, string sourceFilePath, Action<string> onMessage = null, Action<string> onError = null) => SaveFurnitureFile(furnitureId, sourceFilePath, _modelExtensions, "model", onMessage, onError);
    public string SavePreviewFile(int furnitureId, string sourceFilePath, Action<string> onMessage = null, Action<string> onError = null) => SaveFurnitureFile(furnitureId, sourceFilePath, _previewExtensions, "preview", onMessage, onError);

    public string GetModelPath(int furnitureId, string fileName) => GetFurnitureFilePath(furnitureId, fileName);
    public string GetPreviewPath(int furnitureId, string fileName) => GetFurnitureFilePath(furnitureId, fileName);

    public string GetFileName(string sourceFilePath) => string.IsNullOrWhiteSpace(sourceFilePath) ? string.Empty : Path.GetFileName(sourceFilePath);

    public async UniTask LoadPreviewSprite(int furnitureId, string fileName, Action<Sprite> onComplete, Action<string> onError = null)
    {
        string path = GetPreviewPath(furnitureId, fileName);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            onError?.Invoke("Empty file path or directory");
            return;
        }

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + path))
        {
            await uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                Rect rect = new Rect(0, 0, texture.width, texture.height);
                onComplete?.Invoke(Sprite.Create(texture, rect, Vector2.one * 0.5f));
            }
            else
                onError?.Invoke(uwr.error);
        }
    }
    public async UniTask LoadModelGameObject(int furnitureId, string fileName, Transform parent, Action<GameObject> onComplete, Action<string> onError = null)
    {
        string path = GetModelPath(furnitureId, fileName);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            onError?.Invoke("Empty file path or directory");
            return;
        }

        var gltfImport = new GLTFast.GltfImport();
        try
        {
            bool success = await gltfImport.Load(path, null); // File loading

            if (success)
            {
                GameObject rootObject = new GameObject($"Furniture_{furnitureId}"); // GO-container for loaded model
                rootObject.transform.SetParent(parent, false);

                bool instantiateSuccess = await gltfImport.InstantiateMainSceneAsync(rootObject.transform); //Async create GO

                if (instantiateSuccess)
                {
                    onComplete?.Invoke(rootObject);
                }
                else
                {
                    GameObject.Destroy(rootObject);
                    onError?.Invoke("Failed to instantiate glTF scene components");
                }
            }
            else
            {
                onError?.Invoke("gltFast failed to load or parse the GLB file");
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Загрузка GLB была отменена.");
        }
        catch (Exception ex)
        {
            onError?.Invoke($"Critical error while loading GLB: {ex.Message}");
        }
    }

    private string SaveFurnitureFile(int furnitureId, string sourceFilePath, string[] allowedExtensions, string fileTypeName, Action<string> onMessage, Action<string> onError)
    {
        if (furnitureId <= 0)
        {
            onError?.Invoke($"Invalid furniture id for {fileTypeName} file.");
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            onError?.Invoke($"{fileTypeName} file path is empty.");
            return string.Empty;
        }

        if (!File.Exists(sourceFilePath))
        {
            onError?.Invoke($"{fileTypeName} file not found: {sourceFilePath}");
            return string.Empty;
        }

        string extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();

        if (!IsExtensionAllowed(extension, allowedExtensions))
        {
            onError?.Invoke($"Invalid {fileTypeName} file format: {extension}");
            return string.Empty;
        }

        string fileName = Path.GetFileName(sourceFilePath);
        string furnitureDirectory = GetFurnitureDirectory(furnitureId);

        if (!Directory.Exists(furnitureDirectory))
            Directory.CreateDirectory(furnitureDirectory);

        string destinationPath = Path.Combine(furnitureDirectory, fileName);

        try
        {
            File.Copy(sourceFilePath, destinationPath, true);

            onMessage?.Invoke($"{fileTypeName} file saved: {fileName}");

            return fileName;
        }
        catch (Exception e)
        {
            onError?.Invoke($"Failed to save {fileTypeName} file: {e.Message}");
            return string.Empty;
        }
    }

    private string GetFurnitureFilePath(int furnitureId, string fileName) => furnitureId <= 0 || string.IsNullOrWhiteSpace(fileName) ? string.Empty : Path.Combine(GetFurnitureDirectory(furnitureId), fileName);
    private string GetFurnitureDirectory(int furnitureId)
    {
        EnsureBaseDirectory();

        string furnitureDirectory = Path.Combine(BaseDirectory, furnitureId.ToString());

        if (!Directory.Exists(furnitureDirectory))
            Directory.CreateDirectory(furnitureDirectory);

        return furnitureDirectory;
    }

    private void EnsureBaseDirectory()
    {
        if (!Directory.Exists(BaseDirectory))
            Directory.CreateDirectory(BaseDirectory);
    }
    private bool IsExtensionAllowed(string extension, string[] allowedExtensions)
    {
        foreach (string allowedExtension in allowedExtensions)
        {
            if (extension == allowedExtension)
                return true;
        }

        return false;
    }
}