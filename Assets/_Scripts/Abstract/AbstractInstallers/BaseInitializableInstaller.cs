using Zenject;

public class BaseInitializableInstaller<T> : Installer<BaseInitializableInstaller<T>>
{
    public override void InstallBindings() => Container.BindInterfacesAndSelfTo<T>().AsSingle().NonLazy();
}