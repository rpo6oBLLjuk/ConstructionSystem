public class ActiveBlueprintService
{
    public BlueprintData SelectedBlueprint { get; private set; }

    public void SetActiveBlueprint(BlueprintData blueprint) => SelectedBlueprint = blueprint;
    public void Clear() => SelectedBlueprint = null;
}
