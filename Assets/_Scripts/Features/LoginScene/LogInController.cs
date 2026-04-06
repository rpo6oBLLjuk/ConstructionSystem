using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogInController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TMP_Text _titleText;
    [SerializeField] TMP_Text _swapText;
    [SerializeField] TMP_Text _signText;

    [Space]
    [SerializeField] Button _signButton;
    [SerializeField] Button _swapButton;

    [Space]
    [SerializeField] TMP_InputField _loginInputField;
    [SerializeField] TMP_InputField _passwordInputField;
    [SerializeField] TMP_InputField _secondPasswordInputField;

    [Header("Texts")]
    [SerializeField] AuthModeData _signInData;
    [SerializeField] AuthModeData _signUpData;

    [Header("Animation")]
    [SerializeField] float _duration = 0.5f;
    [SerializeField] float _angle = 90;

    [Space]
    [SerializeField] Ease _forwardEaseType;
    [SerializeField] Ease _backwardEaseType;

    [Space]
    [SerializeField] bool _isSignInMode = true;


    private void OnEnable()
    {
        _swapButton.onClick.AddListener(InvertMode);
        //_signButton.onClick.AddListener(InvertMode);
    }

    private void OnDisable()
    {
        _swapButton.onClick.RemoveListener(InvertMode);
        //_signButton.onClick.RemoveListener(InvertMode);
    }

    private void Start()
    {
        InitSecondPasswordInputField();

        SwapMode(_isSignInMode, false);
    }

    private void InvertMode() => SwapMode(!_isSignInMode);
    private void SwapMode(bool isSignInMode, bool animate = true)
    {
        _isSignInMode = isSignInMode;

        if (!animate)
        {
            UpdateTexts(isSignInMode);
            return;
        }

        DOTween.Sequence()
            .AppendCallback(() => AnimateUI())
            .AppendInterval(_duration / 2)
            .AppendCallback(() => UpdateTexts(isSignInMode));
    }

    private void UpdateTexts(bool isSignInMode)
    {
        _titleText.text = isSignInMode ? _signInData.Title : _signUpData.Title;
        _swapText.text = isSignInMode ? _signInData.SwapButton : _signUpData.SwapButton;
        _signText.text = isSignInMode ? _signInData.SignButton : _signUpData.SignButton;

        _loginInputField.text = string.Empty;
        _passwordInputField.text = string.Empty;
        _secondPasswordInputField.text = string.Empty;

        _secondPasswordInputField.gameObject.SetActive(!isSignInMode);
    }

    private void AnimateUI()
    {
        SwapAnim(_titleText.transform);

        SwapAnim(_loginInputField.transform);
        SwapAnim(_passwordInputField.transform);
        SwapAnim(_secondPasswordInputField.transform);
        SwapAnim(_signButton.transform);

        SwapAnim(_swapText.transform);
    }
    private Tween SwapAnim(Transform graphic) =>
        graphic.DORotate(Vector3.up * _angle, _duration / 2)
            .SetEase(_forwardEaseType)
            .OnComplete(() => graphic.DORotate(Vector3.zero, _duration / 2)
                .SetEase(_backwardEaseType)
            );

    private void InitSecondPasswordInputField()
    {
        _secondPasswordInputField = Instantiate(_passwordInputField, _passwordInputField.transform.parent);
        _secondPasswordInputField.transform.SetSiblingIndex(2);
        _secondPasswordInputField.enabled = false;
    }
}

[System.Serializable]
public class AuthModeData
{
    [field: SerializeField] public string Title { get; private set; } = "SIGN IN";
    [field: SerializeField] public string SignButton { get; private set; } = "Sign In";
    [field: SerializeField] public string SwapButton { get; private set; } = "Don't Have an account? Get started!";
}