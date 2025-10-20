using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintManagerView : MonoBehaviour
{
    [SerializeField] BlueprintManager _blueprintManager;

    [SerializeField] TMP_Text _pointsCount;
    [SerializeField] TMP_Text _linesCount;

    [SerializeField] Button _resetPointsButton;

    [SerializeField] List<Vector2> _defaultPointsPosition = new();


    private void Update()
    {
        _pointsCount.text = _blueprintManager.PointsController.Points.Count.ToString();
        _linesCount.text = _blueprintManager.LinesController.Lines.Count.ToString();
    }
}
