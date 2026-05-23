using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneTransitionData", menuName = "Scriptable Objects/SceneTransitionData")]
public class SceneTransitionConfig : ScriptableObject
{
    [field: SerializeField] public List<SceneData> Scenes { get; private set; }

    [field: Header("Animation")]
    [field: SerializeField] public float Duration = 0.5f;
    [field: SerializeField] public Ease HideEaseType = Ease.InOutQuad;
    [field: SerializeField] public Ease ShowEaseType = Ease.OutQuint;


#if UNITY_EDITOR
    private void Reset()
    {
        Scenes.Clear();
        foreach (AppScene appScene in Enum.GetValues(typeof(AppScene)))
        {
            Scenes.Add(new SceneData()
            {
                AppScene = appScene,
                Index = -1
            });
        }
    }
    private void OnValidate() => ValidateScenes();
    private void ValidateScenes()
    {
        Scenes.ForEach(sceneItem =>
        {
            if (sceneItem.Index != -1)
                sceneItem.Name = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneItem.Index));
            else
                sceneItem.Name = "";
        });
        
        Scenes.Where(data => data.Name == "" && data.Index != -1).ToList().ForEach(data => DebugWrapper.LogError(this, $"SceneTransitionList contains unsigned SceneIndex: {data.Index}"));
    }
#endif
}

[Serializable]
public class SceneData
{
    public AppScene AppScene;
    public int Index;
    public string Name;
}