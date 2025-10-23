using UnityEngine;
using UnityEngine.UI;

public class ButtonSpriteSwapper : MonoBehaviour
{
    public Button Button;
    
    [SerializeField] Image image;
    [SerializeField] Sprite _activeSprite;
    [SerializeField] Sprite _inactiveSprite;

    public bool IsActiveSprite
    {
        get => _isActiveSprite;
        set
        {
            _isActiveSprite = value;
            image.sprite = _isActiveSprite ? _activeSprite : _inactiveSprite;
        }
    }
    [SerializeField] private bool _isActiveSprite = true;


    private void OnEnable() => Button.onClick.AddListener(InvertActivity);
    private void OnDisable() => Button.onClick.RemoveListener(InvertActivity);

    private void Start() => SetActive(IsActiveSprite); //Trigger 16th line

    public void SetActive(bool isActive) => IsActiveSprite = isActive;
    private void InvertActivity() => IsActiveSprite = !IsActiveSprite;

    private void Reset()
    {
        Button = GetComponent<Button>();
        if (TryGetComponent(out image))
            _activeSprite = image.sprite;
    }
}
