using UnityEngine;
using Zenject;

public class CameraRotator : MonoBehaviour
{
    [Inject] InputSystem _inputSystem;

    [SerializeField] private float _rotateSpeed = 10;
    [SerializeField] private float _moveDamping = 0.1f;

    [SerializeField] private Vector3 inputVelocity = new();
    [SerializeField] private Vector3 previousVelocity = new();

    private Vector3 refVector = Vector3.zero;


    private void Update()
    {
        GetInputVelocity();
        RotateCamera();
    }

    private void GetInputVelocity()
    {
        if(!_inputSystem.InputActionAsset.Player.enabled)
        {
            inputVelocity = Vector3.zero;
            return;
        }
        inputVelocity = _rotateSpeed * Time.deltaTime * new Vector2(Input.mousePositionDelta.y, Input.mousePositionDelta.x);
    }

    private void RotateCamera()
    {
        Vector3 _smoothedVelocity = Vector3.SmoothDamp(previousVelocity, inputVelocity, ref refVector, _moveDamping * Application.targetFrameRate * Time.deltaTime);
        previousVelocity = _smoothedVelocity;

        Vector3 currentEuler = transform.localEulerAngles;

        // Конвертируем X угол в диапазон -180 до 180
        float currentX = currentEuler.x > 180 ? currentEuler.x - 360 : currentEuler.x;

        // Вычисляем новые углы
        float newX = currentX + -_smoothedVelocity.x;
        float newY = currentEuler.y + _smoothedVelocity.y;

        // Ограничиваем вертикальный угол
        newX = Mathf.Clamp(newX, -89f, 89f);

        // Устанавливаем rotation
        transform.localRotation = Quaternion.Euler(newX, newY, 0f);
    }

    private Vector3 GetInput() => Input.mousePositionDelta; //need convert to new Input System
}
