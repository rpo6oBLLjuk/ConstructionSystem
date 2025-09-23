using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    public Bounds bounds;
    public BoxCollider boxCollider;

    [SerializeField] private bool skipMeshCombination = true;


    List<MeshFilter> _meshFilters = new();


    private void Awake()
    {
        if (!enabled)
            return;

        AddMeshByObj(transform); //take self mesh

        GetChildrenRecursive(transform);

        if (skipMeshCombination)
        {
            GetTotalMeshBounds();

            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = bounds.size;
            boxCollider.center = bounds.center;
        }
        else
        {
            CombineMeshes();
        }

        this.enabled = false;
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

    //Fake combine
    private void GetTotalMeshBounds()
    {
        if (_meshFilters.Count == 0)
            return;

        bounds = _meshFilters[0].sharedMesh.bounds;

        Bounds currentBounds;
        for (int i = 1; i < _meshFilters.Count; i++)
        {
            currentBounds = _meshFilters[i].sharedMesh.bounds;
            currentBounds.center += _meshFilters[i].transform.localPosition;
            bounds.Encapsulate(currentBounds);
        }
    }

    //Real combine
    private void CombineMeshes()
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

        if (transform.TryGetComponent(out MeshFilter filter))
            filter.sharedMesh = mesh;
        else
            transform.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;

        Material material = transform.GetComponentInChildren<MeshRenderer>(true).sharedMaterial;
        if (transform.TryGetComponent(out MeshRenderer meshRenderer))
            meshRenderer.sharedMaterial = material;
        else
            transform.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;

        transform.gameObject.SetActive(true);
        transform.gameObject.AddComponent<BoxCollider>();
    }
}
