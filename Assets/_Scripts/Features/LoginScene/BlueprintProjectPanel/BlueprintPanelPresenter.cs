using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BlueprintPanelPresenter : MonoBehaviour
{
    [Inject] BlueprintDataSaver _blueprintDataSaver;
    [Inject] NotificationService _notificationService;
    [Inject] SceneTransitionController _sceneTransitionController;
    [Inject] ActiveBlueprintService _activeBlueprintService;

    [SerializeField] CameraSplineController _cameraSplineController;

    [SerializeField] BlueprintGridView _blueprintGridView;
    [SerializeField] BlueprintPreviewPanel _blueprintPreviewPanel;

    [SerializeField] RectTransform _blueprintSelectionContainer;
    private BlueprintData _previousSelectedBlueprint;


    private void OnEnable()
    {
        _cameraSplineController.OnForwardAnimCompleted += HandleCameraAnimCompleted;

        _blueprintGridView.OnBlueprintSelected += HandleBlueprintSelection;
        _blueprintGridView.OnNewFileSelected += HandleNewFileRequest;

        _blueprintPreviewPanel.OnBlueprintDelete += OnBlueprintDeleteRequested;
        _blueprintPreviewPanel.OnBlueprintOpen += OnBlueprintOpenRequested;

        _blueprintPreviewPanel.OnBlueprintRename += HandleBlueprintRename;
    }
    private void OnDisable()
    {
        _cameraSplineController.OnForwardAnimCompleted -= HandleCameraAnimCompleted;

        _blueprintGridView.OnBlueprintSelected -= HandleBlueprintSelection;
        _blueprintGridView.OnNewFileSelected -= HandleNewFileRequest;

        _blueprintPreviewPanel.OnBlueprintDelete -= OnBlueprintDeleteRequested;
        _blueprintPreviewPanel.OnBlueprintOpen -= OnBlueprintOpenRequested;

        _blueprintPreviewPanel.OnBlueprintRename -= HandleBlueprintRename;
    }

    private void Awake() => SetVisibility(false);

    public void SetVisibility(bool active) => _blueprintSelectionContainer.gameObject.SetActive(active);

    private void HandleBlueprintSelection(BlueprintData blueprintData)
    {
        if (_previousSelectedBlueprint != null)
            _blueprintGridView.SetBlueprintActive(_previousSelectedBlueprint, false);

        _previousSelectedBlueprint = blueprintData;
        _blueprintGridView.SetBlueprintActive(blueprintData, true);

        _blueprintPreviewPanel.ShowBlueprintPreview(blueprintData);
    }
    private void HandleNewFileRequest()
    {
        _notificationService.ShowInputDialog("Enter blueprint name...", "Create New Blueprint", (name) =>
        {
            if (!_blueprintDataSaver.Exists(name))
                CreateNewFile(name);
            else
                _notificationService.ShowDialog($"A  file named <b>{name}</b> already exists. Do you want to replace it?", "Overwrite File", new List<(string, Action)>
                {
                    ("No", null),
                    ("Yes", () => CreateNewFile(name))
                });
        });
    }

    private void OnBlueprintOpenRequested(BlueprintData blueprintData)
    {
        _activeBlueprintService.SetActiveBlueprint(blueprintData);
        _sceneTransitionController.LoadScene(AppScene.Blueprint);
    }

    private void OnBlueprintDeleteRequested(BlueprintData blueprintData)
    {
        _blueprintDataSaver.DeleteSave(blueprintData.name);
        _blueprintGridView.RemoveBlueprintData(blueprintData);
    }

    private void HandleCameraAnimCompleted()
    {
        _blueprintGridView.UpdateBlueprintsData(_blueprintDataSaver.LoadAllBlueprints());
        SetVisibility(true);
    }

    private void HandleBlueprintRename(BlueprintData blueprintData, string newName)
    {
        _blueprintDataSaver.Rename(blueprintData.name, newName);
        _blueprintGridView.UpdateBlueprintsData(_blueprintDataSaver.LoadAllBlueprints());
    }

    private void CreateNewFile(string name)
    {
        BlueprintData blueprintData = new BlueprintData();
        _blueprintDataSaver.Save(blueprintData, name);

        _blueprintGridView.AddBlueprintData(blueprintData);
        HandleBlueprintSelection(blueprintData);
    }
}
