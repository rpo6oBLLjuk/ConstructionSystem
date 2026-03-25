using UnityEngine;
using Zenject;

public class ConstructionBuilder
{
    [Inject] InputSystem _inputSystem;
    [Inject] ConstructionSystem _constructionSystem;
    [Inject] ConstructionSystemConfig _constructionSystemConfig;


    public (bool hasCollision, Collider[] colliders) Update(Vector3 hitPoint, Quaternion rotation)
    {
        Vector3 position = hitPoint;
        Vector3 size = _constructionSystem.CurrentConstructionData.BoxCollider.size;

        //prevent textures from being clamped
        if (Physics.CheckBox(position + (Vector3.up * size.y / 2), size / 2 * 0.99999f, rotation, _constructionSystemConfig.BuilderLayerMask, QueryTriggerInteraction.Ignore))
            return (true, Physics.OverlapBox(position + (Vector3.up * size.y / 2), size / 2 * 0.99999f, rotation, _constructionSystemConfig.BuilderLayerMask, QueryTriggerInteraction.Ignore));

        if (Input.GetMouseButtonDown(0) && _inputSystem.InputActionAsset.Player.enabled)
            InstantiateConstruction(position, rotation);

        return (false, null);
    }
    private void InstantiateConstruction(Vector3 position, Quaternion rotation) => _constructionSystem.InstantiateCurrentConstruction(position: position, rotation: rotation);
}
