using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField] private bool hideOnStart = true;
    private bool isVisible;

    
    void Start() => ChangeCurcorState(!hideOnStart);
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ChangeCurcorState(true);
    }

    public void ChangeCurcorState(bool isVisible)
    {
        this.isVisible = isVisible;
        Cursor.visible = isVisible;

        Cursor.lockState = (isVisible ? CursorLockMode.None : CursorLockMode.Locked);

    }
}
