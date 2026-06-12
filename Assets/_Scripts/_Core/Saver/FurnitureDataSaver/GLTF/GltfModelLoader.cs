using Cysharp.Threading.Tasks;
using GLTFast;
using System;
using UnityEngine;

public class GltfModelLoader
{
    public async UniTask LoadModel(string modelPath, Transform parent, Action<GameObject> onComplete = null, Action<string> onError = null)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            onError?.Invoke("Model path is empty.");
            return;
        }

        if (parent == null)
        {
            onError?.Invoke("Model parent is null.");
            return;
        }

        ClearParent(parent);

        try
        {
            GltfImport gltf = new();
            string uri = new Uri(modelPath).AbsoluteUri;

            bool loaded = await gltf.Load(uri);

            if (!loaded)
            {
                onError?.Invoke("glTF model loading failed.");
                return;
            }

            GameObject container = new("LoadedModel");
            container.transform.SetParent(parent, false);

            bool instantiated = await gltf.InstantiateMainSceneAsync(container.transform);

            if (!instantiated)
            {
                UnityEngine.Object.Destroy(container);
                onError?.Invoke("glTF model instantiation failed.");
                return;
            }

            onComplete?.Invoke(container);
        }
        catch (Exception e)
        {
            onError?.Invoke(e.Message);
        }
    }
    private void ClearParent(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
    }
}
