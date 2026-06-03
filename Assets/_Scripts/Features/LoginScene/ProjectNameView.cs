using DG.Tweening;
using UnityEngine;

public class ProjectNameView : MonoBehaviour
{
    [SerializeField] CameraSplineController _controller;
    [SerializeField] RectTransform _rectTransform;

    [SerializeField] Ease _showEaseType;
    [SerializeField] Ease _hideERaseType;

    private float _defaultXPostion;


    private void OnEnable()
    {
        _controller.OnForwardAnimStarted += HandleHide;
        _controller.OnBackAnimStarted += HandleShow;
    }
    private void OnDisable()
    {
        _controller.OnForwardAnimStarted -= HandleHide;
        _controller.OnBackAnimStarted -= HandleShow;
    }

    private void Start() => _defaultXPostion = _rectTransform.anchoredPosition.x;

    private void HandleHide() => _rectTransform.DOAnchorPosX(-_defaultXPostion - _rectTransform.rect.width * 2, 0.5f).SetEase(_hideERaseType);
    private void HandleShow() => _rectTransform.DOAnchorPosX(_defaultXPostion, 0.5f).SetEase(_showEaseType);
}
