using System;
using UnityEngine;

[Serializable]
public abstract class SignUpPhase
{
    public GameObject Container;
    public string PhaseName = "Phase Name";

    public abstract bool IsValid();
}

[Serializable]
public class UserDataPhase : SignUpPhase
{
    [field: SerializeField] public InputFieldValidator FirstName { get; private set; }
    [field: SerializeField] public InputFieldValidator LastName { get; private set; }

    public override bool IsValid() =>
        FirstName.IsValidLength() && LastName.IsValidLength() &&
        !string.IsNullOrWhiteSpace(FirstName.text) && !string.IsNullOrWhiteSpace(LastName.text);
}

[Serializable]
public class ContactsDataPhase : SignUpPhase
{
    [field: SerializeField] public InputFieldValidator Phone { get; private set; }
    [field: SerializeField] public InputFieldValidator Email { get; private set; }

    public override bool IsValid() =>
        Email.IsValidLength() && Phone.IsValidLength() &&
        Email.text.Contains("@") && Phone.text.Length > 5;
}

[Serializable]
public class SignUpDataPhase : SignUpPhase
{
    [field: SerializeField] public InputFieldValidator Login { get; private set; }
    [field: SerializeField] public InputFieldValidator Pass { get; private set; }
    [field: SerializeField] InputFieldValidator Confirm { get; set; }

    public override bool IsValid() =>
        Login.IsValidLength() && Pass.IsValidLength() && Confirm.IsValidLength() &&
        !string.IsNullOrWhiteSpace(Login.text) && Pass.text == Confirm.text && Pass.text.Length >= 6;
}
