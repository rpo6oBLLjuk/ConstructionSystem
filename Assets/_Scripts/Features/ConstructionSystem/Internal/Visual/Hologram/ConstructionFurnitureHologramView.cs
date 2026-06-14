using ColliderVisualizerNamespace;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionFurnitureHologramView : MonoBehaviour
    {
        [Header("References")]
        [Inject] private ConstructionManager _constructionManager;

        [SerializeField] private ConstructionPlacementCollisionChecker _collisionChecker;

        [Header("Colors")]
        [SerializeField] private Color _validSolidColor = new(0f, 1f, 0f, 0.15f);
        [SerializeField] private Color _validWireColor = Color.green;

        [SerializeField] private Color _invalidSolidColor = new(1f, 0f, 0f, 0.15f);
        [SerializeField] private Color _invalidWireColor = Color.red;

        private GameObject _activeObject;
        private ColliderVisualizer _visualizer;


        private void OnEnable()
        {
            _constructionManager.FurniturePrototypeSelected += FurniturePrototypeSelectedHandler;
            _constructionManager.FurniturePrototypeDeselected += FurniturePrototypeDeselectedHandler;
            _constructionManager.FurnitureInstanceCreated += FurnitureInstanceCreatedHandler;
        }
        private void OnDisable()
        {
            _constructionManager.FurniturePrototypeSelected -= FurniturePrototypeSelectedHandler;
            _constructionManager.FurniturePrototypeDeselected -= FurniturePrototypeDeselectedHandler;
            _constructionManager.FurnitureInstanceCreated -= FurnitureInstanceCreatedHandler;

            DisableActiveVisualizer();
        }

        private void Update()
        {
            if (_activeObject == null || _visualizer == null)
                return;

            ConstructionCollisionCheckResult result = _collisionChecker.CurrentResult;
            Collider activeCollider = result.ActiveCollider != null ? result.ActiveCollider : _collisionChecker.GetActiveCollider(_activeObject);

            _visualizer.ChangeCollider(activeCollider);
            SetState(result.IsValid);
        }

        private void FurniturePrototypeSelectedHandler(Furniture furniture, GameObject instance) => InitializeVisualizerWithAwait(instance).Forget();
        private void FurniturePrototypeDeselectedHandler(Furniture furniture, GameObject instance)
        {
            DisableActiveVisualizer();

            _activeObject = null;
            _visualizer = null;
        }
        private void FurnitureInstanceCreatedHandler(Furniture furniture, GameObject instance)
        {
            DisableVisualizerOn(instance);
        }

        private async UniTask InitializeVisualizerWithAwait(GameObject instance)
        {
            await UniTask.WaitForEndOfFrame();

            _activeObject = instance;

            _visualizer = _activeObject.GetOrAddComponent<ColliderVisualizer>();

            _visualizer.enabled = true;
            _visualizer.Initialize();

            _visualizer.NeedDrawSolid = true;
            _visualizer.NeedDrawWire = true;

            _visualizer.ChangeCollider(_collisionChecker.GetActiveCollider(_activeObject));

            SetState(true);
        }

        private void DisableActiveVisualizer()
        {
            if (_visualizer == null)
                return;

            _visualizer.NeedDrawSolid = false;
            _visualizer.NeedDrawWire = false;
            _visualizer.enabled = false;
        }
        private void DisableVisualizerOn(GameObject target)
        {
            ColliderVisualizer visualizer = target.GetComponent<ColliderVisualizer>();

            if (visualizer == null)
                return;

            visualizer.NeedDrawSolid = false;
            visualizer.NeedDrawWire = false;
            visualizer.enabled = false;
        }

        private void SetState(bool isValid)
        {
            _visualizer.SolidColor = isValid ? _validSolidColor : _invalidSolidColor;
            _visualizer.WireColor = isValid ? _validWireColor : _invalidWireColor;
        }
    }
}