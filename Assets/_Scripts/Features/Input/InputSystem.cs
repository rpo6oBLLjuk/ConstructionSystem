using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public PlayerInputActionAsset InputActionAsset
    {
        get
        {
            if (_InputActionAsset == null)
            {
                _InputActionAsset = new();
                _InputActionAsset.Enable();
            }

            return _InputActionAsset;
        }
    }
    private PlayerInputActionAsset _InputActionAsset;

    private void Awake()
    {

    }
}
