using UnityEngine;

public class ConstructionBuilder : MonoBehaviour
{
    [SerializeField] private ConstructionRaycaster _raycaster;
    [SerializeField] private GameObject _testConstrustion;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            GameObject.Instantiate(_testConstrustion, _raycaster.hitPoint, Quaternion.identity);
    }
}
