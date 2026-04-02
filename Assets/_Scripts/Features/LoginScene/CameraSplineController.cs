using Coffee.UIEffects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class CameraSplineController : MonoBehaviour
{
    [SerializeField] CinemachineSplineDolly _cinemachineSplineDolly;
    [SerializeField] Vector2 _splineStartEndValue = new Vector2(0, 0.97f);

    [SerializeField] float duration = 1.0f;
    [SerializeField] Ease _forwardEase = Ease.InOutQuad;
    [SerializeField] Ease _backwardEase = Ease.OutBounce;

    [SerializeField] Camera _firstCamera;
    [SerializeField] Camera _secondCamera;

    [SerializeField] UIEffectTweener effectTweener;


    bool _isFirst = true;


    private void Start()
    {
        SetActiveCamera(_firstCamera);

        //TestMethod().Forget();
    }

#if UNITY_EDITOR
    private async UniTask TestMethod()
    {
        for (int i = 0; i < 10; i++)
        {
            await UniTask.WaitForSeconds(1);
            AnimateCameraSpline(true);
            await UniTask.WaitForSeconds(duration);

            await UniTask.WaitForSeconds(1);
            AnimateCameraSpline(false);
            await UniTask.WaitForSeconds(duration);
        }
    }
#endif

    private void AnimateCameraSpline(bool forward)
    {
        SetActiveCamera(_firstCamera);

        _cinemachineSplineDolly.CameraPosition = forward ? _splineStartEndValue.x : _splineStartEndValue.y;
        DOTween.To(() => _cinemachineSplineDolly.CameraPosition, x => _cinemachineSplineDolly.CameraPosition = x, forward ? _splineStartEndValue.y : _splineStartEndValue.x, duration)
            .SetEase(forward ? _forwardEase : _backwardEase)
            .OnComplete(() =>
            {
                SetActiveCamera(forward ? _secondCamera : _firstCamera);
                _cinemachineSplineDolly.CameraPosition = forward ? 1 : 0;

                if(forward)
                    effectTweener.PlayForward(true);
            });
    }

    private void SetActiveCamera(Camera localCamera)
    {
        localCamera.enabled = true;
        (localCamera == _firstCamera ? _secondCamera : _firstCamera).enabled = false;
    }

}
