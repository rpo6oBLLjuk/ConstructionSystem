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
    public event Action OnBlueprintOpen;

    public event Action<string> OnBlueprintRename;

    [SerializeField] Image _previewImage;
    [SerializeField] TMP_Text _previewSize;
    [SerializeField] TMP_Text _previewEditDate;

    [SerializeField] TMP_InputField _renameField;
    [SerializeField] Button _renameApproveButton;

    [SerializeField] Button _deleteButton;
    [SerializeField] Button _openButton;

    private Sprite _defaultPreview;


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

    private void Start() => _defaultPreview = _previewImage.sprite;

    public void ShowBlueprintPreview(ProjectData blueprintData, UserProject project)
    {
        _projectDataSaver.LoadPreviewSprite(project,
            onComplete: sprite => _previewImage.sprite = sprite,
            onError: _ => _previewImage.sprite = _defaultPreview
        ).Forget();

        _previewSize.text = $"{blueprintData.Square} m²";
        _previewEditDate.text = $"{project.UpdatedAt}";

        _renameField.text = project.ProjectName;
    }

    private void RenameBlueprint() => OnBlueprintRename?.Invoke(_renameField.text);
    private void DeleteBlueprint() => OnBlueprintDelete?.Invoke();
    private void OpenBlueprint() => OnBlueprintOpen?.Invoke();
}
