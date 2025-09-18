using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    List<MeshFilter> meshFilters = new();

    private void Start()
    {
        AddMeshByObj(transform); //take self mesh

        GetChildrenRecursive(transform);
        CombineMeshes();
    }

    private void CombineMeshes()
    {
        CombineInstance[] combine = new CombineInstance[meshFilters.Count];

        for (int i = 0; i < meshFilters.Count; ++i)
        {
            // ЗАМЕНИЛИ матрицу трансформации:
            Matrix4x4 relativeMatrix = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
            combine[i].transform = relativeMatrix;

            combine[i].mesh = meshFilters[i].sharedMesh;
            //combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        Mesh mesh = new();
        mesh.CombineMeshes(combine, true);

        if(transform.TryGetComponent(out MeshFilter filter))
        {
            filter.sharedMesh = mesh;
        }
        else
        {
            transform.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;

            Material material = transform.GetComponentInChildren<MeshRenderer>(true).sharedMaterial;
            transform.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }
        transform.gameObject.SetActive(true);
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
            if (!meshFilters.Contains(meshF))
                meshFilters.Add(meshF);
    }
}
