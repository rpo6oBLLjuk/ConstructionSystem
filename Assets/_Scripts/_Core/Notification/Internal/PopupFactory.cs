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

        ApplyText(title, "TitleText", popup.transform.GetChild(0));
        ApplyText(message, "MessageText", popup.transform);

        ApplyImage(popup.transform.GetChild(0), notificationType);

        ApplyBackgroundColor("Outline", popup.transform.GetChild(0), notificationType);
        ApplyBackgroundColor("Divider", popup.transform.GetChild(0), notificationType);

        if (!popup.TryGetComponent(out CanvasGroup canvasGroup))
        {
            DebugWrapper.LogError(this, "Can't find 'CanvasGroup' in _defaultPopup");
            return null;
        }

        AnimatePopup(canvasGroup, popup.GetComponentInChildren<Slider>());

        return popup.GetComponent<CanvasGroup>();
    }

    private void ApplyText(string text, string objectName, Transform transform)
    {
        if (!transform.Find(objectName).TryGetComponent(out TMP_Text pulledText))
            DebugWrapper.LogError(this, $"Can't find '{objectName}' in _defaultPopup children");
        else
            pulledText.text = text;
    }
    private void ApplyImage(Transform transform, NotificationType notificationType)
    {
        if (!transform.Find("IconImage").TryGetComponent(out Image pulledImage))
            DebugWrapper.LogError(this, "Can't find 'IconImage' in _defaultPopup children");
        else
        {
            pulledImage.sprite = notificationType switch
            {
                NotificationType.Info => _notificationConfig.InfoSprite,
                NotificationType.Warning => _notificationConfig.WarningSprite,
                NotificationType.Error => _notificationConfig.ErrorSprite,
                NotificationType.Success => _notificationConfig.SuccessSprite,
                _ => (null)
            };

            ApplyColor(pulledImage, notificationType);
        }

    }
    private void ApplyBackgroundColor(string objectName, Transform transform, NotificationType notificationType)
    {
        if (!transform.Find(objectName).TryGetComponent(out Image pulledImage))
            DebugWrapper.LogError(this, $"Can't find '{objectName}' in _defaultPopup children");
        else
            ApplyColor(pulledImage, notificationType);
    }

    private void ApplyColor(Graphic graphic, NotificationType notificationType)
    {
        graphic.color = notificationType switch
        {
            NotificationType.Info => _notificationConfig.InfoColor,
            NotificationType.Warning => _notificationConfig.WarningColor,
            NotificationType.Error => _notificationConfig.ErrorColor,
            NotificationType.Success => _notificationConfig.SuccessColor,
            _ => _notificationConfig.InfoColor,
        };
    }


    //Твин анимации уведомления
    private Tween AnimatePopup(CanvasGroup canvasGroup, Slider slider) => DOTween.Sequence(canvasGroup) // Создание последовательности с ссылкой на компонент
        .Append(canvasGroup.DOFade(1, _notificationConfig.PopupShowDuration).From(0)                    // Добавление в секвенцию анимации появления альфа-канала
            .OnComplete(() => slider.DOValue(1, _notificationConfig.PopupAliveDuration)))               // По окончании появления начинается интерполяция слайдера
        .AppendInterval(_notificationConfig.PopupAliveDuration)                                         // К секвенции добавляется ожилание существования уведомления
        .Append(canvasGroup.DOFade(0, _notificationConfig.PopupHideDuration))                           // Анимация исчезновения альфа-канала
        .OnComplete(() => GameObject.Destroy(canvasGroup.gameObject));                                  // По завершении анимации удаляется объект
}
