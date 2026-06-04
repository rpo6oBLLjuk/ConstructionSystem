using Zenject;

public class ProjectDataSaverInstaller : MonoInstaller
{
    public override void InstallBindings() => Container.Bind<ProjectDataSaver>().AsSingle();
}
