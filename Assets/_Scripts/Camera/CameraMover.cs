using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10;
    [SerializeField] private float _fastMoveSpeed = 50;
    [SerializeField] private float _verticalSpeedMultiplier = 0.5f;
    [SerializeField] private float _moveDamping = 0.1f;



    [SerializeField] private Vector3 _inputVelocity = new();
    [SerializeField] private Vector3 _previousVelocity = new();

    //[SerializeField] private Vector3 _upDownInputVelocity = new();

    private Vector3 refVector = Vector3.zero;


    private void Update()
    {
        GetInputVelocity();
        transform.position += CalculateLinearVelocity();
    }

    private void GetInputVelocity() => _inputVelocity = (transform.forward * GetVerticalInput() + transform.right * GetHorizontalInput() + Vector3.up * GetUpDownInput()) * GetMoveSpeed() * Time.deltaTime;

    private Vector3 CalculateLinearVelocity()
    {
        Vector3 returnValue = Vector3.SmoothDamp(_previousVelocity, _inputVelocity, ref refVector, _moveDamping);
        _previousVelocity = returnValue;
        return returnValue;
    }

    private float GetVerticalInput() => Input.GetAxisRaw("Vertical"); //ned convert to new Input System
    private float GetHorizontalInput() => Input.GetAxisRaw("Horizontal"); //need convert to new Input System
    private float GetUpDownInput() => (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Q) ? _verticalSpeedMultiplier : 0) - (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.E) ? _verticalSpeedMultiplier : 0);

    private float GetMoveSpeed() => Input.GetKey(KeyCode.LeftShift) ? _fastMoveSpeed : _moveSpeed;
}
