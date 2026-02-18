using System;
using UnityEngine;

public class UserInputHandler : MonoBehaviour
{
    //public KeyBindContainer fixedSnapping; //Привязка к координатам                           //Предположительно S
    //public KeyBindContainer groundSnapping; //Привязка к земле с игнорированием объектов      //Предположительно G
    //public KeyBindContainer magnetSnapping; //Привязка к другим объектам                      //Предположительно M
    //public KeyBindContainer visualizeHologram; //Визуализация голограммы установки объекта    //Предположительно V

    //public KeyBindContainer fixedRotating; //Фиксация вращения по определённым делениям углов //Предположительно R


    //private void Update()
    //{
    //    fixedSnapping.Update();
    //    groundSnapping.Update();
    //    magnetSnapping.Update();
    //    visualizeHologram.Update();
    //    fixedRotating.Update();
    //}
}

//[Serializable]
//public class KeyBindContainer
//{
//    public Action<bool> KeyStateChanged;

//    public KeyCode KeyCode;
//    public bool IsHoldable;
//    [ReadOnly] public bool IsPressed;


//    public void Update()
//    {
//        if (Input.GetKeyDown(KeyCode))
//            Press(IsHoldable ? true : !IsPressed);
//        if (Input.GetKeyUp(KeyCode))
//        {
//            if (IsHoldable)
//                Release();

//        }
//    }

//    public void Press(bool value)
//    {
//        IsPressed = value;
//        KeyStateChanged?.Invoke(value);
//    }
//    public void Release()
//    {
//        IsPressed = false;
//        KeyStateChanged?.Invoke(false);
//    }
//}