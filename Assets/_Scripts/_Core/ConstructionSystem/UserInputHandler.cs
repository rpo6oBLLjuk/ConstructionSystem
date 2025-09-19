using System;
using UnityEngine;

public class UserInputHandler : MonoBehaviour
{
    public KeyBindContainer softSnapping; //Привязка к земле или привязка к координатам
    public KeyBindContainer groundSnapping; //Привязка к земле с игнорированием объектов
    public KeyBindContainer magnetSnapping; //Привязка к другим объектам
    public KeyBindContainer visualizeHologram; //Визуализация голограммы установки объекта

    private void Update()
    {
        softSnapping.Update();
        groundSnapping.Update();
    }
}

[Serializable]
public class KeyBindContainer
{
    public Action<bool> KeyStateChanged;

    public KeyCode KeyCode;
    public bool IsHoldable;
    [HideInInspector] public bool IsPressed;


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode))
            Press(IsHoldable ? true : !IsPressed);
        if (Input.GetKeyUp(KeyCode))
        {
            if (IsHoldable)
                Release();

        }
    }

    public void Press(bool value)
    {
        IsPressed = value;
        KeyStateChanged?.Invoke(value);
    }
    public void Release()
    {
        IsPressed = false;
        KeyStateChanged?.Invoke(false);
    }
}