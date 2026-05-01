using System;
using TMPro;
using UnityEngine;

[Serializable]
public class InputFieldValidator
{
    [field: SerializeField] public Vector2 MinMax { get; private set; } = new Vector2(4, 20);
    [field: SerializeField] public TMP_InputField InputField { get; private set; }

    public string text => InputField.text; //Сохранение правил именования компонента

    public bool IsValidLength() =>
        InputField != null &&
        InputField.text.Length >= MinMax.x &&
        InputField.text.Length <= MinMax.y;
}
