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
    [SerializeField] Ease _panelEaseType;

    private bool _isSignIn = false;


    private void OnEnable()
    {
        _signInView.OnSubmit += HandleSignIn;
        _signUpView.OnSubmit += HandleSignUp;

        _signInView.OnSwitch += SwitchToSignUp;
        _signUpView.OnSwitch += SwitchToSignIn;
    }
    private void OnDisable()
    {
        _signInView.OnSubmit -= HandleSignIn;
        _signUpView.OnSubmit -= HandleSignUp;

        _signInView.OnSwitch -= SwitchToSignUp;
        _signUpView.OnSwitch -= SwitchToSignIn;
    }

    private void Awake() => SetSignIn(true, 0);

    private async void HandleSignIn(string login, string password)
    {
        DebugWrapper.Log(this, $"Attempting login for: {login}");

        await _userModule.LogIn(login, password,
            onLoginFailed: (error) =>
            {
                _notificationService.ShowPopup(error, "Login Failed", NotificationType.Error);
            },
            onLoginSuccess: (user) =>
            {
                _notificationService.ShowPopup($"Welcome back, {user.FirstName}!", "Success", NotificationType.Info);

                Transition();
            }
        );
    }
    private async void HandleSignUp(User newUser)
    {
        DebugWrapper.InactiveLog(this, $"Attempting registration for: {newUser.Login}");

        bool success = await _userModule.CreateUser(newUser, (error) => _notificationService.ShowPopup(error, "Registration fail", NotificationType.Error));

        if (success)
        {
            _notificationService.ShowPopup("Account created successfully!", "Success", NotificationType.Info);

            Transition();
        }
    }

    private void SwitchToSignUp() => SetSignIn(false, _duration);
    private void SwitchToSignIn() => SetSignIn(true, _duration);

    private void SetSignIn(bool value, float duration)
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
        AnimateUI();
        AnimateCamera();
    }

    private void AnimateUI()
    {
        _signPanelContainer.DOAnchorPosY(-1000, 0.5f)
            .SetEase(_panelEaseType);
        //Ќадо оба окна анимировать так, чтобы они баунсили вверх, и уходили вниз экрана. ¬озможно стоит учесть вращение камеры, и переводить Canvas в 3d, как бы пролета€ меню.
    }

    private void AnimateCamera() => _cameraSplineController.AnimateCameraSpline(true);

}
