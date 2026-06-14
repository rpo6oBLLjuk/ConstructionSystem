using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class MeshCombiner
    {
        private readonly List<MeshFilter> _meshFilters = new();
        private readonly List<Material> _materials = new();
        private readonly Dictionary<Material, List<CombineInstance>> _combineByMaterial = new();


        public (Mesh, Material[]) Combine(Transform root)
        {
            _meshFilters.Clear();
            _materials.Clear();
            _combineByMaterial.Clear();

            CollectMeshFilters(root);
            CollectCombineData(root);

            Mesh combinedMesh = BuildCombinedMesh(root);

            _meshFilters.Clear();
            _combineByMaterial.Clear();

            return new(combinedMesh, _materials.ToArray());
        }

        private void CollectMeshFilters(Transform root)
        {
            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);

            foreach (MeshFilter meshFilter in meshFilters)
                _meshFilters.Add(meshFilter);
        }
        private void CollectCombineData(Transform root)
        {
            foreach (MeshFilter meshFilter in _meshFilters)
            {
                MeshRenderer renderer = meshFilter.GetComponent<MeshRenderer>();
                Mesh mesh = meshFilter.sharedMesh;

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    Material material = renderer.sharedMaterials[i];

                    if (!_combineByMaterial.ContainsKey(material))
                    {
                        _combineByMaterial.Add(material, new List<CombineInstance>());
                        _materials.Add(material);
                    }

                    _combineByMaterial[material].Add(new CombineInstance()
                    {
                        mesh = mesh,
                        subMeshIndex = i,
                        transform = root.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix
                    });
                }
            }
        }
        private Mesh BuildCombinedMesh(Transform root)
        {
            List<CombineInstance> materialMeshes = new();

            foreach (Material material in _materials)
            {
                Mesh materialMesh = new();
                materialMesh.indexFormat = IndexFormat.UInt32;
                materialMesh.CombineMeshes(_combineByMaterial[material].ToArray(), true, true);

                materialMeshes.Add(new CombineInstance()
                {
                    mesh = materialMesh,
                    transform = Matrix4x4.identity
                });
            }

            Mesh combinedMesh = new();
            combinedMesh.name = $"{root.name}_CombinedMesh";
            combinedMesh.indexFormat = IndexFormat.UInt32;
            combinedMesh.CombineMeshes(materialMeshes.ToArray(), false, false);
            combinedMesh.RecalculateBounds();
            combinedMesh.RecalculateNormals();

            return combinedMesh;
        }
    }
}