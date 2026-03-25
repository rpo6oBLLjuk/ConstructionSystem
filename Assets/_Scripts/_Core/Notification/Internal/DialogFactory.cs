using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DialogFactory
{
    [SerializeField] NotificationConfig _notificationConfig;
    [SerializeField] GameObject _dialogWindow;
    [SerializeField] Button _defaultButton;

    CanvasGroup _dialogCanvasGroup;
    List<Button> _currentDialogButtons = new();
    Tween _previousTween;

    public void Start()
    {
        _dialogCanvasGroup = _dialogWindow.GetComponent<CanvasGroup>();
        _dialogWindow.SetActive(false);

        _defaultButton.gameObject.SetActive(false);
    }

    public void ShowDialog(string message, string title, List<(string, Action)> buttons)
    {
        _previousTween.Kill();
        RemoveAllButtons();

        _dialogWindow.SetActive(true);

        ApplyText(title, "TitleText", _dialogWindow.transform.GetChild(0));
        ApplyText(message, "MessageText", _dialogWindow.transform.GetChild(0));

        ApplyButtons(buttons);

        AnimateDialog(true);
    }

    private void ApplyText(string text, string objectName, Transform transform)
    {
        if (!transform.Find(objectName).TryGetComponent(out TMP_Text pulledText))
            DebugWrapper.LogError(this, "Can't find 'TitleText' in _defaultPopup children");
        else
            pulledText.text = text;
    }
    private void ApplyButtons(List<(string, Action)> buttons)
    {
        Transform parent = _defaultButton.transform.parent;
        foreach (var (name, action) in buttons)
        {
            Button btn = GameObject.Instantiate(_defaultButton, parent).GetComponent<Button>();
            btn.gameObject.SetActive(true);

            btn.transform.GetComponentInChildren<TMP_Text>().text = name;
            var capturedAction = action; //ѕредотвращение замыкани€, т.к. реф в action будет посто€нно перезаписыватьс€ на следующий по списку

            btn.onClick.AddListener(() => capturedAction?.Invoke());
            btn.onClick.AddListener(() => CloseDialog(name));

            _currentDialogButtons.Add(btn);
        }
    }
    private Tween AnimateDialog(bool show) => DOTween.Sequence(_dialogCanvasGroup)
        .Append(_dialogCanvasGroup.DOFade(show ? 1 : 0, show ? _notificationConfig.DialogShowDuration : _notificationConfig.DialogHideDuration));

    private void CloseDialog(string btnName = null)
    {
        if (btnName != null)
            DebugWrapper.InactiveLog(this, $"Dialog answer: {btnName}");

        _currentDialogButtons.ForEach(btn => btn.onClick.RemoveAllListeners());

        _previousTween = AnimateDialog(false)
            .OnComplete(() =>
            {
                RemoveAllButtons();

                _dialogWindow.SetActive(false);
            });
    }

    private void RemoveAllButtons()
    {
        _currentDialogButtons.ForEach(btn => GameObject.Destroy(btn.gameObject));
        _currentDialogButtons.Clear();
    }
}
