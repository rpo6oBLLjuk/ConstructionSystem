using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    public bool IsInTransition { get; private set; }
    
    [SerializeField] SceneTransitionConfig _config;
    [SerializeField] CanvasGroup _loadScreen;

    [Header("test only")]
    [SerializeField] float _delayBeforeTransition = 0f;
    [SerializeField] bool _sceneLoadOnStart = false;


    private void Start()
    {
        if (_sceneLoadOnStart)
            LoadScene(AppScene.Construction);
    }

    public void LoadScene(AppScene scene)
    {
        LoadAdditiveSceneAsync(_config.Scenes
            .Find(data => data.AppScene == scene).Index)
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

    private void SetLoadscreenState(bool show, out Tween fadeTween)
    {
        _loadScreen.blocksRaycasts = show;
        fadeTween = _loadScreen
            .DOFade(show ? 1 : 0, _config.Duration)
            .From(!show ? 1 : 0)
            .SetEase(show ? _config.ShowEaseType : _config.HideEaseType);
    }

    
}

public enum AppScene
{
    Login,
    Blueprint,
    Construction
}
