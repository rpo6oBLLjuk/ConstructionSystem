using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BlueprintFieldController : MonoBehaviour
{
    [Inject] BlueprintManager _blueprintManager;
    [Inject] BlueprintVisualConfig _blueprintVisualConfig;

    [SerializeField] private RectTransform _blueprintField;
    [SerializeField] private List<Image> _borders;

    private float _currentScale;
    private Vector3 _previousMousePosition;


    void Update()
    {
        ScrollField();
        MoveField();
        SetBordersSize();
    }

    private void LateUpdate()
    {
        CorrectPosition();
    }

    private void SetBordersSize()
    {
        _borders.ForEach(image =>
        {
            if (image.rectTransform.anchoredPosition.x == 0)
                image.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    _blueprintVisualConfig.DefaultBordersSize / _blueprintManager.ScaleFactor
                );
            else
                image.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    _blueprintVisualConfig.DefaultBordersSize / _blueprintManager.ScaleFactor
                );
        });
    }
    private void ScrollField()
    {
        if (Input.mouseScrollDelta.y != 0)
            _currentScale = _blueprintManager.AddBlueprintScaleFactor(Mathf.Sign(Input.mouseScrollDelta.y));

    }
    private void MoveField()
    {
        //ƒанный финт позвол€ет на врем€ передвижени€ чертежа заблокировать мышь в окне игры, чтобы она не уходила за экран монитора или иные пределы окна
        if (Input.GetMouseButtonDown(2))
        {
            Cursor.lockState = CursorLockMode.Confined;
            _previousMousePosition = Input.mousePosition;
        }
        //ѕо окончании движени€ мыши - курсор разблокируетс€, и пользователь может свободно его использовать
        else if (Input.GetMouseButtonUp(2))
            Cursor.lockState = CursorLockMode.None;

        if (Input.GetMouseButton(2))
        {
            //»спользуетс€ previous/next позици€ мыши, т.к. delta неидеально работает при попиксельном смещении
            Vector3 delta = Input.mousePosition - _previousMousePosition;
            _previousMousePosition = Input.mousePosition;

            //ћышь у кра€ экрана имеет нулевое смещение позиции, поэтому замен€етс€ на дельту смещени€
            if (delta.x == 0)
                delta.x = Input.mousePositionDelta.x;
            if (delta.y == 0)
                delta.y = Input.mousePositionDelta.y;

            _blueprintField.position += delta;
        }
    }

    private void CorrectPosition()
    {
        float width = _blueprintField.rect.width * _blueprintManager.CanvasScaleFactor * _blueprintManager.ScaleFactor;
        float height = _blueprintField.rect.height * _blueprintManager.CanvasScaleFactor * _blueprintManager.ScaleFactor;

        width /= 2; //Half field size
        height /= 2;

        Vector2 correctPosition = new Vector2(Mathf.Clamp(_blueprintField.position.x, Screen.currentResolution.width - width, width), Mathf.Clamp(_blueprintField.position.y, Screen.currentResolution.height - height, height));
        _blueprintField.position = correctPosition;
    }
}
