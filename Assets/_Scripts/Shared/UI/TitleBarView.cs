using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TitleBarView : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    private const int SW_MINIMIZE = 6;
#endif
    [Inject] NotificationService _notificationService;

    [SerializeField] Button _minimizeButton;
    [SerializeField] Button _closeButton;


    private void OnEnable()
    {
        _minimizeButton.onClick.AddListener(MinimizeButtonClick);
        _closeButton.onClick.AddListener(CloseButtonClick);
    }
    private void OnDisable()
    {
        _minimizeButton.onClick.RemoveListener(MinimizeButtonClick);
        _closeButton.onClick.RemoveListener(CloseButtonClick);
    }

    private void MinimizeButtonClick()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        ShowWindow(GetActiveWindow(), SW_MINIMIZE);
#else
        DebugWrapper.InactiveLog(this, "Minimize work only in build");
#endif
    }
    private void CloseButtonClick() => _notificationService.ShowDialog(
        "Log out to the desktop?",
        "Quit",
        new List<(string, Action)>() 
        {
            ("Cancel", null),
            ("Ok", CloseApplication)
        });

    private void CloseApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
