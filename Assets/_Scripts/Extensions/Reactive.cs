using System;

/// <summary>
/// Личный костыль, созданный в целях упрощённой реактивности, без перегрузки функционала
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class Reactive<T>
{
    private T _value;

    public event Action<T> OnChanged;

    public Reactive() { }

    public Reactive(T initialValue) => _value = initialValue;

    public T Value
    {
        get => _value;
        set
        {
            if (Equals(_value, value))
                return;
            _value = value;
            OnChanged?.Invoke(_value);
        }
    }

    // Неявное приведение к T
    public static implicit operator T(Reactive<T> reactive) => reactive._value;

    // Неявное приведение от T
    public static implicit operator Reactive<T>(T value) => new Reactive<T>(value);

    // Для удобства ToString
    public override string ToString() => _value?.ToString() ?? "null";
}