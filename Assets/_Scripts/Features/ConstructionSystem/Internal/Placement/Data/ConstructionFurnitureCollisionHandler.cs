using UnityEngine;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionFurnitureCollisionHandler : MonoBehaviour
    {
        public bool Placed = false;

        [SerializeField] private BoxCollider _boundsCollider;
        [SerializeField] private MeshCollider _detailedCollider;

        public BoxCollider BoundsCollider => _boundsCollider;
        public MeshCollider DetailedCollider => _detailedCollider;


        public void Initialize(BoxCollider boundsCollider, MeshCollider detailedCollider)
        {
            _boundsCollider = boundsCollider;
            _detailedCollider = detailedCollider;
        }

        public Collider GetCollider(bool isBox) => isBox ? _boundsCollider : _detailedCollider;
        public Collider GetVisualizationCollider(bool isBox)
        {
            if (isBox)
                return _boundsCollider;

            return _detailedCollider != null ? _detailedCollider : _boundsCollider;
        }
    }
}