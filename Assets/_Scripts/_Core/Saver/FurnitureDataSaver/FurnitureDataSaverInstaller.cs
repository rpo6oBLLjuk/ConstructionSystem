using Zenject;

public class FurnitureDataSaverInstaller : MonoInstaller
{
    public override void InstallBindings() => Container.Bind<FurnitureDataSaver>().AsSingle();
}
