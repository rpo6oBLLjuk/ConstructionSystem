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

    private const string ModelFileName = "model";
    private const string PreviewFileName = "preview";

    private readonly string[] _modelExtensions = { ".glb" };
    private readonly string[] _previewExtensions = { ".jpg", ".jpeg", ".png" };

    private string BaseDirectory => Path.Combine(Application.persistentDataPath, RootDirectory);


    public FurnitureDataSaver() => EnsureBaseDirectory();

    public bool SaveModelFile(int furnitureId, string sourceFilePath, Action<string> onMessage = null, Action<string> onError = null) => SaveFurnitureFile(furnitureId, sourceFilePath, ModelFileName, _modelExtensions, onMessage, onError);
    public bool SavePreviewFile(int furnitureId, string sourceFilePath, Action<string> onMessage = null, Action<string> onError = null) => SaveFurnitureFile(furnitureId, sourceFilePath, PreviewFileName, _previewExtensions, onMessage, onError);
    public bool SavePreviewBytes(int furnitureId, byte[] bytes, string extension, Action<string> onMessage = null, Action<string> onError = null)
    {
        extension = NormalizeExtension(extension);

        if (!IsExtensionAllowed(extension, _previewExtensions))
        {
            onError?.Invoke($"Invalid preview format: {extension}");
            return false;
        }

        string directory = GetFurnitureDirectory(furnitureId);
        DeleteExistingFiles(directory, PreviewFileName, _previewExtensions);

        string fileName = $"{PreviewFileName}{extension}";
        string path = Path.Combine(directory, fileName);

        try
        {
            File.WriteAllBytes(path, bytes);
            onMessage?.Invoke($"Preview saved: {fileName}");
            return true;
        }
        catch (Exception e)
        {
            onError?.Invoke($"Failed to save preview: {e.Message}");
            return false;
        }
    }

    public bool HasModel(int furnitureId) => !string.IsNullOrWhiteSpace(GetModelPath(furnitureId));
    public bool HasPreview(int furnitureId) => !string.IsNullOrWhiteSpace(GetPreviewPath(furnitureId));

    public string GetModelPath(int furnitureId) => GetExistingFurnitureFilePath(furnitureId, ModelFileName, _modelExtensions);
    public string GetPreviewPath(int furnitureId) => GetExistingFurnitureFilePath(furnitureId, PreviewFileName, _previewExtensions);

    public async UniTask LoadPreviewSprite(int furnitureId, Action<Sprite> onComplete, Action<string> onError = null)
    {
        string path = GetPreviewPath(furnitureId);

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            onError?.Invoke("Preview file not found");
            return;
        }

        await LoadPreviewByAbsolutePath(path, onComplete, onError);
    }
    public async UniTask LoadPreviewByAbsolutePath(string path, Action<Sprite> onComplete, Action<string> onError = null)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture("file://" + path);

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            return;
        }
        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        onComplete?.Invoke(ConvertTextureToSprite(texture));
    }

    public async UniTask LoadModelGameObject(int furnitureId, Transform parent, Action<GameObject> onComplete, Action<string> onError = null)
    {
        string path = GetModelPath(furnitureId);

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            onError?.Invoke("Model file not found");
            return;
        }

        await LoadModelByAbsolutePath(path, parent, onComplete, onError);
    }
    public async UniTask LoadModelByAbsolutePath(string path, Transform parent, Action<GameObject> onComplete, Action<string> onError = null)
    {
        var gltfImport = new GLTFast.GltfImport();
        try
        {
            bool loaded = await gltfImport.Load(path, null);

            if (!loaded)
            {
                onError?.Invoke("glTFast failed to load or parse the GLB file");
                return;
            }

            GameObject rootObject = new("FurnitureModel");
            rootObject.transform.SetParent(parent, false);

            bool instantiated = await gltfImport.InstantiateMainSceneAsync(rootObject.transform);

            if (!instantiated)
            {
                GameObject.Destroy(rootObject);
                onError?.Invoke("Failed to instantiate glTF scene components");
                return;
            }

            onComplete?.Invoke(rootObject);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("GLB loading was cancelled.");
        }
        catch (Exception ex)
        {
            onError?.Invoke($"Critical error while loading GLB: {ex.Message}");
        }
    }

    public Sprite ConvertTextureToSprite(Texture2D texture)
    {
        Rect rect = new(0, 0, texture.width, texture.height);

        Sprite sprite = Sprite.Create(texture, rect, Vector2.one * 0.5f);
        return sprite;
    }


    public bool DeleteFurnitureData(int furnitureId, Action<string> onMessage = null, Action<string> onError = null)
    {
        if (furnitureId <= 0)
        {
            onError?.Invoke("Invalid furniture id.");
            return false;
        }

        string directory = GetFurnitureDirectory(furnitureId);
        if (!Directory.Exists(directory))
        {
            onMessage?.Invoke($"Furniture directory does not exist: {furnitureId}");
            return true;
        }

        try
        {
            Directory.Delete(directory, true);
            onMessage?.Invoke($"Furniture data deleted: {furnitureId}");
            return true;
        }
        catch (Exception e)
        {
            onError?.Invoke($"Failed to delete furniture data: {e.Message}");
            return false;
        }
    }

    private bool SaveFurnitureFile(int furnitureId, string sourceFilePath, string targetFileNameWithoutExtension, string[] allowedExtensions, Action<string> onMessage, Action<string> onError)
    {
        if (furnitureId <= 0)
        {
            onError?.Invoke("Invalid furniture id.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
        {
            onError?.Invoke($"File not found: {sourceFilePath}");
            return false;
        }

        string extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();

        if (!IsExtensionAllowed(extension, allowedExtensions))
        {
            onError?.Invoke($"Invalid file format: {extension}");
            return false;
        }

        string furnitureDirectory = GetFurnitureDirectory(furnitureId);

        DeleteExistingFiles(furnitureDirectory, targetFileNameWithoutExtension, allowedExtensions);

        string targetFileName = $"{targetFileNameWithoutExtension}{extension}";
        string destinationPath = Path.Combine(furnitureDirectory, targetFileName);

        try
        {
            File.Copy(sourceFilePath, destinationPath, true);
            onMessage?.Invoke($"File saved: {targetFileName}");
            return true;
        }
        catch (Exception e)
        {
            onError?.Invoke($"Failed to save file: {e.Message}");
            return false;
        }
    }
    private void DeleteExistingFiles(string directory, string fileNameWithoutExtension, string[] extensions)
    {
        foreach (string extension in extensions)
        {
            string path = Path.Combine(directory, $"{fileNameWithoutExtension}{extension}");

            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private string GetFurnitureDirectory(int furnitureId)
    {
        EnsureBaseDirectory();

        string furnitureDirectory = Path.Combine(BaseDirectory, furnitureId.ToString());

        if (!Directory.Exists(furnitureDirectory))
            Directory.CreateDirectory(furnitureDirectory);

        return furnitureDirectory;
    }
    private string GetExistingFurnitureFilePath(int furnitureId, string fileNameWithoutExtension, string[] allowedExtensions)
    {
        if (furnitureId <= 0)
            return string.Empty;

        string directory = GetFurnitureDirectory(furnitureId);

        foreach (string extension in allowedExtensions)
        {
            string path = Path.Combine(directory, $"{fileNameWithoutExtension}{extension}");
            if (File.Exists(path))
                return path;
        }

        return string.Empty;
    }

    private void EnsureBaseDirectory()
    {
        if (!Directory.Exists(BaseDirectory))
            Directory.CreateDirectory(BaseDirectory);
    }

    private bool IsExtensionAllowed(string extension, string[] allowedExtensions)
    {
        extension = NormalizeExtension(extension);

        foreach (string allowedExtension in allowedExtensions)
        {
            if (extension == allowedExtension)
                return true;
        }

        return false;
    }
    private string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return string.Empty;

        extension = extension.ToLowerInvariant();

        return extension.StartsWith(".")
            ? extension
            : $".{extension}";
    }
}