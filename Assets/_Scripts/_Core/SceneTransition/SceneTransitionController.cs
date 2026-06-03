using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class SceneTransitionController : MonoBehaviour
{
    [Inject] private ZenjectSceneLoader _sceneLoader;

    public bool IsInTransition { get; private set; }

    [SerializeField] SceneTransitionConfig _config;
    [SerializeField] CanvasGroup _loadScreen;

    [Header("editor only")]
    [SerializeField] float _delayBeforeTransition = 0f;
    [SerializeField] float _minimalTransitionDuration = 0f;
    [SerializeField] bool _sceneLoadOnStart = false;


    private void Start()
    {
#if UNITY_EDITOR
        if (_sceneLoadOnStart)
            LoadScene(AppScene.Construction);
#endif
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

#if UNITY_EDITOR
        await UniTask.WaitForSeconds(_delayBeforeTransition); //TEST ONLY
#endif

        SetLoadscreenState(true, out Tween loadScreenTween);

        AsyncOperation asyncSceneLoad = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        asyncSceneLoad.allowSceneActivation = false;

        float time = Time.time;
        await UniTask.WaitUntil(() => asyncSceneLoad.progress >= 0.9f && !loadScreenTween.IsPlaying());

        DebugWrapper.Log(this, $"Loaded scene: {System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(index))}");

#if UNITY_EDITOR
        float awaitDuration = _minimalTransitionDuration - (Time.time - time);
        if (awaitDuration > 0f)
            await UniTask.WaitForSeconds(awaitDuration);
#endif
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
