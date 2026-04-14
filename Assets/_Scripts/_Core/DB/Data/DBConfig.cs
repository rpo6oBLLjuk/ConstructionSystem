using UnityEngine;

[CreateAssetMenu(fileName = "DBConfig", menuName = "Scriptable Objects/Config/DB")]
public class DBConfig : ScriptableObject
{
    public string ConnectionString { get; private set; }
}
