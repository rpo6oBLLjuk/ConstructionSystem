using System;
using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ProjectGridView : AbstractLayoutView<UserProject, ProjectViewFactory, ProjectGridEventContext>
{
    [SerializeField] Button _newProjectButton;

    public event Action<UserProject> OnProjectSelected;
    public event Action OnNewProjectSelected;


    private void OnEnable() => _newProjectButton.onClick.AddListener(() => OnNewProjectSelected?.Invoke());
    private void OnDisable() => _newProjectButton.onClick.RemoveAllListeners();


    public void SetProjectActive(UserProject project, bool isActive)
    {
        if (project == null)
            return;

        if (objectsList.ContainsKey(project.Id))
            objectsList[project.Id].Item2.transform.GetChild(0).GetComponentInChildren<UIEffect>().enabled = isActive;
    }

    private void OnGridElementClick(UserProject project) => OnProjectSelected?.Invoke(project);
}
