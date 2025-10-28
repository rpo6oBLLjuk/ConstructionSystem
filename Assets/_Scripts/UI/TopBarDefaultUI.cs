using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TopBarDefaultUI : MonoBehaviour
{
    [SerializeField] BorderlessToggle borderlessToggleController;

    [Space]
    [SerializeField] Button minimizeButton;
    [SerializeField] Button fullScreenButton;
    [SerializeField] Button closeButton;

    private bool fullScreen = true;


    void OnEnable()
    {
        minimizeButton.onClick.AddListener(MinimizeButtonClick);
        fullScreenButton.onClick.AddListener(MaximizeButtonClick);
        closeButton.onClick.AddListener(CloseButtonClick);
    }

    void OnDisable()
    {
        minimizeButton.onClick.RemoveListener(MinimizeButtonClick);
        fullScreenButton.onClick.RemoveListener(MaximizeButtonClick);
        closeButton.onClick.RemoveListener(CloseButtonClick);
    }

    private void Start()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.fullScreen = true;
        fullScreen = Screen.fullScreen;

        StartCoroutine(Coroutine());

        MaximizeButtonClick();
        MaximizeButtonClick();
    }

    private void MinimizeButtonClick()
    {
        borderlessToggleController.OnClickMinimize();
    }
    private void MaximizeButtonClick()
    {
        if (fullScreen)
        {
            borderlessToggleController.SetBorderless();

            Screen.fullScreen = false;
            Screen.fullScreenMode |= FullScreenMode.Windowed;
            borderlessToggleController.OnClickRestore();
        }
        else
        {
            Screen.fullScreen = true;
            Screen.fullScreenMode |= FullScreenMode.FullScreenWindow;
            borderlessToggleController.SetBordered();
            borderlessToggleController.OnClickMaximize();
        }

        fullScreen = !fullScreen;

#if !UNITY_EDITOR
        Debug.LogError($"[UI] Fullscreen value: {fullScreen}, Screen.FullScreen: {Screen.fullScreen}, ScreenMode: {Enum.GetName(typeof(FullScreenMode), Screen.fullScreenMode)} ");
#endif
    }

    private void CloseButtonClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator Coroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            //
        }
    }
}
