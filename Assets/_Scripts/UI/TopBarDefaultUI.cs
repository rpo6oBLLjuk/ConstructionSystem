using System;
using UnityEngine;
using UnityEngine.UI;

public class TopBarDefaultUI : MonoBehaviour
{
    void OnEnable()
    {
        Screen.fullScreenMode = FullScreenMode.MaximizedWindow;

#if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Confined;
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
}
