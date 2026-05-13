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

    [SerializeField] TMP_Text _titleText;
    [SerializeField] TMP_Text _messageText;

    [SerializeField] TMP_Text _placeholderText;
    [SerializeField] TMP_InputField _inputField;

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

        _messageText.enabled = true;
        _inputField.gameObject.SetActive(false);

        _dialogWindow.SetActive(true);

        _titleText.text = title;
        _messageText.text = message;

        ApplyButtons(buttons);
        AnimateDialog(true);
    }

    public void ShowInputDialog(string placeholder, string title, Action<string> onSubmit)
    {
        _previousTween.Kill();
        RemoveAllButtons();

        _messageText.enabled = false;
        _inputField.gameObject.SetActive(true);

        _dialogWindow.SetActive(true);

        _titleText.text = title;
        _placeholderText.text = placeholder;
        _inputField.text = string.Empty;

        ApplyButtons(new List<(string, Action)>
        {
            ("Cancel", null),
            ("Ok", () => onSubmit?.Invoke(_inputField.text))
        });
        AnimateDialog(true);
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

            btn.onClick.AddListener(() => CloseDialog(name));
            btn.onClick.AddListener(() => capturedAction?.Invoke());

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
