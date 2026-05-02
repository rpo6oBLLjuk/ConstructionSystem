using System;
using UnityEngine;
using UnityEngine.UI;

public class SignInView : MonoBehaviour
{
    public event Action<string, string> OnSubmit;

    [SerializeField] InputFieldValidator _loginField;
    [SerializeField] InputFieldValidator _passwordField;
    [SerializeField] Button _submitButton;


    private void OnEnable() => _submitButton.onClick.AddListener(() => OnSubmit?.Invoke(_loginField.text, _passwordField.text));
    private void OnDisable() => _submitButton.onClick.RemoveAllListeners();

    public void Clear() => _loginField.InputField.text = _passwordField.InputField.text = "";
}
