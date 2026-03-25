using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    public bool IsInTransition { get; private set; }

    [SerializeField] CanvasGroup _loadScreen;
    [SerializeField] List<SceneItem> scenes;

    [SerializeField] float _duration = 0.5f;
    [SerializeField] Ease _hideEaseType = Ease.InOutQuad;
    [SerializeField] Ease _showEaseType = Ease.OutQuint;

    [Header("test only")]
    [SerializeField] float _delayBeforeTransition = 0f;
    [SerializeField] bool _sceneLoadOnStart = false;


    private void Start()
    {
        ValidateScenes();

        if (_sceneLoadOnStart)
            LoadScene(AppScene.Construction);
    }

    public void LoadScene(AppScene scene)
    {
        LoadAdditiveSceneAsync(scenes
            .Find(sceneItem => sceneItem.AppScene == scene).Index)
        .Forget();
    }

    public async UniTask LoadAdditiveSceneAsync(int index)
    {
        if (IsInTransition)
            return;
        IsInTransition = true;

        await UniTask.WaitForSeconds(_delayBeforeTransition); //TEST ONLY

        SetLoadscreenState(true, out Tween loadScreenTween);

        AsyncOperation asyncSceneLoad = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        asyncSceneLoad.allowSceneActivation = false;

        await UniTask.WaitUntil(() => asyncSceneLoad.progress >= 0.9f && !loadScreenTween.IsPlaying());

        DebugWrapper.Log(this, $"Loaded scene: {System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(index))}");

        asyncSceneLoad.allowSceneActivation = true;

        await UniTask.WaitUntil(() => asyncSceneLoad.isDone);
        SetLoadscreenState(false, out _);
    }

    //лернд ме днохяюм
    private void SetLoadscreenState(bool show, out Tween fadeTween)
    {
        _loadScreen.blocksRaycasts = show;

        fadeTween = _loadScreen
            .DOFade(show ? 1 : 0, _duration)
            .From(!show ? 1 : 0)
            .SetEase(show ? _showEaseType : _hideEaseType);
    }

    //Editor only
    private void Reset()
    {
        scenes = new();
        foreach (AppScene appScene in Enum.GetValues(typeof(AppScene)))
        {
            scenes.Add(new SceneItem()
            {
                AppScene = appScene,
                Index = -1
            });
        }
    }
    private void OnValidate()
    {
        scenes.ForEach(sceneItem =>
        {
            if (sceneItem.Index != -1)
                sceneItem.Name = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneItem.Index));
            else
                sceneItem.Name = "";
        });
    }

    private void ValidateScenes()
    {
        OnValidate();

        foreach (var item in scenes)
            if (item.Name == "")
                DebugWrapper.LogError(this, $"SceneTransitionList contains unsigned SceneIndex: {item.Index}");
    }

    [Serializable]
    private class SceneItem
    {
        public AppScene AppScene;
        public int Index;
        public string Name;
    }
}

public enum AppScene
{
    Login,
    Blueprint,
    Construction
}
