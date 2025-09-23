using UnityEngine;
using UnityEngine.UI;

public class WindowCloser : MonoBehaviour
{
    [SerializeField] private RectTransform closedObject;

    [SerializeField] Button openButton;
    [SerializeField] Button closeButon;

    [SerializeField] bool isVisible = false;


    void OnEnable()
    {
        openButton.onClick.AddListener(OnOpenButtonClick);
        closeButon.onClick.AddListener(OnCloseButtonClick);
    }
    void OnDisable()
    {
        closeButon.onClick.RemoveListener(OnCloseButtonClick);
        openButton.onClick.RemoveListener(OnOpenButtonClick);
    }

    void Awake() => ChangeWindowState(isVisible);

    void OnOpenButtonClick() => ChangeWindowState(true);
    void OnCloseButtonClick() => ChangeWindowState(false);

    private void ChangeWindowState(bool isVisible)
    {
        openButton.gameObject.SetActive(!isVisible);
        closeButon.gameObject.SetActive(isVisible);

        closedObject.gameObject.SetActive(isVisible);
    }
}
