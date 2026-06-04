using Zenject;

public class DBInstallers : MonoInstaller
{
    public override void InstallBindings()
    {
        //Main service install
        BaseInitializableInstaller<DBService>.Install(Container);

        //User install
        InstallTypes<UserRepository, UserModule>();
        
        //UserProjects install
        InstallTypes<UserProjectRepository, UserProjectModule>();

        //Orders install
        InstallTypes<OrderRepository, OrderModule>();
        BaseInstaller<OrderItemRepository>.Install(Container);

        //Furniture install
        InstallTypes<FurnitureRepository, FurnitureModule>();
    }

    private void InstallTypes<TRepository, TModule>()
    {
        BaseInstaller<TRepository>.Install(Container);
        BaseInitializableInstaller<TModule>.Install(Container);
    }
}
