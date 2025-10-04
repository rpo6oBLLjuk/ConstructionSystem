using CustomInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ConstructionObjectData", menuName = "Scriptable Objects/Construction/ObjectData")]
public class ConstructionObjectData : ScriptableObject
{
    [field: SerializeField] public int Id {  get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField, Preview(Size.medium)] public Texture2D Image { get; private set; }
    [field: SerializeField] public string Name { get; private set; }

    public static bool operator !(ConstructionObjectData obj) => obj == null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Prefab != null)
        {
            Texture2D texture = AssetPreview.GetAssetPreview(Prefab);
            Image = texture;
        }
    }
#endif
}