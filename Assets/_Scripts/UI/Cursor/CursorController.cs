using UnityEngine;
using Zenject;

public class CursorController : MonoBehaviour
{
    InputSystem _inputSystem;

    public bool IsVisible => _isVisible;
    private bool _isVisible;


    private void Start() => ChangeLockState(CursorLockMode.None);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ChangeCurcorState(true);
    }

    public void ChangeCurcorState(bool isVisible)
    {
        _isVisible = isVisible;
        Cursor.visible = isVisible;

        ChangeLockState(_isVisible ? CursorLockMode.None : CursorLockMode.Locked);

        try
        {
            if (isVisible)
                _inputSystem.InputActionAsset.Player.Disable();
            else
                _inputSystem.InputActionAsset.Player.Enable();
        }
        catch
        {
            Debug.LogWarning("Input system not injected in CursorController");
        }
        
    }

    public void ChangeLockState(CursorLockMode cursorLockMode) => Cursor.lockState = cursorLockMode;
}
