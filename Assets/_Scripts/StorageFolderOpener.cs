using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StorageFolderOpener : MonoBehaviour
{
    [SerializeField] Button _button;


    private void Awake() => _button = GetComponent<Button>();

    private void OnEnable() => _button.onClick.AddListener(OpenPersistentDataFolder);
    private void OnDisable() => _button.onClick.RemoveListener(OpenPersistentDataFolder);

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
