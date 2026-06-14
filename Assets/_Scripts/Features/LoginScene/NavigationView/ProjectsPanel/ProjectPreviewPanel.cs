using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Serializable]
public class ProjectPreviewPanel : MonoBehaviour
{
    [Inject] ProjectDataSaver _projectDataSaver;

    public event Action OnBlueprintDelete;
    public event Action<bool> OnBlueprintOpen; //As space

    public event Action<string> OnBlueprintRename;

    [SerializeField] RawImage _previewImage;
    [SerializeField] TMP_Text _previewSize;
    [SerializeField] TMP_Text _previewEditDate;

    [SerializeField] TMP_InputField _renameField;
    [SerializeField] Button _renameApproveButton;

    [SerializeField] Button _deleteButton;
    [SerializeField] Button _openButton;
    [SerializeField] Button _openAsSpaceButton;

    private Texture _defaultPreview;


    private void OnEnable()
    {
        _renameApproveButton.onClick.AddListener(RenameBlueprint);

        _deleteButton.onClick.AddListener(DeleteBlueprint);
        _openButton.onClick.AddListener(OpenBlueprint);
        _openAsSpaceButton.onClick.AddListener(OpenAsSpaceBlueprint);
    }
    private void OnDisable()
    {
        _renameApproveButton.onClick.RemoveListener(RenameBlueprint);

        _deleteButton.onClick.RemoveListener(DeleteBlueprint);
        _openButton.onClick.RemoveListener(OpenBlueprint);
        _openAsSpaceButton.onClick.RemoveListener(OpenAsSpaceBlueprint);
    }

    private void Start() => _defaultPreview = _previewImage.texture;

    public void ShowBlueprintPreview(ProjectData blueprintData, UserProject project)
    {
        _projectDataSaver.LoadPreviewSprite(project,
            onComplete: texture => _previewImage.texture = texture,
            onError: _ => _previewImage.texture = _defaultPreview
        ).Forget();

        _previewSize.text = $"{blueprintData.Square} m²";
        _previewEditDate.text = $"{project.UpdatedAt}";

        _renameField.text = project.ProjectName;
    }

    private void RenameBlueprint() => OnBlueprintRename?.Invoke(_renameField.text);
    private void DeleteBlueprint() => OnBlueprintDelete?.Invoke();
    private void OpenBlueprint() => OnBlueprintOpen?.Invoke(false);
    private void OpenAsSpaceBlueprint() => OnBlueprintOpen?.Invoke(true);
}
