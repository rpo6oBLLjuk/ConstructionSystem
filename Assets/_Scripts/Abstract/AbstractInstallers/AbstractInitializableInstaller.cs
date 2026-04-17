using Zenject;

public class AbstractInitializableInstaller<T> : MonoInstaller where T : IInitializable
{
    public override void InstallBindings() => Container.BindInterfacesAndSelfTo<T>().AsSingle().NonLazy(); // NonLazy сразу создаЄт экземпл€р класса
}
