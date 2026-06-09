using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Serializable]
public class ProjectPreviewPanel : MonoBehaviour
{
    [Inject] NotificationService _notificationService;

    public event Action OnBlueprintDelete;
    public event Action OnBlueprintOpen;

    public event Action<string> OnBlueprintRename;

    [SerializeField] Image _previewImage;
    [SerializeField] TMP_Text _previewSize;
    [SerializeField] TMP_Text _previewEditDate;

    [SerializeField] TMP_InputField _renameField;
    [SerializeField] Button _renameApproveButton;

    [SerializeField] Button _deleteButton;
    [SerializeField] Button _openButton;

    private UserProject _currentProject;

    private void OnEnable()
    {
        _renameApproveButton.onClick.AddListener(RenameBlueprint);

        _deleteButton.onClick.AddListener(DeleteBlueprint);
        _openButton.onClick.AddListener(OpenBlueprint);
    }
    private void OnDisable()
    {
        _renameApproveButton.onClick.RemoveListener(RenameBlueprint);

        _deleteButton.onClick.RemoveListener(DeleteBlueprint);
        _openButton.onClick.RemoveListener(OpenBlueprint);
    }

    public void ShowBlueprintPreview(ProjectData blueprintData, UserProject project)
    {
        _currentProject = project;

        if (blueprintData != null)
        {
            _previewSize.text = $"Size\n{blueprintData.square} m²";
            _previewEditDate.text = $"Last edit\n{project.UpdatedAt}";
        }

        _renameField.text = project.ProjectName;
    }

    private void RenameBlueprint() => OnBlueprintRename?.Invoke(_renameField.text);
    private void DeleteBlueprint() => OnBlueprintDelete?.Invoke();
    private void OpenBlueprint() => OnBlueprintOpen?.Invoke();
}
