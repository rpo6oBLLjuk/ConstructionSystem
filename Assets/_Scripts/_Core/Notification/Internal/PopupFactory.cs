using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PopupFactory
{
    [SerializeField] NotificationConfig _notificationConfig;
    [SerializeField] GameObject _defaultPopup;

    List<CanvasGroup> _popupsPool;


    public void Start() => _defaultPopup?.SetActive(false);

    public CanvasGroup CreatePopupNotification(string message, string title, NotificationType notificationType)
    {
        GameObject popup = GameObject.Instantiate(_defaultPopup, _defaultPopup.transform.parent);
        popup.SetActive(true);

        ApplyText(title, "TitleText", popup.transform);
        ApplyText(message, "MessageText", popup.transform);

        ApplyImage(popup.transform, notificationType);

        if (!popup.TryGetComponent(out CanvasGroup canvasGroup))
        {
            DebugWrapper.LogError(this, "Can't find 'CanvasGroup' in _defaultPopup");
            return null;
        }

        AnimatePopup(canvasGroup);

        return popup.GetComponent<CanvasGroup>();
    }

    private void ApplyText(string text, string objectName, Transform transform)
    {
        if (!transform.Find(objectName).TryGetComponent(out TMP_Text pulledText))
            DebugWrapper.LogError(this, "Can't find 'TitleText' in _defaultPopup children");
        else
            pulledText.text = text;
    }
    private void ApplyImage(Transform transform, NotificationType notificationType)
    {
        if (!transform.Find("IconImage").TryGetComponent(out Image pulledImage))
            DebugWrapper.LogError(this, "Can't find 'IconImage' in _defaultPopup children");
        else
            pulledImage.sprite = notificationType switch
            {
                NotificationType.Info => _notificationConfig.InfoSprite,
                NotificationType.Warning => _notificationConfig.WarningSprite,
                NotificationType.Error => _notificationConfig.ErrorSprite,
                _ => (null)
            };
    }
    private Tween AnimatePopup(CanvasGroup canvasGroup) => DOTween.Sequence(canvasGroup)
        .Append(canvasGroup.DOFade(1, _notificationConfig.PopupShowDuration).From(0))
        .AppendInterval(_notificationConfig.PopupAliveDuration)
        .Append(canvasGroup.DOFade(0, _notificationConfig.PopupHideDuration))
        .OnComplete(() => GameObject.Destroy(canvasGroup.gameObject));
}
