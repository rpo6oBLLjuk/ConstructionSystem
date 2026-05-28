using System;

public class BlueprintGridEventContext : AbstractLayoutEventsContext
{
    public Action<UserProject> OnBlueprintSelected;
    public Action OnNewFileSelected;
}
