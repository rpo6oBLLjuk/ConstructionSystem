using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BaseLayoutPresenter : MonoBehaviour
{
    public event Action OnPresenterClosed;

    [SerializeField] RectTransform _container;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Button _closeButton;

    [SerializeField] float _fadeDuration = 0.3f;
    [SerializeField] float _moveDuration = 0.4f;
    [SerializeField] Ease _showEase = Ease.OutCubic;
    [SerializeField] Ease _hideEase = Ease.InCubic;

    private Vector2 _storedAnchorPosition;


    protected virtual void Awake()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_container != null)
            _storedAnchorPosition = _container.anchoredPosition;

        SetCanvasGroupState(false);
    }

    protected virtual void OnEnable() => _closeButton.onClick.AddListener(CloseButtonHandler);
    protected virtual void OnDisable() => _closeButton.onClick.RemoveListener(CloseButtonHandler);

    public virtual void Show() => AnimateWindow(true);
    public virtual void Hide() => AnimateWindow(false);

    public void SetCanvasGroupState(bool show)
    {
        _canvasGroup.alpha = show ? 1 : 0;
        _canvasGroup.blocksRaycasts = show;
        _canvasGroup.interactable = show;
    }

    private void CloseButtonHandler()
    {
        OnPresenterClosed?.Invoke();
        Hide();
    }

    private void AnimateWindow(bool show)
    {
        if (show)
        {
            SetCanvasGroupState(true);

            float startX = _container.parent is RectTransform parentRect
                ? parentRect.rect.width
                : Screen.width;

            _container.anchoredPosition = new Vector2(startX, _storedAnchorPosition.y);
            _canvasGroup.alpha = 0f;

            Sequence tween = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(1f, _fadeDuration))
                .Join(_container.DOAnchorPos(_storedAnchorPosition, _moveDuration)
                    .SetEase(_showEase));
        }
        else
        {
            float endX = _container.parent is RectTransform parentRect
                ? parentRect.rect.width
                : Screen.width;

            Sequence tween = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(0f, _fadeDuration))
                .Join(_container.DOAnchorPosX(endX, _moveDuration)
                    .SetEase(_hideEase))
                .OnComplete(() => SetCanvasGroupState(false));
        }
    }

}
