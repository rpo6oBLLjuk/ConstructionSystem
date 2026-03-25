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
    }

    public void ShowPopup(string message, string title, NotificationType notificationType)
    {
        _popupFactory.CreatePopupNotification(message, title, notificationType);
        DebugWrapper.InactiveLog(this, $"Popup Msg: {message}, title: {title}, type: {Enum.GetName(typeof(NotificationType), notificationType)}");
    }
    public void ShowDialog(string message, string title, List<(string, Action)> buttons)
    {
        _dialogFactory.ShowDialog(message, title, buttons);
        DebugWrapper.InactiveLog(this, $"Dialog Msg: {message}, title: {title}");
    }

#if UNITY_EDITOR
    private async UniTask TestPopup()
    {
        await UniTask.Delay(500);
        for (int i = 0; i < 3; i++)
        {
            ShowPopup("TestPopup", "Info title", NotificationType.Info);
            await UniTask.Delay(250);
            ShowPopup("TestPopup", "Warning title", NotificationType.Warning);
            await UniTask.Delay(250);
            ShowPopup("TestPopup", "Error title", NotificationType.Error);
            await UniTask.Delay(1000);
        }
    }
    private async UniTask TestDialog()
    {
        await UniTask.Delay(500);
        for (int i = 0; i < 3; i++)
        {
            ShowDialog("Do you want to save file?", "Save?", new List<(string, Action)>
            {
                ("No", () => {}),
                ("Yes", () => {}),
                ("Cancel", () => {})
            });
            await UniTask.Delay(3000);
        }
    }
#endif
}
