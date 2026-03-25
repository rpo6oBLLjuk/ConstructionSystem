using UnityEngine;

[CreateAssetMenu(fileName = "ConstructionSystemConfigs", menuName = "Scriptable Objects/Construction/SystemConfig")]
public class ConstructionSystemConfig : ScriptableObject
{
    [field: Header("Builder config")]
    [field: SerializeField] public LayerMask BuilderLayerMask { get; private set; }

    [field: Header("Raycaster config")]
    [field: SerializeField] public LayerMask RaycasterLayerMask { get; private set; }
    [field: SerializeField] public float MaxCameraRaycastDistance { get; private set; } = 10;
    [field: SerializeField] public float MaxSnapRaycastDistance { get; private set; } = 100;

    [field: Header("Rotator config")]
    [field: SerializeField] public float MouseScrollMultiplier { get; private set; } = 250f;
}
