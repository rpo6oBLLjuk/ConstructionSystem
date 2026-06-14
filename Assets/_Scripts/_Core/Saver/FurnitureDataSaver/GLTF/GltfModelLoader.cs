using System;
using Cysharp.Threading.Tasks;
using GLTFast;
using GLTFast.Loading;
using UnityEngine;

public class GltfModelLoader
{
    private TimeBudgetPerFrameDeferAgent _deferAgentComponent;
    private IDeferAgent _deferAgent;
    private bool _isLoading = false;

    public GltfModelLoader() => _deferAgent = GetDeferAgent();


    public async UniTask LoadModel(string modelPath, Transform parent, Action<GameObject> onComplete = null, Action<string> onError = null)
    {
        if (_isLoading)
        {
            onError?.Invoke("During the model upload process.");
            return;
        }

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

        _isLoading = true;

        try
        {
            var settings = new DefaultDownloadProvider();

            GltfImport gltf = new(deferAgent: _deferAgent);
            string uri = new Uri(modelPath).AbsoluteUri;

            bool loaded = await gltf.Load(uri);

            if (!loaded)
            {
                onError?.Invoke("glTF model loading failed.");
                return;
            }

            await UniTask.Yield(PlayerLoopTiming.Update);

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
        finally
        {
            _isLoading = false;
        }
    }

    private IDeferAgent GetDeferAgent()
    {
        if (_deferAgent != null)
            return _deferAgent;

        GameObject deferAgentObject = new("Gltf Defer Agent");
        UnityEngine.Object.DontDestroyOnLoad(deferAgentObject);

        _deferAgentComponent = deferAgentObject.AddComponent<TimeBudgetPerFrameDeferAgent>();
        _deferAgent = _deferAgentComponent;

        return _deferAgent;
    }

}
