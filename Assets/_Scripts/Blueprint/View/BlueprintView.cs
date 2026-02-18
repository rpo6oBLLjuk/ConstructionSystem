using UnityEngine;
using Zenject;

public abstract class BlueprintView : MonoBehaviour
{
    [Inject] protected BlueprintManager _blueprintManager;
    [SerializeField] private BlueprintViewLayers _viewLayer;

    protected virtual void OnEnable()
    {

    }

    public void OnViewLayerValueChanged(BlueprintViewLayers blueprintViewLayers) => OnLayerChanged(BlueprintViewLayersHelper.HasLayer(blueprintViewLayers, _viewLayer));
    protected void OnLayerChanged(bool isActive) { }

}
