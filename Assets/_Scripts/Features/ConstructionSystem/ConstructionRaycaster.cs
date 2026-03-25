using System;
using UnityEngine;
using Zenject;

public class ConstructionRaycaster
{
    [Inject] ConstructionSystemConfig _config;
    Camera _camera;


    [Obsolete("¬ынести ссылку на камеру в сервис камеры")]
    public ConstructionRaycaster()
    {
        _camera = Camera.main; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    }


    public Vector3 Update()
    {
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hitInfo, _config.MaxCameraRaycastDistance, _config.RaycasterLayerMask, QueryTriggerInteraction.Ignore))
            return hitInfo.point;
        else if (Physics.Raycast(_camera.transform.position + _camera.transform.forward * _config.MaxCameraRaycastDistance, Vector3.down, out RaycastHit downHit, _config.MaxSnapRaycastDistance, _config.RaycasterLayerMask, QueryTriggerInteraction.Ignore))
            return downHit.point;
        else
            return Vector3.zero;
    }
}
