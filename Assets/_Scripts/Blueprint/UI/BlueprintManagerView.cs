using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintManagerView : MonoBehaviour
{
    [SerializeField] BlueprintManager _blueprintManager;

    [SerializeField] TMP_Text _pointsCount;
    [SerializeField] TMP_Text _linesCount;

    [SerializeField] Button _resetPointsButton;


    private void OnEnable()
    {
        _resetPointsButton.onClick.AddListener(ResetBlueprintButtonClick);
    }

    private void OnDisable()
    {
        _resetPointsButton.onClick.RemoveListener(ResetBlueprintButtonClick);
    }

    private void Update()
    {
        _pointsCount.text = _blueprintManager.PointsController.Points.Count.ToString();
        _linesCount.text = _blueprintManager.LinesController.Lines.Count.ToString();
    }

    private void ResetBlueprintButtonClick() => _blueprintManager.ResetBlueprint();
}
