using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Button))]
public class StorageFolderOpener : MonoBehaviour
{
    [Inject] UserModule _userModule;

    [SerializeField] Button _button;
    [SerializeField] CanvasGroup _canvasGroup;


    private void Awake() => _button = GetComponent<Button>();

    private void OnEnable()
    {
        _button.onClick.AddListener(OpenPersistentDataFolder);
        _userModule.LoggedIn += HandleUserLogin;
    }
    private void OnDisable()
    {
        _button.onClick.RemoveListener(OpenPersistentDataFolder);
        _userModule.LoggedIn -= HandleUserLogin;
    }

    private void HandleUserLogin(User currentUser)
    {
        bool haveAccess = currentUser.RoleId == 3;

        _canvasGroup.alpha = haveAccess ? 1 : 0;
        _canvasGroup.blocksRaycasts = haveAccess ? true : false;
        _canvasGroup.interactable = haveAccess ? true : false;
    }
    private void OpenPersistentDataFolder()
    {
        string path = Application.persistentDataPath;

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });

            this.InactiveLog($"Opened folder at path: {path}");
        }
        catch (Exception ex)
        {
            this.InactiveLog($"Failed to open folder: {ex.Message}");
        }
    }
}
