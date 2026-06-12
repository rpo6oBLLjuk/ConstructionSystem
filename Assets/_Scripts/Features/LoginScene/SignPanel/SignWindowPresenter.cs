using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class SignWindowPresenter : MonoBehaviour
{
    [Inject] NotificationService _notificationService;
    [Inject] UserModule _userModule;

    [SerializeField] SignInView _signInView;
    [SerializeField] SignUpView _signUpView;

    [SerializeField] CameraSplineController _cameraSplineController;

    [Header("Swap view")]
    [SerializeField] RectTransform _signPanelContainer;

    [SerializeField] float _duration = 0.25f;
    [SerializeField] Ease _showEaseType;
    [SerializeField] Ease _hideEaseType;

    private bool _isSignIn = false;
    private float _defaultYPosition;

    private bool _inProgress = false;

    private void OnEnable()
    {
        _signInView.OnSubmit += HandleSignIn;
        _signUpView.OnSubmit += HandleSignUp;

        _signInView.OnSwitch += SwitchToSignUp;
        _signUpView.OnSwitch += SwitchToSignIn;

        _cameraSplineController.OnBackAnimStarted += HandleShow;

        _userModule.LoggedOut += HandleLogOut;
    }
    private void OnDisable()
    {
        _signInView.OnSubmit -= HandleSignIn;
        _signUpView.OnSubmit -= HandleSignUp;

        _signInView.OnSwitch -= SwitchToSignUp;
        _signUpView.OnSwitch -= SwitchToSignIn;

        _cameraSplineController.OnBackAnimStarted -= HandleShow;

        _userModule.LoggedOut -= HandleLogOut;
    }

    private void Awake() => SetSignInView(true, 0);
    private void Start()
    {
        _defaultYPosition = _signPanelContainer.anchoredPosition.y;

        TryAutoSignIn();
    }

    private void TryAutoSignIn()
    {
        if (_userModule.CurrentUser == null)
            return;

        this.InactiveLog($"Auto sign in for: <b>{_userModule.CurrentUser.Login}</b>");

        AnimateCamera(true);
    }

    private async void HandleSignIn(string login, string password)
    {
        if (_inProgress || _userModule.CurrentUser != null)
            return;

        _inProgress = true;

        DebugWrapper.Log(this, $"Attempting login for: {login}");

        await _userModule.LogIn(login, password,
            onLoginFailed: (error) =>
            {
                _notificationService.ShowPopup(error, "Login Failed", NotificationType.Error);
            },
            onLoginSuccess: (user) =>
            {
                _notificationService.ShowPopup($"Welcome back, <b>{user.FirstName}<b>!", "Success", NotificationType.Success);
                Transition();
            }
        );
        _inProgress = false;

    }
    private async void HandleSignUp(User newUser)
    {
        if (_inProgress || _userModule.CurrentUser != null)
            return;

        _inProgress = true;

        DebugWrapper.InactiveLog(this, $"Attempting registration for: <b>{newUser.Login}<b>");

        if(await _userModule.CreateUser(newUser, (error) => _notificationService.ShowPopup(error, "Registration fail", NotificationType.Error)))
        {
            _notificationService.ShowPopup("Account created successfully!", "Success", NotificationType.Success);
            Transition();
        }

        _inProgress = false;
    }

    private void HandleShow() => DOVirtual.DelayedCall(0.5f, () => AnimateUI(true));
    private void HandleLogOut(User user)
    {
        _inProgress = false;

        _signInView.Clear();
        _signUpView.Clear();

        SetSignInView(true, 0);
    }

    private void SwitchToSignUp() => SetSignInView(false, _duration);
    private void SwitchToSignIn() => SetSignInView(true, _duration);

    private void SetSignInView(bool value, float duration)
    {
        _isSignIn = value;

        if (_isSignIn)
        {
            _signInView.Show(duration);
            _signUpView.Hide(duration);
        }
        else
        {
            _signInView.Hide(duration);
            _signUpView.Show(duration);
        }
    }

    private void Transition()
    {
        AnimateUI(false);
        AnimateCamera();
    }
    private void AnimateUI(bool show)
    {
        _signPanelContainer.DOAnchorPosY(show ? _defaultYPosition : -1000, 0.5f)
            .SetEase(show ? _showEaseType : _hideEaseType);
        //Ќадо оба окна анимировать так, чтобы они баунсили вверх, и уходили вниз экрана. ¬озможно стоит учесть вращение камеры, и переводить Canvas в 3d, как бы пролета€ меню.
    }
    private void AnimateCamera(bool instant = false) => _cameraSplineController.AnimateCameraSpline(true, instant);
}
