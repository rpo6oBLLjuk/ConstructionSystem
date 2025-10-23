using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlueprintHistoryView : MonoBehaviour
{
    [SerializeField] BlueprintManager _blueprintManager;

    [SerializeField] Button _undoButton;
    [SerializeField] Button _redoButton;
    [SerializeField] Button _resetButton;

    [SerializeField] float _undoRedoDelay = 0.25f;
    [SerializeField] float _holdThreshold = 1f;

    private Sequence _undoTween;
    private Sequence _redoTween;


    private void OnEnable()
    {
        _undoButton.onClick.AddListener(_blueprintManager.HistoryController.Undo);
        _redoButton.onClick.AddListener(_blueprintManager.HistoryController.Redo);
        _resetButton.onClick.AddListener(_blueprintManager.ResetBlueprint);
    }
    private void OnDisable()
    {
        _undoButton.onClick.RemoveListener(_blueprintManager.HistoryController.Undo);
        _redoButton.onClick.RemoveListener(_blueprintManager.HistoryController.Redo);
        _resetButton.onClick.RemoveListener(_blueprintManager.ResetBlueprint);
    }

    private void Start()
    {
        InitializeTweens();

        AddListenerForTrigger(_undoButton, _undoTween);
        AddListenerForTrigger(_redoButton, _redoTween);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                _undoButton.onClick?.Invoke();
                _undoTween.Restart();
            }
            if (Input.GetKeyUp(KeyCode.Z))
                _undoTween.Pause();
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
            _undoTween.Pause();

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                _redoButton.onClick?.Invoke();
                _redoTween.Restart();
            }
            if (Input.GetKeyUp(KeyCode.Y))
                _redoTween.Pause();
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
            _redoTween.Pause();
    }

    private void InitializeTweens()
    {
        _undoTween = DOTween.Sequence();

        _undoTween.SetDelay(_holdThreshold)
            .Append(DOVirtual.DelayedCall(_undoRedoDelay, () =>
            {
                _undoButton.onClick?.Invoke();
            })
            .SetAutoKill(false)
            .SetLoops(-1)
        ).Pause();

        _redoTween = DOTween.Sequence();
        _redoTween.SetDelay(_holdThreshold)
            .Append(DOVirtual.DelayedCall(_undoRedoDelay, () =>
            {
                _redoButton.onClick?.Invoke();
            })
            .SetAutoKill(false)
            .SetLoops(-1)
        ).Pause();
    }
    private void AddListenerForTrigger(Button button, Tween tween)
    {
        EventTrigger eventTrigger = button.transform.GetOrAddComponent<EventTrigger>();

        EventTrigger.Entry pointerDownEntry = new();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => tween.Restart());

        EventTrigger.Entry pointerUpEntry = new();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => tween.Pause());

        EventTrigger.Entry pointerExitEntry = new();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => tween.Pause());

        eventTrigger.triggers.Add(pointerDownEntry);
        eventTrigger.triggers.Add(pointerUpEntry);
        eventTrigger.triggers.Add(pointerExitEntry);
    }
}
