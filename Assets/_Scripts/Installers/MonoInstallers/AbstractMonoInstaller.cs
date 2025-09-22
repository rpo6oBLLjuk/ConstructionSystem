using System;
using UnityEngine;
using Zenject;

[Serializable]
public abstract class AbstractMonoInstaller<T> : MonoInstaller where T : MonoBehaviour
{
    [SerializeField] private T service;

    public override void InstallBindings() => Container.Bind<T>().FromInstance(service).AsSingle();
    private void Reset() => service = GetComponent<T>();
}