using System;
using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ProjectGridView : AbstractLayoutView<UserProject, BlueprintViewFactory, BlueprintGridEventContext>
{
    [SerializeField] Button _newFileButton;

    public event Action<UserProject> OnProjectSelected;
    public event Action OnNewFileSelected;


    private void OnEnable() => _newFileButton.onClick.AddListener(() => OnNewFileSelected?.Invoke());
    private void OnDisable() => _newFileButton.onClick.RemoveAllListeners();


    public void SetProjectActive(UserProject project, bool isActive)
    {
        if (objectsList.ContainsKey(project.Id))
            objectsList[project.Id].Item2.transform.GetChild(0).GetComponentInChildren<UIEffect>().enabled = isActive;
    }

    private void OnGridElementClick(UserProject project) => OnProjectSelected?.Invoke(project);
}
