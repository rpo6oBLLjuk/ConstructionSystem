using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BlueprintManagerView : MonoBehaviour
{
    [SerializeField] BlueprintManager _blueprintManager;
    [SerializeField] BlueprintLineHandler[] _blueprintLineHandlers;
    [SerializeField] List<TMP_Text> _lineTexts;

    [SerializeField] private Vector2 autosizeMixMax = new(10, 30);
    [SerializeField] private AnimationCurve autosizeMultiplier;


    private void LateUpdate()
    {
        ActualizeLinesCount();
        AutoScaleTexts();
        AutoRotateTexts();
    }

    private void ActualizeLinesCount()
    {
        BlueprintLineHandler[] cach = _blueprintManager.LinesController.Lines.ToArray();
        if (_blueprintLineHandlers != cach)
        {
            _blueprintLineHandlers = cach;
            _lineTexts = _blueprintLineHandlers.ToList().Select(blh => blh.GetComponentInChildren<TMP_Text>()).ToList();
        }
    }
    private void AutoScaleTexts()
    {
        for (int i = 0; i < _blueprintLineHandlers.Length; i++)
        {
            float width = _blueprintLineHandlers[i].SelfImage.rectTransform.sizeDelta.x;
            float fontSize = Mathf.Clamp(autosizeMixMax.x + (autosizeMixMax.y - autosizeMixMax.x) * autosizeMultiplier.Evaluate(width), autosizeMixMax.x, autosizeMixMax.y);
            _lineTexts[i].fontSize = fontSize;
        }
    }
    private void AutoRotateTexts()
    {
        for (int i = 0; i < _blueprintLineHandlers.Length; i++)
        {
            float zRotation = _blueprintLineHandlers[i].transform.rotation.eulerAngles.z;
            float multyplier = zRotation > 90.01f && zRotation < 269.99f ? -1 : 1;
            _lineTexts[i].transform.localScale = new Vector3(Mathf.Abs(_lineTexts[i].transform.localScale.x) * multyplier, Mathf.Abs(_lineTexts[i].transform.localScale.y) * multyplier, _lineTexts[i].transform.localScale.z);
        }
    }
}
