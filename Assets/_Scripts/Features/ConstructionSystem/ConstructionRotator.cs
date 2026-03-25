using UnityEngine;
using Zenject;

public class ConstructionRotator
{
    [Inject] InputSystem _inputSystem;
    [Inject] ConstructionSystemConfig _config;


    private float currentRotationAngle;


    public Quaternion Update()
    {
        if (_inputSystem.InputActionAsset.Player.enabled)
            CalculateRotation(Input.mouseScrollDelta.y);

        return Quaternion.Euler(0, currentRotationAngle, 0);
    }

    private void CalculateRotation(float mouseScrollInput)
    {
        currentRotationAngle += mouseScrollInput * _config.MouseScrollMultiplier * Time.deltaTime;

        if (_inputSystem.InputActionAsset.Player.RotationFixing.ReadValue<bool>())
        {
            float preValue = currentRotationAngle;
            currentRotationAngle = (float)(((int)currentRotationAngle) / 15) * 15;
            Debug.Log($"before: {preValue}, current: {currentRotationAngle}");
        }
    }
}
