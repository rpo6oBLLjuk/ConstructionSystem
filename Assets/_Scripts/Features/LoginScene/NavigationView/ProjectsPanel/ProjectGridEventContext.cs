using System;

public class ProjectGridEventContext : AbstractLayoutEventsContext
{
    public Action<UserProject> OnBlueprintSelected;
    public Action OnNewFileSelected;
}
