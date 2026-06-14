using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace rpoboBLLjuk.SpaceCanvas
{
    public enum SlidePanelHideDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public abstract class BaseSlidePanel : MonoBehaviour
    {
        [Header("Base Panel")]
        [SerializeField] protected RectTransform _panelRoot;
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected Button _toggleButton;

        [Header("Slide")]
        [SerializeField] private SlidePanelHideDirection _hideDirection = SlidePanelHideDirection.Left;
        [SerializeField] private float _additionalOffset = 0f;
        [SerializeField] private float _duration = 0.25f;
        [SerializeField] private Ease _showEase = Ease.OutBack;
        [SerializeField] private Ease _hideEase = Ease.InBack;
        [SerializeField] private bool _hideOnStart = true;

        private Vector2 _shownPosition;
        private Vector2 _hiddenPosition;
        private bool _isShown;
        private Tween _currentTween;


        protected virtual void Awake()
        {
            InitializePositions();

            if (_hideOnStart)
                Hide(true);
            else
                Show(true);

            OnPanelInitialized();
        }

        protected virtual void OnEnable()
        {
            if (_toggleButton != null)
                _toggleButton.onClick.AddListener(Toggle);
        }
        protected virtual void OnDisable()
        {
            if (_toggleButton != null)
                _toggleButton.onClick.RemoveListener(Toggle);

            _currentTween?.Kill();
        }

        public void Toggle()
        {
            if (_isShown)
                Hide();
            else
                Show();
        }
        public void Show()
        {
            Show(false);
        }
        public void Hide()
        {
            Hide(false);
        }

        protected virtual void Show(bool instant)
        {
            _isShown = true;

            _currentTween?.Kill();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            if (instant)
            {
                _panelRoot.anchoredPosition = _shownPosition;
                return;
            }

            _currentTween = _panelRoot.DOAnchorPos(_shownPosition, _duration).SetEase(_showEase);
        }
        protected virtual void Hide(bool instant)
        {
            _isShown = false;

            _currentTween?.Kill();

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (instant)
            {
                _panelRoot.anchoredPosition = _hiddenPosition;

                if (_canvasGroup != null)
                    _canvasGroup.alpha = 0f;

                return;
            }

            _currentTween = _panelRoot.DOAnchorPos(_hiddenPosition, _duration)
                .SetEase(_hideEase)
                .OnComplete(() =>
                {
                    if (_canvasGroup != null)
                        _canvasGroup.alpha = 0f;
                });
        }

        protected virtual void OnPanelInitialized()
        {

        }

        private void InitializePositions()
        {
            if (_panelRoot == null)
            {
                DebugWrapper.LogError(this, "Panel root is not assigned.");
                return;
            }

            Canvas.ForceUpdateCanvases();

            _shownPosition = _panelRoot.anchoredPosition;
            _hiddenPosition = _shownPosition + GetHideOffset();
        }
        private Vector2 GetHideOffset()
        {
            float width = _panelRoot.rect.width + _additionalOffset;
            float height = _panelRoot.rect.height + _additionalOffset;

            return _hideDirection switch
            {
                SlidePanelHideDirection.Left => new Vector2(-width, 0f),
                SlidePanelHideDirection.Right => new Vector2(width, 0f),
                SlidePanelHideDirection.Up => new Vector2(0f, height),
                SlidePanelHideDirection.Down => new Vector2(0f, -height),
                _ => Vector2.zero
            };
        }
    }
}
