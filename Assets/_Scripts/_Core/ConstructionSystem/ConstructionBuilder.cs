using ColliderVisualizerNamespace;
using UnityEngine;

public class ConstructionBuilder : MonoBehaviour
{
    [SerializeField] private ConstructionRaycaster _raycaster;
    [SerializeField] private MeshFilter _testConstrustion;
    [SerializeField] ColliderVisualizer _hologram;

    [SerializeField] Color _goodColor = Color.green;
    [SerializeField] Color _badColor = Color.red;


    private void Start()
    {
        _hologram.GetComponent<MeshFilter>().mesh = _testConstrustion.mesh; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!\
        if(_hologram.TryGetComponent(out BoxCollider hologramBoxCollider)) //!!!!!!!!!!!!!!!!!!! нужно переместить в контроллер, в "выбранный обект"
        {
            hologramBoxCollider.size = _testConstrustion.mesh.bounds.size;
            hologramBoxCollider.center = _testConstrustion.mesh.bounds.center;
            //hologramBoxCollider.center = new Vector3(0, _testConstrustion.mesh.bounds.size.y / 2, 0);
        }
    }

    private void Update()
    {
        Vector3 position = _raycaster.hitPoint;
        Vector3 size = _testConstrustion.mesh.bounds.size;

        _hologram.transform.position = position + Vector3.up * 0.001f; //prevent textures from being clamped

        if (Physics.BoxCast(position, size, Vector3.up, _testConstrustion.transform.rotation, 10, _raycaster._layerMask))
        {
            SetHologrammColor(_badColor);
            Debug.Log("Bad");
        }
        else
            SetHologrammColor(_goodColor);


        if (Input.GetMouseButtonDown(0))
            GameObject.Instantiate(_testConstrustion, position, Quaternion.identity);
    }

    private void SetHologrammColor(Color color) => _hologram.SolidColor = color;
}
