using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintViewFactory : AbstractLayoutFactory<UserProject, BlueprintGridEventContext>
{
    public override void FillData(GameObject gameObject, UserProject data) => gameObject.GetComponentInChildren<TMP_Text>().text = data.ProjectName;
    public override void FillListeners(GameObject gameObject, UserProject data, BlueprintGridEventContext context) => gameObject.GetComponentInChildren<Button>().onClick.AddListener(() => context.OnBlueprintSelected?.Invoke(data));
}
