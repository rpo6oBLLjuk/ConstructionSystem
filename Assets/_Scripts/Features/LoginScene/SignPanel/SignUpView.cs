using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpView : MonoBehaviour
{
    public event Action<User> OnSubmit;

    [Header("Control buttons")]
    [SerializeField] Button _nextButton;
    [SerializeField] Button _backButton;
    [SerializeField] Button _sumbitButton;

    [Header("Phase Slider view")]
    [SerializeField] Slider _phaseSlider;
    [SerializeField] TMP_Text _phaseText;

    [Header("Phases")]
    [SerializeField] UserDataPhase _userData;
    [SerializeField] ContactsDataPhase _contactsData;
    [SerializeField] SignUpDataPhase _signUpData;

    private List<SignUpPhase> _phases;
    private int _currentPhase = 0;


    private void Awake()
    {
        _phases = new List<SignUpPhase> { _userData, _contactsData, _signUpData };
        ShowPhase(0);
    }

    private void Update() => UpdateButtons();

    private void OnEnable()
    {
        _nextButton.onClick.AddListener(HandleNext);
        _backButton.onClick.AddListener(HandleBack);

        _sumbitButton.onClick.AddListener(HandleSumbit);
    }
    private void OnDisable()
    {
        _nextButton.onClick.RemoveListener(HandleNext);
        _backButton.onClick.RemoveListener(HandleBack);

        _sumbitButton.onClick.RemoveListener(HandleSumbit);
    }

    public void ShowPhase(int index)
    {
        for (int i = 0; i < _phases.Count; i++)
            _phases[i].Container.SetActive(i == index);

        _currentPhase = index;
        UpdatePhaseUI(index);
    }

    private void HandleNext()
    {
        ShowPhase(_currentPhase + 1);
        UpdateButtons();
    }
    private void HandleBack()
    {
        if (_currentPhase == 0)
            return;

        ShowPhase(_currentPhase - 1);
        UpdateButtons();
    }

    private void HandleSumbit()
    {
        User user = new()
        {
            FirstName = _userData.FirstName.text,
            LastName = _userData.LastName.text,

            PhoneNumber = _contactsData.Phone.text,
            Email = _contactsData.Email.text,


            Login = _signUpData.Login.text,
            Password = _signUpData.Pass.text,
        };

        OnSubmit?.Invoke(user);
    }

    private void UpdateButtons()
    {
        bool isFirst = _currentPhase == 0;
        bool isLast = _currentPhase == _phases.Count - 1;

        _backButton.interactable = !isFirst;
        _nextButton.interactable = !isLast && _phases[_currentPhase].IsValid();
        //_sumbinButton.gameObject.SetActive(isLast);
    }

    private void UpdatePhaseUI(int index)
    {
        _phaseText.text = $"Stage {index + 1}: {_phases[index].PhaseName}";
        _phaseSlider.DOValue(index, 0.25f);
    }
}
