using Zenject;

public class DBInstallers : MonoInstaller
{
    public override void InstallBindings()
    {
        BaseInitializableInstaller<DBService>.Install(Container);
        BaseInstaller<UserRepository>.Install(Container);
        BaseInitializableInstaller<UserModule>.Install(Container);
    }
}
