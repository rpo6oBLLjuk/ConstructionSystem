using ColliderVisualizerNamespace;
using UnityEngine;
using Zenject;

public class ConstructionBuilder : MonoBehaviour
{
    [Inject] InputSystem _inputSystem;
    [Inject] ConstructionSystem _constructionSystem;

    [SerializeField] private ConstructionRaycaster _raycaster;

    [SerializeField] Color _goodColor = Color.green;
    [SerializeField] Color _badColor = Color.red;

    [SerializeField] LayerMask _testLayerMask;

    [SerializeField] private float mouseScrollMultiplier = 10f;

    ConstructionHandler _hologramHandler;
    ColliderVisualizer _hologramVisualizer;
    
    private float currentRotationAngle = 0f;


    private void Start()
    {
        if (_hologramHandler)
            Destroy(_hologramHandler.gameObject);

        //InstantiateHologramConstryuction
        _hologramHandler = _constructionSystem.InstantiateCurrentConstruction();

        //Set hologram colliders as trigger (QueryTriggerInteraction.Ignore)
        _hologramHandler.BoxCollider.isTrigger = true;
        _hologramHandler.MeshCollider.convex = true;
        _hologramHandler.MeshCollider.isTrigger = true;

        //Get or Add collider visualizer (for hologram construction only)
        _hologramVisualizer = _hologramHandler.transform.GetOrAddComponent<ColliderVisualizer>();
        _hologramVisualizer.enabled = true;
    }

    private void Update()
    {
        if (_inputSystem.InputActionAsset.Player.enabled)
        {
            CalculateRotation(Input.mouseScrollDelta.y);
        }
        

        Vector3 position = _raycaster.hitPoint;
        Vector3 size = _hologramHandler.BoxCollider.size;

        _hologramHandler.transform.rotation = Quaternion.identity * Quaternion.Euler(0, currentRotationAngle, 0);
        Vector3 offset = _hologramHandler.transform.rotation * new Vector3(_hologramHandler.BoxCollider.center.x, 0.001f, _hologramHandler.BoxCollider.center.z);
        _hologramHandler.transform.position = position - offset;

        //prevent textures from being clamped
        if (Physics.CheckBox(position + (Vector3.up * size.y / 2), size / 2 * 0.99999f, Quaternion.identity * Quaternion.Euler(0, currentRotationAngle, 0), _testLayerMask, QueryTriggerInteraction.Ignore))
            SetHologrammColor(_badColor);
        else
            SetHologrammColor(_goodColor);

        if (Input.GetMouseButtonDown(0) && _inputSystem.InputActionAsset.Player.enabled)
            InstantiateConstruction(_hologramHandler.transform.position, Quaternion.identity * Quaternion.Euler(0, currentRotationAngle, 0));
    }
    private void SetHologrammColor(Color color) => _hologramVisualizer.SolidColor = color;

    private void InstantiateConstruction(Vector3 position, Quaternion rotation) => _constructionSystem.InstantiateCurrentConstruction(position: position, rotation: rotation);

    private void CalculateRotation(float mouseScrollInput)
    {
        currentRotationAngle += mouseScrollInput * mouseScrollMultiplier * Time.deltaTime;

        if (_inputSystem.InputActionAsset.Player.RotationFixing.ReadValue<bool>())
        {
            float preValue = currentRotationAngle;
            currentRotationAngle = (float)(((int)currentRotationAngle) / 15) * 15;
            Debug.Log($"before: {preValue}, current: {currentRotationAngle}");
        }
    }
}
