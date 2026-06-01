using System;
using UnityEngine;

[Serializable]
public abstract class SignUpPhase
{
    public GameObject Container;
    public string PhaseName = "Phase Name";

    public abstract float IsValid();
}

[Serializable]
public class UserDataPhase : SignUpPhase
{
    [field: SerializeField] public InputFieldValidator FirstName { get; private set; }
    [field: SerializeField] public InputFieldValidator LastName { get; private set; }

    public override float IsValid() =>
        (FirstName.IsValidLength() && !string.IsNullOrWhiteSpace(FirstName.text) ? 0.5f : 0) +
        (LastName.IsValidLength() && !string.IsNullOrWhiteSpace(LastName.text) ? 0.5f : 0);
}

[Serializable]
public class ContactsDataPhase : SignUpPhase
{
    [field: SerializeField] public InputFieldValidator Phone { get; private set; }
    [field: SerializeField] public InputFieldValidator Email { get; private set; }

    public override float IsValid() =>
        (Email.IsValidLength() && Email.text.Contains("@") ? 0.5f : 0f) +
        (Phone.IsValidLength() ? 0.5f : 0);
}

[Serializable]
public class SignUpDataPhase : SignUpPhase
{
    [field: SerializeField] public InputFieldValidator Login { get; private set; }
    [field: SerializeField] public InputFieldValidator Pass { get; private set; }
    [field: SerializeField] public InputFieldValidator Confirm { get; set; }

    public override float IsValid() =>
        (Login.IsValidLength() && !string.IsNullOrWhiteSpace(Login.text) ? 0.4f : 0) +
        (Pass.IsValidLength() && !string.IsNullOrWhiteSpace(Pass.text) ? 0.4f : 0) +
        (Confirm.IsValidLength() && Pass.text == Confirm.text ? 0.2f : 0);
}
