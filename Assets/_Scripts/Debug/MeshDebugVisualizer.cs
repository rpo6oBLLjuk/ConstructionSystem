using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshDebugVisualizer : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    new private Collider collider;

    private void OnDrawGizmos()
    {
        if (meshRenderer == null)
            return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(meshRenderer.bounds.center, meshRenderer.bounds.size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
    }

    private void Reset()
    {
        collider = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }
}
