using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintTextsController : BlueprintView<CanvasGroup>
{
    protected override IEnumerable<CanvasGroup> ViewList => BlueprintLineHandlers.Select(lineHandler => lineHandler.Text.GetComponentInParent<CanvasGroup>());
    protected override BlueprintViewLayers ViewLayer => BlueprintViewLayers.Text;

    private List<BlueprintLineHandler> BlueprintLineHandlers => _blueprintManager.LinesController.Lines;

    private float _defaultpixelsPerUnitMultiplier;
    private Vector2 _defaultEffectDistance;
    private Vector4 _defaultMargin;


    protected override void OnEnable()
    {
        base.OnEnable();
        _blueprintManager.OnPointAdded += SetTextsVisible;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        _blueprintManager.OnPointAdded -= SetTextsVisible;
    }

    private void Start()
    {
        _defaultpixelsPerUnitMultiplier = BlueprintLineHandlers[0].Text.GetComponentInParent<Image>(true).pixelsPerUnitMultiplier;
        _defaultEffectDistance = BlueprintLineHandlers[0].GetComponentInChildren<Outline>(true).effectDistance;
        _defaultMargin = BlueprintLineHandlers[0].Text.margin;
    }

    private void LateUpdate()
    {
        AutoScaleTexts();
        SetLinesSizeTexts();
    }

    private void AutoScaleTexts()
    {
        for (int i = 0; i < BlueprintLineHandlers.Count; i++)
        {
            float width = BlueprintLineHandlers[i].SelfImage.rectTransform.sizeDelta.x - BlueprintLineHandlers[i].SelfImage.rectTransform.sizeDelta.y;
            float fontSize = _blueprintVisualConfig.TextData.FontSize;

            //Clip text
            if (width / 100 < _blueprintVisualConfig.TextData.ClippingCurve.Evaluate(
                    (_blueprintManager.ScaleFactor - _blueprintVisualConfig.BlueprintScaleFactorMinMax.x) / _blueprintVisualConfig.BlueprintScaleFactorMinMax.y))
                fontSize = 0;

            if(fontSize == 0)
            {
                BlueprintLineHandlers[i].Text.GetComponentInParent<CanvasGroup>().alpha = 0;
                return;
            }
            else
                BlueprintLineHandlers[i].Text.GetComponentInParent<CanvasGroup>().alpha = 1;

            BlueprintLineHandlers[i].Text.fontSize = fontSize / _blueprintManager.ScaleFactor;

            Image img = BlueprintLineHandlers[i].Text.GetComponentInParent<Image>();
            img.pixelsPerUnitMultiplier = _defaultpixelsPerUnitMultiplier * _blueprintManager.ScaleFactor;
            img.GetComponent<Outline>().effectDistance = _defaultEffectDistance / _blueprintManager.ScaleFactor;
            
            BlueprintLineHandlers[i].Text.margin = _defaultMargin / _blueprintManager.ScaleFactor;

            Debug.Log(fontSize);
            //RectTransform parent = (_lineTexts[i].rectTransform.parent as RectTransform);
            //parent.sizeDelta = new Vector2(fontSize * 3, fontSize);
            //parent.localScale = new Vector2(_blueprintManager.BlueprintScaleFactor,_blueprintManager.BlueprintScaleFactor);
        }
    }
    private void SetLinesSizeTexts() => BlueprintLineHandlers.ForEach(handler => handler.Text.text = ((handler.SelfImage.rectTransform.sizeDelta.x - handler.SelfImage.rectTransform.sizeDelta.y) / 100).ToString("F3"));

    private void SetTextsVisible(int index, Vector2 _) => SetCanvasGroupVisible(BlueprintLineHandlers[index].Text.GetComponentInParent<CanvasGroup>());
}
