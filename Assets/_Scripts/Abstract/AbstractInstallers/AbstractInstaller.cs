using Zenject;

public class AbstractInstaller<T> : MonoInstaller
{
    public override void InstallBindings() => Container.Bind<T>().AsSingle(); //»нстанс создатс€ после первого запроса к классу
}