using UnityEngine;

public class ConstructionRaycaster : MonoBehaviour
{
    public Vector3 hitPoint;

    [SerializeField] Camera _camera;
    [SerializeField] LayerMask _layerMask;

    [SerializeField] float _maxCameraRaycastDistance = 10;
    [SerializeField] float _maxSnapRaycastDistance = 100;

    private void Update()
    {
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hitInfo, _maxCameraRaycastDistance, _layerMask, QueryTriggerInteraction.Ignore))
            hitPoint = hitInfo.point;
        else if (Physics.Raycast(_camera.transform.position + _camera.transform.forward * _maxCameraRaycastDistance, Vector3.down, out RaycastHit downHit, _maxSnapRaycastDistance, _layerMask, QueryTriggerInteraction.Ignore))
            hitPoint = downHit.point;
        else
            hitPoint = Vector3.zero;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
