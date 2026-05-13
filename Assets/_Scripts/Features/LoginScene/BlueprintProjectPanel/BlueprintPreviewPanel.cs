using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Serializable]
public class BlueprintPreviewPanel : MonoBehaviour
{
    [Inject] NotificationService _notificationService;

    public event Action<BlueprintData> OnBlueprintDelete;
    public event Action<BlueprintData> OnBlueprintOpen;

    public event Action<BlueprintData, string> OnBlueprintRename;

    [SerializeField] Image _previewImage;
    [SerializeField] TMP_Text _previewSize;
    [SerializeField] TMP_Text _previewEditDate;

    [SerializeField] TMP_InputField _renameField;
    [SerializeField] Button _renameApproveButton;

    [SerializeField] Button _deleteButton;
    [SerializeField] Button _openButton;

    private BlueprintData _currentBlueprintData;


    private void OnEnable()
    {
        _renameApproveButton.onClick.AddListener(RenameBlueprint);

        _deleteButton.onClick.AddListener(DeleteBlueprint);
        _openButton.onClick.AddListener(OpenBlueprint);
    }
    private void OnDisable()
    {
        _renameApproveButton.onClick.RemoveListener(RenameBlueprint);
    }

    public void ShowBlueprintPreview(BlueprintData blueprintData)
    {
        _currentBlueprintData = blueprintData;

        _previewSize.text = $"Size\n{blueprintData.square} m²";
        _previewEditDate.text = $"Last edit\n{blueprintData.editTime}";

        _renameField.text = blueprintData.name;
    }

    private void RenameBlueprint() => OnBlueprintRename?.Invoke(_currentBlueprintData, _renameField.text);

    private void DeleteBlueprint()
    {
        _notificationService.ShowDialog($"Delete blueprint <b>{_currentBlueprintData.name}</b>?", "Confirmation of deletion", new List<(string, Action)>
        {
            ("Cancel", null),
            ("OK", () => OnBlueprintDelete?.Invoke(_currentBlueprintData))
        });
    }
    private void OpenBlueprint() => OnBlueprintOpen?.Invoke(_currentBlueprintData);
}
