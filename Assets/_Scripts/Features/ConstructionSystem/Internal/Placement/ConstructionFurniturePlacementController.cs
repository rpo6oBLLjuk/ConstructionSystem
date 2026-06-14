using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionFurniturePlacementController : MonoBehaviour
    {
        [Header("References")]
        [Inject] private ConstructionManager _constructionManager;

        [SerializeField] private ConstructionFurnitureMeshInitializer _meshInitializer = new();
        [SerializeField] private ConstructionPlacementCollisionChecker _collisionChecker;

        [SerializeField] private Camera _camera;

        [Header("Placement")]
        [SerializeField] private bool _useSnapping = false;
        [SerializeField] private float _gridStep = 0.1f;
        [SerializeField] private float _rotationStep = 15f;

        [Space]
        [SerializeField] private float _scrollSensitivity = 5f;
        [SerializeField] private float _floorHeight = 0f;

        private Furniture _activeFurniture;
        private GameObject _activeObject;
        private float _currentRotationY;
        private bool _isPlacing;


        private void OnEnable() => _constructionManager.FurniturePrototypeSelected += FurnitureSelectedHandler;
        private void OnDisable() => _constructionManager.FurniturePrototypeSelected -= FurnitureSelectedHandler;

        private void Update()
        {
            if (!_isPlacing || _activeObject == null)
                return;

            UpdatePosition();
            UpdateRotation();
            ConfirmPlacement();
            CancelPlacement();
        }

        private void FixedUpdate()
        {
            if (!_isPlacing || _activeObject == null)
                return;

            UpdateCollision();
        }

        private void FurnitureSelectedHandler(Furniture furniture, GameObject instance)
        {
            if (instance == null)
                return;

            _activeFurniture = furniture;
            _activeObject = instance;
            _isPlacing = true;

            _currentRotationY = _activeObject.transform.eulerAngles.y;

            _activeObject.SetActive(true);
            _meshInitializer.TryInitialize(_activeObject);
            _collisionChecker.Check(_activeObject);
        }

        private void UpdatePosition()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Plane floorPlane = new(Vector3.up, new Vector3(0f, _floorHeight, 0f));

            if (!floorPlane.Raycast(ray, out float distance))
                return;

            Vector3 targetPosition = ray.GetPoint(distance);

            targetPosition.y = _floorHeight;
            targetPosition = SnapPosition(targetPosition);

            _activeObject.transform.position = targetPosition;
        }
        private void UpdateRotation()
        {
            float scroll = Input.mouseScrollDelta.y * _scrollSensitivity;

            if (Mathf.Approximately(scroll, 0f))
                return;

            _currentRotationY = SnapAngle(_currentRotationY, scroll);
            _activeObject.transform.rotation = Quaternion.Euler(0f, _currentRotationY, 0f);
        }
        private void UpdateCollision() => _collisionChecker.Check(_activeObject);

        private void ConfirmPlacement()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (!_collisionChecker.CurrentResult.IsValid)
                return;

            _constructionManager.CreateFurnitureInstanceByPrototype(_activeObject);
            _constructionManager.DecelectFurniturePrototype(_activeFurniture, _activeObject);

            _isPlacing = false;
            _activeFurniture = null;
            _activeObject = null;
        }
        private void CancelPlacement()
        {
            if (!Input.GetMouseButtonDown(1) && !Input.GetKeyDown(KeyCode.Escape))
                return;

            _constructionManager.DecelectFurniturePrototype(_activeFurniture, _activeObject);

            _isPlacing = false;
            _activeFurniture = null;
            _activeObject = null;
        }

        private Vector3 SnapPosition(Vector3 position)
        {
            if (!_useSnapping)
                return position;

            position.x = Mathf.Round(position.x / _gridStep) * _gridStep;
            position.z = Mathf.Round(position.z / _gridStep) * _gridStep;

            return position;
        }
        private float SnapAngle(float angle, float scroll) => _useSnapping ? Mathf.Round((angle + Mathf.Sign(scroll) * _rotationStep) / _rotationStep) * _rotationStep : Mathf.Round(angle + scroll);
    }
}