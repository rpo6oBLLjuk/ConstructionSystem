using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectViewFactory : AbstractLayoutFactory<UserProject, ProjectGridEventContext>
{
    public override void FillData(GameObject gameObject, UserProject data) => gameObject.GetComponentInChildren<TMP_Text>().text = data.ProjectName;
    public override void FillListeners(GameObject gameObject, UserProject data, ProjectGridEventContext context) => gameObject.GetComponentInChildren<Button>().onClick.AddListener(() => context.OnBlueprintSelected?.Invoke(data));
}
