using ColliderVisualizerNamespace;
using UnityEngine;
using Zenject;

public class ConstructionBuilder : MonoBehaviour
{
    [Inject] InputSystem _inputSystem;
    [Inject] ConstructionSystem _constructionSystem;

    [SerializeField] private ConstructionRaycaster _raycaster;
    [SerializeField] ConstructionHandler _hologramHandler;
    [SerializeField] ColliderVisualizer _hologramVisualizer;

    [SerializeField] Color _goodColor = Color.green;
    [SerializeField] Color _badColor = Color.red;

    [SerializeField] LayerMask _testLayerMask;

    [SerializeField] private float mouseScrollMultiplier = 10f;
    private float currentRotationAngle = 0f;
    private int instancesCount = 0;


    private void Start()
    {
        if (_hologramHandler)
            Destroy(_hologramHandler.gameObject);

        _hologramHandler = Instantiate(_constructionSystem.CurrentConstructionData.Prefab).GetComponent<ConstructionHandler>();
        _hologramHandler.Initialize(_constructionSystem.CurrentConstructionData, _constructionSystem.Constructions);

        _hologramHandler.BoxCollider.isTrigger = true;
        _hologramHandler.MeshCollider.convex = true;
        _hologramHandler.MeshCollider.isTrigger = true;

        _hologramVisualizer = _hologramHandler.GetComponent<ColliderVisualizer>();
        _hologramVisualizer.enabled = true;
    }

    private void Update()
    {
        float mouseScroll = Input.mouseScrollDelta.y * mouseScrollMultiplier;
        currentRotationAngle += mouseScroll * Time.deltaTime;

        Vector3 position = _raycaster.hitPoint;
        Vector3 size = _hologramHandler.BoxCollider.size;
        //Debug.Log($"BoxCollider.size: {_hologramHandler.BoxCollider.size}, Renderer.bounds.size: {_hologramHandler.MeshRenderer.bounds.size}");

        //Смещение при повороте на 90 градусов до 0.1f по z (и/или x)
        _hologramHandler.transform.rotation = Quaternion.identity * Quaternion.Euler(0, currentRotationAngle, 0);
        Vector3 offset = new Vector3(_hologramHandler.BoxCollider.center.x, 0.001f, _hologramHandler.BoxCollider.center.z);
        offset = _hologramHandler.transform.rotation * offset;
        _hologramHandler.transform.position = position - offset; //prevent textures from being clamped

        if (Physics.CheckBox(position + (Vector3.up * size.y / 2), size / 2 * 0.99999f, Quaternion.identity * Quaternion.Euler(0, currentRotationAngle, 0), _testLayerMask, QueryTriggerInteraction.Ignore))
            SetHologrammColor(_badColor);
        else
            SetHologrammColor(_goodColor);

        Debug.Log($"Hologram: {_hologramHandler.transform.position}, Box: {position + (Vector3.up * size.y / 2)}");

        if (Input.GetMouseButtonDown(0) && _inputSystem.InputActionAsset.Player.enabled)
            InstantiateConstruction(position, Quaternion.identity * Quaternion.Euler(0, currentRotationAngle, 0));
    }
    public Vector3 RotateAroundYAxis(Vector3 inputVector, float angleDegrees)
    {
        Quaternion rotation = Quaternion.Euler(0f, angleDegrees, 0f);

        return rotation * inputVector;
    }
    private void SetHologrammColor(Color color) => _hologramVisualizer.SolidColor = color;

    private void InstantiateConstruction(Vector3 position, Quaternion rotation) //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Написать фабрику для создания объектов
    {
        instancesCount++;
        string name = _hologramHandler.gameObject.name + $"Instance_{instancesCount}";
        GameObject instance = Instantiate(_constructionSystem.CurrentConstructionData.Prefab, position, Quaternion.identity);
        instance.GetComponent<ConstructionHandler>().Initialize(_constructionSystem.CurrentConstructionData, _constructionSystem.Constructions);
        instance.transform.rotation = rotation; //Вращение после поворота для того, чтобы Mesh.Bounds корректно рассчитались.
    }
}
