using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SignInView : MonoBehaviour
{
    public event Action<string, string> OnSubmit;
    public event Action OnSwitch;

    [SerializeField] CanvasGroup _canvasGroup;

    [SerializeField] InputFieldValidator _loginField;
    [SerializeField] InputFieldValidator _passwordField;
    [SerializeField] Button _submitButton;

    [SerializeField] Button _switchButton;

    [Header("Animation")]
    [SerializeField] float _angle = 90;

    [Space]
    [SerializeField] Ease _forwardEaseType;
    [SerializeField] Ease _backwardEaseType;


    private void OnEnable()
    {
        _submitButton.onClick.AddListener(() => OnSubmit?.Invoke(_loginField.text, _passwordField.text));
        _switchButton.onClick.AddListener(HandleSwitch);
    }
    private void OnDisable()
    {
        _submitButton.onClick.RemoveAllListeners();
        _switchButton.onClick.RemoveListener(HandleSwitch);
    }

    public void Clear() => _loginField.InputField.text = _passwordField.InputField.text = "";

    public void Show(float duration) => AnimateUI(true, duration);
    public void Hide(float duration) => AnimateUI(false, duration);

    private void HandleSwitch() => OnSwitch?.Invoke();

    private void AnimateUI(bool show, float duration) => SwapAnim(_canvasGroup.transform, show, duration);

    private Tween SwapAnim(Transform graphic, bool show, float duration) =>
        graphic.DORotate(show ? Vector3.zero : Vector3.up * _angle, duration)
            .From(show ? Vector3.up * _angle : Vector3.zero)
            .SetDelay(show ? duration : 0)
            .SetEase(show ? _forwardEaseType : _backwardEaseType)
            .OnStart(() =>
            {
                if (show)
                {
                    _canvasGroup.alpha = 1;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
            })
            .OnComplete(() =>
            {
                if (!show)
                {
                    _canvasGroup.alpha = 0;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
            });
}
