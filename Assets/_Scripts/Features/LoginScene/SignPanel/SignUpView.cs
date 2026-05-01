using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpView : MonoBehaviour
{
    public event Action<User> OnSubmit;

    [SerializeField] List<RectTransform> _phaseContainers;

    [SerializeField] Button _nextButton;
    [SerializeField] Button _backButton;
    [SerializeField] Button _sumbinButton;

    [SerializeField] TMP_InputField _login;
    [SerializeField] TMP_InputField _pass;
    [SerializeField] TMP_InputField _confirmPass;
    [SerializeField] TMP_InputField _firstName;
    [SerializeField] TMP_InputField _lastName;
    [SerializeField] TMP_InputField _phone;
    [SerializeField] TMP_InputField _email;

    private int _currentPhase = 0;


    private void OnEnable()
    {
        _nextButton.onClick.AddListener(HandleNext);
        _backButton.onClick.AddListener(HandleBack);

        _sumbinButton.onClick.AddListener(HandleSumbit);
    }
    private void OnDisable()
    {
        _nextButton.onClick.RemoveListener(HandleNext);
        _backButton.onClick.RemoveListener(HandleBack);

        _sumbinButton.onClick.RemoveListener(HandleSumbit);
    }

    public void ShowPhase(int index)
    {
        for (int i = 0; i < _phaseContainers.Count; i++)
            _phaseContainers[i].gameObject.SetActive(i == index);

        _currentPhase = index;
    }

    private void HandleNext()
    {
        if (_currentPhase == _phaseContainers.Count - 1)
        {
            _nextButton.interactable = false;
            return;
        }
        _nextButton.interactable = true;
        ShowPhase(_currentPhase + 1);
    }
    private void HandleBack()
    {
        if (_currentPhase == 0)
        {
            _backButton.interactable = false;
            return;
        }

        _backButton.interactable = true;
        ShowPhase(_currentPhase - 1);
    }

    private void HandleSumbit()
    {
        User user = new()
        {
            FirstName = _firstName.text,
            LastName = _lastName.text,

            PhoneNumber = _phone.text,
            Email = _email.text,


            Login = _login.text,
            Password = _pass.text,
        };

        OnSubmit?.Invoke(user);
    }

}
