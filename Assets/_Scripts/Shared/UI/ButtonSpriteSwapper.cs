using UnityEngine;
using UnityEngine.UI;

public class ButtonSpriteSwapper : MonoBehaviour
{
    public Button Button;
    
    [SerializeField] Image image;
    [SerializeField] Sprite _activeSprite;
    [SerializeField] Sprite _inactiveSprite;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            image.sprite = _isActive ? _activeSprite : _inactiveSprite;
        }
    }
    [SerializeField] private bool _isActive = true;


    protected virtual void OnEnable() => Button.onClick.AddListener(InvertActivity);
    protected virtual void OnDisable() => Button.onClick.RemoveListener(InvertActivity);

    protected virtual void Start() => SetActive(IsActive); //Trigger 16th line

    public virtual void SetActive(bool isActive) => IsActive = isActive;
    private void InvertActivity() => SetActive(!IsActive);

    private void Reset()
    {
        Button = GetComponent<Button>();
        if (TryGetComponent(out image))
            _activeSprite = image.sprite;
    }
}
