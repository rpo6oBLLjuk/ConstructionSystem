using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NotificationService : MonoBehaviour
{
    [SerializeField] PopupFactory _popupFactory;
    [SerializeField] DialogFactory _dialogFactory;


    private void Start()
    {
        _popupFactory.Start();
        _dialogFactory.Start();

        ShowDialog("Do you want to save file?", "Save?", new List<(string, Action)>
        {
            ("No", () => Debug.Log("Dialog answer: No")),
            ("Yes", () => Debug.Log("Dialog answer: Yes")),
            ("Cancel", () => Debug.Log("Dialog answer: Cancel"))
        });

        TestNotify().Forget();
    }

    public void ShowPopup(string message, string title, NotificationType notificationType)
    {
        _popupFactory.CreatePopupNotification(message, title, notificationType);
        DebugWrapper.Log(this, $"Popup Msg: {message}, title: {title}, type: {Enum.GetName(typeof(NotificationType), notificationType)}");
    }

    public void ShowDialog(string message, string title, List<(string, Action)> buttons)
    {
        _dialogFactory.ShowDialog(message, title, buttons);
        DebugWrapper.Log(this, $"Dialog Msg: {message}, title: {title}");
    }

    private async UniTask TestNotify()
    {
        await UniTask.Delay(3000);
        ShowDialog("Do you want to save file?", "Save?", new List<(string, Action)>
        {
            ("No", () => Debug.Log("Dialog answer: No")),
            ("Yes", () => Debug.Log("Dialog answer: Yes")),
            ("Cancel", () => Debug.Log("Dialog answer: Cancel"))
        });
        await UniTask.Delay(3000);
        ShowDialog("Do you want to save file?", "Save?", new List<(string, Action)>
        {
            ("No", () => Debug.Log("Dialog answer: No")),
            ("Yes", () => Debug.Log("Dialog answer: Yes")),
            ("Cancel", () => Debug.Log("Dialog answer: Cancel"))
        });
    }
}
