using ColliderVisualizerNamespace;
using UnityEngine;
using Zenject;

public class ConstructionBuilder : MonoBehaviour
{
    [Inject] InputSystem _inputSystem;

    [SerializeField] private ConstructionRaycaster _raycaster;
    [SerializeField] private MeshFilter _testConstrustion;
    [SerializeField] ColliderVisualizer _hologram;

    [SerializeField] Color _goodColor = Color.green;
    [SerializeField] Color _badColor = Color.red;

    [SerializeField] LayerMask _testLayerMask;

    private int instancesCount = 0;


    private void Start()
    {
        _hologram.GetComponent<MeshFilter>().mesh = _testConstrustion.mesh; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!\
        if (_hologram.TryGetComponent(out BoxCollider hologramBoxCollider)) //!!!!!!!!!!!!!!!!!!! нужно переместить в контроллер, в "выбранный обект"
        {
            hologramBoxCollider.size = _testConstrustion.mesh.bounds.size;
            hologramBoxCollider.center = _testConstrustion.mesh.bounds.center;
        }
    }

    private void Update()
    {
        Vector3 position = _raycaster.hitPoint;
        Vector3 size = _testConstrustion.mesh.bounds.size;

        _hologram.transform.position = position + Vector3.up * 0.001f; //prevent textures from being clamped

        if (Physics.CheckBox(position + (Vector3.up * size.y / 2), size / 2 * 0.99999f, Quaternion.identity, _testLayerMask, QueryTriggerInteraction.Ignore))
            SetHologrammColor(_badColor);
        else
            SetHologrammColor(_goodColor);


        if (Input.GetMouseButtonDown(0) && _inputSystem.InputActionAsset.Player.enabled)
            InstantiateConstruction(position, Quaternion.identity);
    }

    private void SetHologrammColor(Color color) => _hologram.SolidColor = color;

    private void InstantiateConstruction(Vector3 position, Quaternion rotation)
    {
        instancesCount++;
        string name = _testConstrustion.gameObject.name + $"Instance_{instancesCount}";
        Instantiate(_testConstrustion, position, rotation);
    }
}
