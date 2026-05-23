using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public abstract class BlueprintView<T> : MonoBehaviour
{
    protected virtual IEnumerable<T> ViewList { get => throw new NotImplementedException(); }
    protected virtual BlueprintViewLayers ViewLayer { get => throw new NotImplementedException(); }

    [Inject] protected BlueprintManager _blueprintManager;
    [Inject] protected BlueprintVisualConfig _blueprintVisualConfig;

    protected bool IsVisible => BlueprintViewLayersHelper.HasLayer(_blueprintVisualConfig.BlueprintViewLayers, ViewLayer);


    //Реализация метода выполнена крайне негибко ввиду того, что CanvasGroup не наследует MaskableGraphic, и нельзя привести их к общему наследнику, поддерживающему .DOFade (а именно Graphic)
    protected virtual void SetVisible(bool isVisible, float fadeDuration)
    {
        ViewList.ToList().ForEach(viewComponent =>
        {
            switch (viewComponent)
            {
                case MaskableGraphic maskableGraphic:
                    maskableGraphic.DOFade(isVisible ? 1 : 0, fadeDuration);
                    break;
                case CanvasGroup canvasGroup:
                    canvasGroup.alpha = isVisible ? 1 : 0;
                    canvasGroup.blocksRaycasts = isVisible;
                    Debug.Log($"After set {canvasGroup.name} alpha = {canvasGroup.alpha}", canvasGroup);
                    break;
                default:
                    DebugWrapper.LogWarning(this, "Incorrect ViewType, add or check");
                    break;
            }
        });
    }

    protected virtual void OnEnable() => _blueprintVisualConfig.ViewLayersChanged += OnViewLayerValueChanged;
    protected virtual void OnDisable() => _blueprintVisualConfig.ViewLayersChanged -= OnViewLayerValueChanged;

    private void OnViewLayerValueChanged(BlueprintViewLayers blueprintViewLayers) => OnLayerChanged(BlueprintViewLayersHelper.HasLayer(blueprintViewLayers, ViewLayer));
    protected virtual void OnLayerChanged(bool isActive) => SetVisible(isActive, 0);

    protected void SetGraphicVisible(Graphic graphic, Color color = default)
    {
        if (color == default)
            color = graphic.color;

        if (!IsVisible)
            color.a = 0;

        graphic.color = color;
    }
    protected void SetCanvasGroupVisible(CanvasGroup canvasGroup) => canvasGroup.alpha = IsVisible ? 1 : 0;
}
