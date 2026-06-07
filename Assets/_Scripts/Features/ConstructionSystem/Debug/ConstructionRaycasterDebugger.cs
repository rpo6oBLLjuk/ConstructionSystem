//using ColliderVisualizerNamespace;
//using UnityEngine;

//public class ConstructionRaycasterDebugger : MonoBehaviour
//{
//    public bool Visualize = true;

//    [SerializeField] ColliderVisualizer _colliderVisualizer;
//    [SerializeField] ConstructionRaycaster _raycaster;


//    private void Awake() => _raycaster = GameObject.FindAnyObjectByType<ConstructionRaycaster>();

//    private void Update()
//    {
//        if (!Visualize) return;
//        if (!_raycaster) return;

//        _colliderVisualizer.transform.position = _raycaster.hitPoint;
//    }
//}
