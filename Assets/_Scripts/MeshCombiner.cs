using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner
{
    [SerializeField] private bool skipMeshCombination = true;


    List<MeshFilter> _meshFilters = new();


    public Mesh GetCombinedMesh(Transform transform)
    {
        AddMeshByObj(transform); //take self mesh
        GetChildrenRecursive(transform);

        return CombineMeshes(transform);

        //if (skipMeshCombination) //!!!!!!!!!!!!!!!! Данный код был создан для генерации фейкового комбайн-меша, а точнее для Bounds вокруг. Подлежит экзекуции. !!!!!!!!!!!!!!!!!!!!
        //{
        //    GetTotalMeshBounds();

        //    boxCollider = gameObject.AddComponent<BoxCollider>();
        //    boxCollider.size = bounds.size;
        //    boxCollider.center = bounds.center;
        //}
        //else
        //{
        //    CombineMeshes();
        //}
    }

    public (MeshFilter, MeshRenderer) AddMeshToTransform(Transform transform, Mesh mesh)
    {
        MeshFilter meshFilter = transform.GetOrAddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        Material material = transform.GetComponentInChildren<MeshRenderer>(true).sharedMaterial;

        MeshRenderer meshRenderer = transform.GetOrAddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        return (meshFilter, meshRenderer);
    }

    public (BoxCollider, MeshCollider) AddCollidersToTransform(Transform transform, MeshRenderer renderer)
    {
        BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.size = renderer.bounds.size + Vector3.one * 0.0001f;
        boxCollider.center = renderer.bounds.center - transform.position;

        MeshCollider meshCollider = transform.GetOrAddComponent<MeshCollider>();
        
        return (boxCollider, meshCollider);
    }

    private void GetChildrenRecursive(Transform current)
    {
        AddChildMeshes(current);

        foreach (Transform child in current)
            GetChildrenRecursive(child);
    }
    private void AddChildMeshes(Transform parent)
    {
        foreach (Transform child in parent)
            AddMeshByObj(child);
    }
    private void AddMeshByObj(Transform obj)
    {
        if (obj.TryGetComponent(out MeshFilter meshF))
            if (meshF.mesh && !_meshFilters.Contains(meshF))
                _meshFilters.Add(meshF);
    }

    //Real combine
    private Mesh CombineMeshes(Transform transform)
    {
        CombineInstance[] combine = new CombineInstance[_meshFilters.Count];

        for (int i = 0; i < _meshFilters.Count; ++i)
        {
            combine[i].mesh = _meshFilters[i].sharedMesh;
            combine[i].transform = transform.worldToLocalMatrix * _meshFilters[i].transform.localToWorldMatrix;
            _meshFilters[i].gameObject.SetActive(false);
        }

        Mesh mesh = new();
        mesh.CombineMeshes(combine, true);

        return mesh;
    }

    //!!!!!!!!!!!!!!!! Данный код был создан для генерации фейкового комбайн-меша, а точнее для Bounds вокруг. Подлежит экзекуции. !!!!!!!!!!!!!!!!!!!!
    //Fake combine
    //private void GetTotalMeshBounds()
    //{
    //    if (_meshFilters.Count == 0)
    //        return;

    //    bounds = _meshFilters[0].sharedMesh.bounds;

    //    Bounds currentBounds;
    //    for (int i = 1; i < _meshFilters.Count; i++)
    //    {
    //        currentBounds = _meshFilters[i].sharedMesh.bounds;
    //        currentBounds.center += _meshFilters[i].transform.localPosition;
    //        bounds.Encapsulate(currentBounds);
    //    }
    //}
}
