using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NavigationView : BaseLayoutPresenter
{
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

    NavigationViewRefsContainer _currentActive;


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

    private void SetActive(NavigationViewRefsContainer navigationViewRefsContainer)
    {
        _currentActive = navigationViewRefsContainer;
        _title.text = _currentActive.Data.Title;
        _description.text = _currentActive.Data.Description;
        _preview.sprite = _currentActive.Data.Preview;
    }

    private void HandleEnable()
    {
        SetCanvasGroupState(true);
        SetActive(_projects);
    }

    private void HandleShow() => _currentActive?.Presenter?.Show();

    [Serializable]
    private class NavigationViewRefsContainer
    {
        public NavigationDataContainer Data;
        public Button ListButton;
        public BaseLayoutPresenter Presenter;
    }
}
