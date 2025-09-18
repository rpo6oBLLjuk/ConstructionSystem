using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10;
    [SerializeField] private float _fastMoveSpeed = 50;
    [SerializeField] private float _moveDamping = 0.1f;

    [SerializeField] private Vector3 inputVelocity = new();
    [SerializeField] private Vector3 previousVelocity = new();

    [SerializeField] private Vector3 upDownInputVelocity = new();

    private Vector3 refVector = Vector3.zero;


    private void Update()
    {
        GetInputVelocity();
        transform.position += CalculateLinearVelocity();
    }

    private void GetInputVelocity() => inputVelocity = (transform.forward * GetVerticalInput() + transform.right * GetHorizontalInput() + Vector3.up * GetUpDownInput()) * GetMoveSpeed() * Time.deltaTime;

    private Vector3 CalculateLinearVelocity()
    {
        Vector3 returnValue = Vector3.SmoothDamp(previousVelocity, inputVelocity, ref refVector, _moveDamping);
        previousVelocity = returnValue;
        return returnValue;
    }

    private float GetVerticalInput() => Input.GetAxisRaw("Vertical"); //ned convert to new Input System
    private float GetHorizontalInput() => Input.GetAxisRaw("Horizontal"); //need convert to new Input System
    private float GetUpDownInput() => (Input.GetKey(KeyCode.Space) ? 0.5f : 0) + (Input.GetKey(KeyCode.LeftControl) ? -0.5f : 0);

    private float GetMoveSpeed() => Input.GetKey(KeyCode.LeftShift) ? _fastMoveSpeed : _moveSpeed;
}
