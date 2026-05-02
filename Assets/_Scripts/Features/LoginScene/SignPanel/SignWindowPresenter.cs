using UnityEngine;
using Zenject;

public class SignWindowPresenter : MonoBehaviour
{
    [Inject] NotificationService _notificationService;
    [Inject] UserModule _userModule;

    [SerializeField] SignInView _signInView;
    [SerializeField] SignUpView _signUpView;

    [SerializeField] CameraSplineController _cameraSplineController;


    private void OnEnable()
    {
        _signInView.OnSubmit += HandleSignIn;
        _signUpView.OnSubmit += HandleSignUp;
    }
    private void OnDisable()
    {
        _signInView.OnSubmit -= HandleSignIn;
        _signUpView.OnSubmit -= HandleSignUp;
    }

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
                
                AnimateCamera();
            }
        );
    }

    private async void HandleSignUp(User newUser)
    {
        DebugWrapper.InactiveLog(this, $"Attempting registration for: {newUser.Login}");

        bool success = await _userModule.CreateUser(newUser);

        if (success)
        {
            _notificationService.ShowPopup("Account created successfully!", "Success", NotificationType.Info);

            AnimateCamera();
        }
        else
        {
            _notificationService.ShowPopup("Registration failed. Try again.", "Error", NotificationType.Error);
        }
    }

    private void AnimateCamera()
    {
        _cameraSplineController.AnimateCameraSpline(true);
        //Need close UI windows
    }

    private void AnimateWindow()
    {
        //Ќадо оба окна анимировать так, чтобы они баунсили вверх, и уходили вниз экрана. ¬озможно стоит учесть вращение камеры, и переводить Canvas в 3d, как бы пролета€ меню.
    }

}
