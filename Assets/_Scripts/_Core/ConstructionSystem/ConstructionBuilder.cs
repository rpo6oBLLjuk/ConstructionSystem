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
        Vector3 position = _raycaster.hitPoint;
        Vector3 size = _hologramHandler.MeshRenderer.bounds.size;

        _hologramHandler.transform.position = position + Vector3.up * 0.001f; //prevent textures from being clamped

        if (Physics.CheckBox(position + _hologramHandler.BoxCollider.bounds.center, size / 2 * 0.99999f, Quaternion.identity, _testLayerMask, QueryTriggerInteraction.Ignore))
            SetHologrammColor(_badColor);
        else
            SetHologrammColor(_goodColor);


        if (Input.GetMouseButtonDown(0) && _inputSystem.InputActionAsset.Player.enabled)
            InstantiateConstruction(position, Quaternion.identity);
    }

    private void SetHologrammColor(Color color) => _hologramVisualizer.SolidColor = color;

    private void InstantiateConstruction(Vector3 position, Quaternion rotation) //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Написать фабрику для создания объектов
    {
        instancesCount++;
        string name = _hologramHandler.gameObject.name + $"Instance_{instancesCount}";
        GameObject instance = Instantiate(_constructionSystem.CurrentConstructionData.Prefab, position, rotation);
        instance.GetComponent<ConstructionHandler>().Initialize(_constructionSystem.CurrentConstructionData, _constructionSystem.Constructions);
    }
}
