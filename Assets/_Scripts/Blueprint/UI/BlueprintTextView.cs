using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BlueprintTextView : MonoBehaviour
{
    [Inject] BlueprintManager _blueprintManager;
    GameObjectViewController _blueprintTextsController = new();

    [Header("Text")]
    [SerializeField] ButtonSpriteSwapper textVisibilityButton;
    BlueprintLineHandler[] _blueprintLineHandlers;
    List<TMP_Text> _lineTexts;

    [SerializeField] Vector2 autosizeMixMax = new(10, 30);
    [SerializeField] AnimationCurve autosizeMultiplier;

    [SerializeField, Space] float _textFadeDuration = 0.25f;

    [Header("Points")]
    [SerializeField] ButtonSpriteSwapper pointsVisibilityButton;

    private float defaultpixelsPerUnitMultiplier;


    private void Start()
    {
        ActualizeLinesCount();
        defaultpixelsPerUnitMultiplier = _blueprintLineHandlers[0].GetComponentInChildren<TMP_Text>(true).GetComponentInParent<Image>(true).pixelsPerUnitMultiplier;
    }

    private void LateUpdate()
    {
        if (Input.mouseScrollDelta.y != 0)
            _blueprintManager.SetBlueprintScaleFactor(_blueprintManager.BlueprintScaleFactor + Input.mouseScrollDelta.y);

        //return;

        ActualizeLinesCount();
        AutoScaleTexts();
        //AutoRotateTexts();
        SetLinesSizeTexts();
    }

    private void ActualizeLinesCount()
    {
        BlueprintLineHandler[] cach = _blueprintManager.LinesController.Lines.ToArray();
        if (_blueprintLineHandlers != cach)
        {
            _blueprintLineHandlers = cach;
            _lineTexts = _blueprintLineHandlers.ToList().Select(blh => blh.GetComponentInChildren<TMP_Text>()).ToList();

            UpdateTextVisibility();
        }
    }
    private void AutoScaleTexts()
    {
        //return;
        for (int i = 0; i < _blueprintLineHandlers.Length; i++)
        {
            float width = _blueprintLineHandlers[i].SelfImage.rectTransform.sizeDelta.x;
            float fontSize = Mathf.Clamp(autosizeMixMax.x + (autosizeMixMax.y - autosizeMixMax.x) * autosizeMultiplier.Evaluate(width), autosizeMixMax.x, autosizeMixMax.y);

            fontSize /= _blueprintManager.BlueprintScaleFactor;
            _lineTexts[i].fontSize = fontSize;

            _lineTexts[i].GetComponentInParent<Image>().pixelsPerUnitMultiplier = defaultpixelsPerUnitMultiplier * _blueprintManager.BlueprintScaleFactor;
            //RectTransform parent = (_lineTexts[i].rectTransform.parent as RectTransform);
            //parent.sizeDelta = new Vector2(fontSize * 3, fontSize);
            //parent.localScale = new Vector2(_blueprintManager.BlueprintScaleFactor,_blueprintManager.BlueprintScaleFactor);
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
    private void SetLinesSizeTexts()
    {
        for (int i = 0; i < _blueprintLineHandlers.Length; i++)
        {
            _lineTexts[i].text = ((_blueprintLineHandlers[i].SelfImage.rectTransform.sizeDelta.x - _blueprintLineHandlers[i].SelfImage.rectTransform.sizeDelta.y) / 100).ToString("F3");
        }
    }

    private void UpdateTextVisibility()
    {
        //DebugWrapper.InactiveLog(this, "Need create Action, when visible layers changed");
        return;

        foreach (var tmp_text in _lineTexts)
        {
            tmp_text.GetComponentInParent<CanvasGroup>().DOFade(true ? 1 : 0, _textFadeDuration);
        }
    }
}
