using UnityEngine;

public class RaycastLengthDecalController : MonoBehaviour
{
    [SerializeField] Transform _camera;

    private void LateUpdate() => transform.position = _camera.position;
}
