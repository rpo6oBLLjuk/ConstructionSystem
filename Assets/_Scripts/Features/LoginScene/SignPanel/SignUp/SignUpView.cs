using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SignUpView : MonoBehaviour
{
    [Inject] NotificationService _notificationService;
    
    public event Action<User> OnSubmit;

    [Header("Control buttons")]
    [SerializeField] Button _nextButton;
    [SerializeField] Button _backButton;
    [SerializeField] Button _sumbitButton;

    [Header("Phase view")]
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

    private void HandleNext() => ShowPhase(_currentPhase + 1);
    private void HandleBack() => ShowPhase(_currentPhase - 1);

    private void HandleSumbit()
    {
        if (GetValidValue() != 1f)
        {
            _notificationService.ShowPopup(
                "Please fill in all required fields.",
                "Incomplete Form", 
                NotificationType.Warning);
            return;
        }

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
        _nextButton.interactable = !isLast && _phases[_currentPhase].IsValid() == 1f;

        float currentValidFields = GetValidValue();
        if (_phaseSlider.value != currentValidFields && !DOTween.IsTweening(_phaseSlider))
            _phaseSlider.DOValue(currentValidFields, 0.25f);

        //_sumbinButton.gameObject.SetActive(isLast);
    }

    private void UpdatePhaseUI(int index)
    {
        _phaseText.text = $"Stage {index + 1}: {_phases[index].PhaseName}";
    }

    private float GetValidValue()
    {
        float currentValidFields = 0;

        currentValidFields += _userData.IsValid();
        currentValidFields += _contactsData.IsValid();
        currentValidFields += _signUpData.IsValid();

        return currentValidFields / 3f;
    }
}
