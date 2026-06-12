public class ActiveProjectService
{
    public UserProject UserProject { get; private set; }
    public ProjectData ProjectData { get; private set; }


    public void SetActiveProject(UserProject userProject, ProjectData project)
    {
        UserProject = userProject;
        ProjectData = project;
    }
    public void Clear()
    {
        UserProject = null;
        ProjectData = null;
    }
}
