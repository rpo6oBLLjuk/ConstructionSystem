public class ActiveProjectService
{
    public ProjectData SelectedProject { get; private set; }

    public void SetActiveProject(ProjectData project) => SelectedProject = project;
    public void Clear() => SelectedProject = null;
}
