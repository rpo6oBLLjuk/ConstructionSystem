namespace rpoboBLLjuk.SpaceCanvas
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "RoomMaterialListConfig", menuName = "Configs/Construction/Material List Config")]
    public class RoomMaterialListConfig : ScriptableObject
    {
        public List<RoomMaterialData> Materials = new();
    }
}
