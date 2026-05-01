using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignInView : MonoBehaviour
{
    public event Action<string, string> OnSubmit;

    [SerializeField] TMP_InputField _loginField;
    [SerializeField] TMP_InputField _passwordField;
    [SerializeField] Button _submitButton;


    private void OnEnable() => _submitButton.onClick.AddListener(() => OnSubmit?.Invoke(_loginField.text, _passwordField.text));
    private void OnDisable() => _submitButton.onClick.RemoveAllListeners();

    public void Clear() => _loginField.text = _passwordField.text = "";
}
