using Zenject;

public class DBInstallers : MonoInstaller
{
    public override void InstallBindings()
    {
        BaseInitializableInstaller<DBService>.Install(Container);

        InstallTypes<UserRepository, UserModule>();
        InstallTypes<UserProjectRepository, UserProjectModule>();
    }

    private void InstallTypes<TRepository, TModule>()
    {
        BaseInstaller<TRepository>.Install(Container);
        BaseInitializableInstaller<TModule>.Install(Container);
    }
}
