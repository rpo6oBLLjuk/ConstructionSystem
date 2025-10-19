using ColliderVisualizerNamespace;
using UnityEngine;
using Zenject;

public class ConstructionHologramVisualizer : MonoBehaviour
{
    [Inject] ConstructionSystem _constructionSystem;

    ConstructionHandler _hologramHandler;
    ColliderVisualizer _hologramVisualizer;

    [SerializeField] Color _hasColliderSolidColor = Color.red;
    [SerializeField] Color _hasntColliderSolidColor = Color.green;

    [SerializeField] Color _hasColliderWireColor = Color.red;
    [SerializeField] Color _hasntColliderWireColor = Color.green;


    private void OnEnable()
    {
        _constructionSystem.OnNewConstructionSelected += NewConstructionSelected;
        _constructionSystem.OnTargetPlacementDataUpdated += ConstructionDataPlacementChanged;
    }

    private void OnDisable()
    {
        _constructionSystem.OnNewConstructionSelected -= NewConstructionSelected;
        _constructionSystem.OnTargetPlacementDataUpdated -= ConstructionDataPlacementChanged;
    }

    private void NewConstructionSelected(SelectedConstructionData _)
    {
        _hologramHandler = _constructionSystem.InstantiateCurrentConstruction();
        _hologramVisualizer = _hologramHandler.GetComponent<ColliderVisualizer>();

        //Set hologram colliders as trigger (QueryTriggerInteraction.Ignore)
        _hologramHandler.BoxCollider.isTrigger = true;
        _hologramHandler.MeshCollider.convex = true;
        _hologramHandler.MeshCollider.isTrigger = true;

        //Get or Add collider visualizer (for hologram construction only)
        _hologramVisualizer = _hologramHandler.transform.GetOrAddComponent<ColliderVisualizer>();
        _hologramVisualizer.enabled = true;
    }

    private void ConstructionDataPlacementChanged(ConstructionPlacementData data)
    {
        _hologramVisualizer.SolidColor = data.HasCollisions ? _hasColliderSolidColor : _hasntColliderSolidColor;
        _hologramVisualizer.WireColor = data.HasCollisions? _hasColliderWireColor : _hasntColliderWireColor;

        //Ранее было hologram.transform.position, соответственно position - offset. => теперь голограмму надо ставить в position + offset
        Vector3 offset = _constructionSystem.PlacementData.TargetRotation * new Vector3(_constructionSystem.CurrentConstructionData.BoxCollider.center.x, 0.001f, _constructionSystem.CurrentConstructionData.BoxCollider.center.z);
        _hologramHandler.transform.position = data.TargetPosition - offset + Vector3.up * 0.0001f;
        _hologramHandler.transform.rotation = data.TargetRotation;
    }
}
