using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class ModelPreviewCameraController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
{
    [Inject] FurnitureDataSaver _furnitureDataSaver;

    /// <summary>
    /// Save preview from render texture, byte[] is texture, string is extension
    /// </summary>
    public event Action<byte[], string, Texture2D> PreviewSaveRequested;

    [Header("RenderZone")]
    [field: SerializeField] public Transform ModelContainer { get; private set; }
    [SerializeField] private Transform _wallsContainer;

    [Header("Camera")]
    [SerializeField] private Camera _previewCamera;
    [SerializeField] private Transform _cameraContainer;
    [SerializeField] private RenderTexture _renderTexture;

    [Header("UI Buttons")]
    [SerializeField] private Button _rotateLeftButton;
    [SerializeField] private Button _rotateRightButton;
    [SerializeField] private Button _rotateUpButton;
    [SerializeField] private Button _rotateDownButton;

    [SerializeField] private Button _resetButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _wallsButton;

    [Header("Rotation")]
    [SerializeField] private float _mouseRotationSpeed = 12f;
    [SerializeField] private float _buttonRotationStep = 45f;
    [SerializeField] private float _minVerticalAngle = 0f;
    [SerializeField] private float _maxVerticalAngle = 85f;

    [Header("Zoom")]
    [SerializeField] private float _zoomSpeed = 4f;
    [SerializeField] private float _minDistance = 1.5f;
    [SerializeField] private float _maxDistance = 8f;

    [Header("Default State")]
    [SerializeField] private float _defaultHorizontalAngle = 0f;
    [SerializeField] private float _defaultVerticalAngle = 25f;
    [SerializeField] private float _defaultDistance = 4f;
    [SerializeField] private float _animRotateDuration = 0.1f;

    [Header("Control")]
    [SerializeField] Button _closeButton;
    [SerializeField] CanvasGroup _canvasGroup;

    private bool _isDragging;

    private float _horizontalAngle;
    private float _verticalAngle;
    private float _distance;

    private const string _previewExtension = ".jpg";
    private const int _previewQuality = 85;


    private void Awake()
    {
        if (_previewCamera == null)
            _previewCamera = GetComponentInChildren<Camera>();

        _horizontalAngle = _defaultHorizontalAngle;
        _verticalAngle = Mathf.Clamp(_defaultVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
        _distance = Mathf.Clamp(_defaultDistance, _minDistance, _maxDistance);

        ApplyCameraTransform();

        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        _rotateLeftButton.onClick.AddListener(RotateLeft);
        _rotateRightButton.onClick.AddListener(RotateRight);
        _rotateUpButton.onClick.AddListener(RotateUp);
        _rotateDownButton.onClick.AddListener(RotateDown);

        _resetButton.onClick.AddListener(ResetCamera);

        _saveButton.onClick.AddListener(SaveHandler);
        _closeButton.onClick.AddListener(Hide);
        _wallsButton.onClick.AddListener(ToggleWalls);
    }
    private void OnDisable()
    {
        _rotateLeftButton.onClick.RemoveListener(RotateLeft);
        _rotateRightButton.onClick.RemoveListener(RotateRight);
        _rotateUpButton.onClick.RemoveListener(RotateUp);
        _rotateDownButton.onClick.RemoveListener(RotateDown);

        _resetButton.onClick.RemoveListener(ResetCamera);

        _saveButton.onClick.AddListener(SaveHandler);
        _closeButton.onClick.RemoveListener(Hide);
        _wallsButton.onClick.RemoveListener(ToggleWalls);

        _isDragging = false;
    }

    public void Show()
    {
        _canvasGroup.DOFade(1, 0.25f);
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
    public void Hide()
    {
        _canvasGroup.DOFade(0, 0.25f);
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _isDragging = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging)
            return;

        Vector2 delta = eventData.delta;

        _horizontalAngle += delta.x * _mouseRotationSpeed * Time.deltaTime;
        _verticalAngle -= delta.y * _mouseRotationSpeed * Time.deltaTime;

        ClampVerticalAngle();
        ApplyCameraTransform();
    }
    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;

        _distance -= scroll * _zoomSpeed * Time.deltaTime;
        _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);

        ApplyCameraTransform();
    }

    private void RotateLeft()
    {
        _horizontalAngle += _buttonRotationStep;
        SnapAnglesToStep();
        ApplyCameraTransform(true);
    }
    private void RotateRight()
    {
        _horizontalAngle -= _buttonRotationStep;
        SnapAnglesToStep();
        ApplyCameraTransform(true);
    }
    private void RotateUp()
    {
        _verticalAngle += _buttonRotationStep;
        SnapAnglesToStep();
        ClampVerticalAngle();
        ApplyCameraTransform(true);
    }
    private void RotateDown()
    {
        _verticalAngle -= _buttonRotationStep;
        SnapAnglesToStep();
        ClampVerticalAngle();
        ApplyCameraTransform(true);
    }

    private void ResetCamera()
    {
        _horizontalAngle = _defaultHorizontalAngle;
        _verticalAngle = Mathf.Clamp(_defaultVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
        _distance = Mathf.Clamp(_defaultDistance, _minDistance, _maxDistance);

        ApplyCameraTransform(true);
    }
    private void ToggleWalls() => _wallsContainer.gameObject.SetActive(!_wallsContainer.gameObject.activeSelf);

    private void SaveHandler()
    {
        Texture2D previewTexture = BuildPreviewTexture();
        byte[] bytes = previewTexture.EncodeToJPG(_previewQuality);

        PreviewSaveRequested?.Invoke(bytes, _previewExtension, previewTexture);
    }

    private void SnapAnglesToStep()
    {
        _horizontalAngle = Mathf.Round(_horizontalAngle / _buttonRotationStep) * _buttonRotationStep;
        _verticalAngle = Mathf.Round(_verticalAngle / _buttonRotationStep) * _buttonRotationStep;
    }
    private void ClampVerticalAngle()
    {
        _verticalAngle = Mathf.Clamp(_verticalAngle, _minVerticalAngle, _maxVerticalAngle);
    }
    private void ApplyCameraTransform(bool anim = false)
    {
        if (_cameraContainer == null || _previewCamera == null)
            return;

        Transform cameraTransform = _previewCamera.transform;

        if (!anim)
        {
            _cameraContainer.rotation = Quaternion.Euler(_verticalAngle, _horizontalAngle, 0f);

            cameraTransform.localPosition = new Vector3(0f, 0f, -_distance);
            cameraTransform.localRotation = Quaternion.identity;
        }
        else
        {
            _cameraContainer.DORotateQuaternion(Quaternion.Euler(_verticalAngle, _horizontalAngle, 0f), _animRotateDuration);

            cameraTransform.DOLocalMove(new Vector3(0f, 0f, -_distance), _animRotateDuration);
            cameraTransform.DOLocalRotateQuaternion(Quaternion.identity, _animRotateDuration);
        }
    }

    private Texture2D BuildPreviewTexture()
    {
        RenderTexture previousActive = RenderTexture.active;

        _previewCamera.targetTexture = _renderTexture;
        _previewCamera.Render();

        RenderTexture.active = _renderTexture;

        Texture2D texture = new(_renderTexture.width, _renderTexture.height, TextureFormat.RGBA32, false, false);

        texture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = pixels[i].gamma;
        texture.Apply();

        RenderTexture.active = previousActive;
        return texture;
    }
}
