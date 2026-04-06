using TMPro;
using UnityEngine;

public class PasswordInputFieldView : ButtonSpriteSwapper
{
    [SerializeField] TMP_InputField _InputField;
    [SerializeField] TMP_Text _passwordValidText;
    [SerializeField] char _passwordChar = '•';


    protected override void OnEnable()
    {
        base.OnEnable();
        _InputField.asteriskChar = _passwordChar;
    }

    public override void SetActive(bool isActive)
    {
        base.SetActive(isActive);

        _InputField.contentType = isActive ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        _InputField.ForceLabelUpdate();
        _passwordValidText.characterSpacing = isActive ? 0 : 10;
    }
}
