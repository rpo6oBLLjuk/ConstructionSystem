using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class NavigationView : BaseLayoutPresenter
{
    [Inject] UserModule _userModule;

    [Header("References")]
    [SerializeField] TMP_Text _title;
    [SerializeField] TMP_Text _description;
    [SerializeField] Image _preview;
    [SerializeField] Button _showButton;

    [Space]
    [SerializeField] CameraSplineController _splineController;

    [Header("Containers")]
    [SerializeField] NavigationViewRefsContainer _projects;
    [SerializeField] NavigationViewRefsContainer _orders;
    [SerializeField] NavigationViewRefsContainer _items;
    [SerializeField] NavigationViewRefsContainer _users;

    [Header("Anim")]
    [SerializeField] float _duration = 0.25f;
    [SerializeField] Ease _easeIn;
    [SerializeField] Ease _easeOut;

    NavigationViewRefsContainer _currentActive;
    private Sequence _previewSequence;


    protected override void OnEnable()
    {
        base.OnEnable();

        _splineController.OnForwardAnimCompleted += HandleEnable;

        _showButton.onClick.AddListener(HandleShow);

        _projects.ListButton.onClick.AddListener(() => SetActive(_projects));
        _orders.ListButton.onClick.AddListener(() => SetActive(_orders));
        _items.ListButton.onClick.AddListener(() => SetActive(_items));
        _users.ListButton.onClick.AddListener(() => SetActive(_users));
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        _splineController.OnForwardAnimCompleted -= HandleEnable;

        _showButton.onClick.RemoveListener(HandleShow);

        _projects.ListButton.onClick.RemoveAllListeners();
        _orders.ListButton.onClick.RemoveAllListeners();
        _items.ListButton.onClick.RemoveAllListeners();
        _users.ListButton.onClick.RemoveAllListeners();
    }

    private void SetActive(NavigationViewRefsContainer navigationViewRefsContainer, bool withoutAnim = false)
    {
        if (_currentActive == navigationViewRefsContainer)
            return;

        _currentActive = navigationViewRefsContainer;
        DoAnimPreview(navigationViewRefsContainer, withoutAnim);
    }

    private void HandleEnable()
    {
        Show();
        SetCanvasGroupState(true);
        SetActive(_projects, true);
    }
    private void HandleShow() => _currentActive?.Presenter?.Show();

    public override void Hide()
    {
        base.Hide();
        _splineController.AnimateCameraSpline(false);
        
        _userModule.LogOut();
    }

    private void DoAnimPreview(NavigationViewRefsContainer navigationViewRefsContainer, bool withoutAnim = false)
    {
        float duration = withoutAnim ? 0 : _duration;

        _previewSequence?.Kill();
        _previewSequence = DOTween.Sequence();

        _previewSequence.Insert(0, _title.DOFade(0, duration).SetEase(_easeIn));
        _previewSequence.Insert(0, _description.DOFade(0, duration).SetEase(_easeIn));
        _previewSequence.Insert(0, _preview.DOFade(0, duration).SetEase(_easeIn));

        _previewSequence.InsertCallback(duration, () =>
        {
            _title.text = navigationViewRefsContainer.Data.Title;
            _description.text = navigationViewRefsContainer.Data.Description;
            _preview.sprite = navigationViewRefsContainer.Data.Preview;
        });

        _previewSequence.Insert(duration, _title.DOFade(1, duration).SetEase(_easeOut));
        _previewSequence.Insert(duration, _description.DOFade(1, duration).SetEase(_easeOut));
        _previewSequence.Insert(duration, _preview.DOFade(1, duration).SetEase(_easeOut));
    }

    [Serializable]
    private class NavigationViewRefsContainer
    {
        public NavigationDataContainer Data;
        public Button ListButton;
        public BaseLayoutPresenter Presenter;
    }
}
