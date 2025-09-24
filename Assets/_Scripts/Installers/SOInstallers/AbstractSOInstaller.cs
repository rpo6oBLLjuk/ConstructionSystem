using UnityEngine;
using Zenject;

public abstract class AbstractSOInstaller<T> : ScriptableObjectInstaller where T : ScriptableObject
{
    [SerializeField] T instance;

    public override void InstallBindings() => Container.Bind<T>().FromInstance(instance);
}
