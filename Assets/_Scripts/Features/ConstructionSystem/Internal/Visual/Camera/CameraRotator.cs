using rpoboBLLjuk.SpaceCanvas;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class CameraRotator : MonoBehaviour
{
    [Inject] ConstructionManager _constructionManager;

    [SerializeField] private float _rotateSpeed = 10;
    [SerializeField] private float _moveDamping = 0.1f;

    [SerializeField] private Vector3 inputVelocity = new();
    [SerializeField] private Vector3 previousVelocity = new();

    private Vector3 refVector = Vector3.zero;
    private bool _allowed = true;
    private bool _activeRotation = false;


    private void OnEnable()
    {
        _constructionManager.FurniturePrototypeSelected += HandleSelect;
        _constructionManager.FurniturePrototypeDeselected += HandleDeselect;
    }
    private void OnDisable()
    {
        _constructionManager.FurniturePrototypeSelected -= HandleSelect;
        _constructionManager.FurniturePrototypeDeselected -= HandleDeselect;
    }

    private void Update()
    {
        if (!_allowed)
            return;

        if (ActiveRotation())
        {
            GetInputVelocity();
            RotateCamera();
        }
    }

    private void HandleSelect(Furniture _, GameObject instance) => _allowed = false;
    private void HandleDeselect(Furniture _, GameObject instance) => _allowed = true;

    private void GetInputVelocity() => inputVelocity = _rotateSpeed * Time.deltaTime * new Vector2(Input.mousePositionDelta.y, Input.mousePositionDelta.x);
    private void RotateCamera()
    {
        Vector3 _smoothedVelocity = Vector3.SmoothDamp(previousVelocity, inputVelocity, ref refVector, _moveDamping * Application.targetFrameRate * Time.deltaTime);
        previousVelocity = _smoothedVelocity;

        Vector3 currentEuler = transform.localEulerAngles;
        float currentX = currentEuler.x > 180 ? currentEuler.x - 360 : currentEuler.x;

        float newX = currentX + -_smoothedVelocity.x;
        float newY = currentEuler.y + _smoothedVelocity.y;
        newX = Mathf.Clamp(newX, -89f, 89f);

        transform.localRotation = Quaternion.Euler(newX, newY, 0f);
    }

    private bool ActiveRotation()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
                SetRotationActivity(true);
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            SetRotationActivity(false);

        return _activeRotation;
    }

    private void SetRotationActivity(bool value)
    {
        _activeRotation = value;
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !value;
    }
}
