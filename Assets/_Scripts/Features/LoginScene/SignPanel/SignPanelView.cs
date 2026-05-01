using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignPanelView : MonoBehaviour
{
    public event Action<string, string> OnSignInRequested;
    public event Action<string, string, string> OnSignUpRequested;

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

    TMP_InputField _firstNameInputField;
    TMP_InputField _lastNameInputField;
    TMP_InputField _phoneNumberInputField;
    TMP_InputField _emailInputField;
    TMP_InputField _secondPasswordInputField;

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
        _signButton.onClick.AddListener(SignButtonClick);
    }
    private void OnDisable()
    {
        _swapButton.onClick.RemoveListener(InvertMode);
        _signButton.onClick.RemoveListener(SignButtonClick);
    }

    private void Start()
    {
        InitCopyInputField(_loginInputField, ref _firstNameInputField, 0);
        InitCopyInputField(_loginInputField, ref _lastNameInputField, 1);
        InitCopyInputField(_loginInputField, ref _phoneNumberInputField, 2);
        InitCopyInputField(_loginInputField, ref _emailInputField, 3);

        InitCopyInputField(_passwordInputField, ref _secondPasswordInputField, 6);

        SwapMode(_isSignInMode, false);
    }

    private void SignButtonClick()
    {
        DebugWrapper.InactiveLog(this, $"Sign Request. IsSignIn: {_isSignInMode}");

        if (_isSignInMode)
            OnSignInRequested?.Invoke(_loginInputField.text, _passwordInputField.text);
        else
            OnSignUpRequested?.Invoke(_loginInputField.text, _passwordInputField.text, _secondPasswordInputField.text);
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

        //_firstNameInputField.text = string.Empty;
        //_firstNameInputField.gameObject.SetActive(!isSignInMode);

        //_lastNameInputField.text = string.Empty;
        //_lastNameInputField.gameObject.SetActive(!isSignInMode);

        //_phoneNumberInputField.text = string.Empty;
        //_phoneNumberInputField.gameObject.SetActive(!isSignInMode);

        //_emailInputField.text = string.Empty;
        //_emailInputField.gameObject.SetActive(!isSignInMode);

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

    private void InitCopyInputField(TMP_InputField copyFrom, ref TMP_InputField copyTo, int index, bool isActive = false)
    {
        copyTo = Instantiate(copyFrom, copyFrom.transform.parent);
        copyTo.transform.SetSiblingIndex(index);
        copyTo.gameObject.SetActive(isActive);
    }
}

[System.Serializable]
public class AuthModeData
{
    [field: SerializeField] public string Title { get; private set; } = "SIGN IN";
    [field: SerializeField] public string SignButton { get; private set; } = "Sign in";
    [field: SerializeField] public string SwapButton { get; private set; } = "Don't have an account? Get started!";
}