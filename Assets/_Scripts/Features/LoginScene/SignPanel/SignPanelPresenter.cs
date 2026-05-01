using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SignPanelPresenter : MonoBehaviour
{
    [Inject] UserModule _userModule;
    [Inject] NotificationService _notificationService;

    [SerializeField] SignPanelView _signPanelView;
    //[SerializeField] 2ndView; //Camera

    [SerializeField] Vector2 minmaxLoginLength = new Vector2(5, 20);
    [SerializeField] Vector2 minmaxPasswordLength = new Vector2(4, 20);


    private void OnEnable()
    {
        _signPanelView.OnSignInRequested += HandleSignInRequest;
        _signPanelView.OnSignUpRequested += HandleSignUpRequest;
    }

    private void OnDisable()
    {
        _signPanelView.OnSignInRequested -= HandleSignInRequest;
        _signPanelView.OnSignUpRequested -= HandleSignUpRequest;
    }

    private void HandleSignInRequest(string login, string password)
    {
        if(!ValidateLoginLength(login))
            return;

        if(!ValidatePasswordLength(password))
            return;

        _userModule.LogIn(
            login,
            password,
            (user) => _notificationService.ShowDialog($"Hello, {user.FirstName} {user.LastName}", "Welcome", new List<(string, Action)>
            {
                ("Ok", () => {})
            }),
            (error) => _notificationService.ShowDialog($"{error}", "Sign in error", new List<(string, Action)>
            {
                ("Ok", () => { })
            })
            ).Forget();

        //2ndView.Show
    }

    private void HandleSignUpRequest(string login, string password, string password2)
    {
        if (password != password2)
        {
            _notificationService.ShowPopup("Passwords not equal", "Sign up error", notificationType: NotificationType.Error);
            return;
        }

        if (!ValidateLoginLength(login))
            return;

        if (!ValidatePasswordLength(password))
            return;

        _userModule.CreateUser(login, password).Forget();
        //2ndView.Show
    }

    private bool ValidateLoginLength(string login)
    {
        if(!HasValidLength(login, minmaxLoginLength))
        {
            _notificationService.ShowPopup($"Login length must be from {minmaxLoginLength.x} to {minmaxLoginLength.y} characters", "Invalid login Length", NotificationType.Warning);
            return false;
        }
        return true;
    }

    private bool ValidatePasswordLength(string password)
    {
        if (!HasValidLength(password, minmaxPasswordLength))
        {
            _notificationService.ShowPopup($"Password length must be from {minmaxPasswordLength.x} to {minmaxPasswordLength.y} characters", "Invalid password Length", NotificationType.Warning);
            return false;
        }
        return true;
    }

    private bool HasValidLength(string text, Vector2 minmax) => text.Length >= minmax.x && text.Length <= minmax.y;

}
