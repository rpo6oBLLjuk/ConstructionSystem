using UnityEngine;

namespace rpoboBLLjuk.SpaceCanvas
{
    [CreateAssetMenu(fileName = "RoomMaterialData", menuName = "Configs/Construction/Material Data")]
    public class RoomMaterialData : ScriptableObject
    {
        public string MaterialName;

        [Header("Textures")]
        public Texture2D BaseTexture;
        public Texture2D NormalTexture;

        [Header("Normal")]
        [Range(0f, 10f)] public float NormalStrength = 1f;
    }
}
