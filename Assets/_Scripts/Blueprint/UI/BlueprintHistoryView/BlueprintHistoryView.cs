using UnityEngine;
using UnityEngine.UI;

public class BlueprintHistoryView : MonoBehaviour
{
    [SerializeField] BlueprintManager _blueprintManager;

    [SerializeField] Button _undoButton;
    [SerializeField] Button _redoButton;
    [SerializeField] Button _resetButton;

    private void OnEnable()
    {
        _undoButton.onClick.AddListener(_blueprintManager.HistoryController.Undo);
        _redoButton.onClick.AddListener(_blueprintManager.HistoryController.Redo);
        _resetButton.onClick.AddListener(_blueprintManager.ResetBlueprint);
    }

    private void OnDisable()
    {
        _undoButton.onClick.RemoveListener(_blueprintManager.HistoryController.Undo);
        _redoButton.onClick.RemoveListener(_blueprintManager.HistoryController.Redo);
        _resetButton.onClick.RemoveListener(_blueprintManager.ResetBlueprint);
    }
}
