using UnityEngine;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionFurnitureMeshInitializer
    {
        private readonly MeshCombiner _meshCombiner = new();


        public bool TryInitialize(GameObject target)
        {
            if (target.GetComponent<Collider>() != null)
                return false;

            (Mesh, Material[]) result = _meshCombiner.Combine(target.transform);

            MeshFilter meshFilter = target.GetOrAddComponent<MeshFilter>();
            MeshRenderer meshRenderer = target.GetOrAddComponent<MeshRenderer>();

            meshFilter.sharedMesh = result.Item1;
            meshRenderer.sharedMaterials = result.Item2;

            AddBoxCollider(target, result.Item1, out BoxCollider boxCollider);
            AddMeshCollider(target, result.Item1, out MeshCollider meshCollider);
            AddColliderHandler(target, boxCollider, meshCollider);

            ClearChildren(target.transform);

            return true;
        }

        private void AddBoxCollider(GameObject target, Mesh mesh, out BoxCollider boxCollider)
        {
            boxCollider = target.AddComponent<BoxCollider>();

            boxCollider.center = mesh.bounds.center;
            boxCollider.size = mesh.bounds.size + Vector3.one * 0.0001f;
        }
        private void AddMeshCollider(GameObject target, Mesh mesh, out MeshCollider meshCollider)
        {
            meshCollider = target.AddComponent<MeshCollider>();

            meshCollider.sharedMesh = mesh;
            meshCollider.convex = false;
        }

        private void AddColliderHandler(GameObject target, BoxCollider boxCollider, MeshCollider meshCollider) => target.GetOrAddComponent<ConstructionFurnitureCollisionHandler>().Initialize(boxCollider, meshCollider);

        private void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
                GameObject.Destroy(root.GetChild(i).gameObject);
        }
    }
}