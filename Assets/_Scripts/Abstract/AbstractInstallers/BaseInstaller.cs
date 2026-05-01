using Zenject;

public class BaseInstaller<T> : Installer<BaseInstaller<T>>
{
    public override void InstallBindings() => Container.Bind<T>().AsSingle();
}