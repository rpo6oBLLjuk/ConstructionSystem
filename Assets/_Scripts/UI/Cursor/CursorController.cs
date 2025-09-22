using UnityEngine;
using Zenject;

public class CursorController : MonoBehaviour
{
    [Inject] InputSystem _inputSystem;

    public bool IsVisible => _isVisible;

    [SerializeField] private bool _hideOnStart = true;
    private bool _isVisible;

    void Start() => ChangeCurcorState(!_hideOnStart);
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ChangeCurcorState(true);
    }

    public void ChangeCurcorState(bool isVisible)
    {
        this._isVisible = isVisible;
        Cursor.visible = isVisible;


        if (isVisible)
            _inputSystem.InputActionAsset.Player.Disable();
        else
            _inputSystem.InputActionAsset.Player.Enable();

        Cursor.lockState = (isVisible ? CursorLockMode.None : CursorLockMode.Locked);

    }
}
