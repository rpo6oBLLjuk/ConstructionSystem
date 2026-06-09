public class ActiveProjectService
{
    public UserProject UserProject { get; private set; }
    public ProjectData SelectedProject { get; private set; }

    public void SetActiveProject(ProjectData project) => SelectedProject = project;
    public void Clear() => SelectedProject = null;
}
