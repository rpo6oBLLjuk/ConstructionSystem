using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class BlueprintHistoryView : MonoBehaviour
{
    [Inject] BlueprintManager _blueprintManager;
    [Inject] BlueprintVisualConfig _visualConfig;

    [SerializeField] BlueprintHistoryController blueprintHistoryController;

    [SerializeField] Button _undoButton;
    [SerializeField] Button _redoButton;
    [SerializeField] Button _resetButton;

    private Sequence _undoTween;
    private Sequence _redoTween;


    private void OnEnable()
    {
        _undoButton.onClick.AddListener(blueprintHistoryController.Undo);
        _redoButton.onClick.AddListener(blueprintHistoryController.Redo);
        _resetButton.onClick.AddListener(_blueprintManager.ResetBlueprint);
    }
    private void OnDisable()
    {
        _undoButton.onClick.RemoveListener(blueprintHistoryController.Undo);
        _redoButton.onClick.RemoveListener(blueprintHistoryController.Redo);
        _resetButton.onClick.RemoveListener(_blueprintManager.ResetBlueprint);

        _undoTween?.Kill();
        _redoTween?.Kill();
    }

    private void Start()
    {
        InitializeTweens();

        AddListenerForTrigger(_undoButton, _undoTween);
        AddListenerForTrigger(_redoButton, _redoTween);
    }

    private void Update()
    {
        GetInput(KeyCode.LeftControl, KeyCode.Z, _undoButton, _undoTween);
        GetInput(KeyCode.LeftControl, KeyCode.Y, _redoButton, _redoTween);
    }
    private void GetInput(KeyCode firstKey, KeyCode secondKey, Button button, Tween tween)
    {
        if (Input.GetKey(firstKey)) //Если первая кнопка (к примеру Ctrl) нажата
        {
            if (Input.GetKeyDown(secondKey)) //Если произошёл момент нажатия второй кнопки (к примеру Z)
            {
                button.onClick?.Invoke(); //Эмулировать нажатие кнопки (к примеру назад)
                tween.Restart(); //Запустить последовательность автоматического нажатия
            }
            if (Input.GetKeyUp(secondKey)) //Если произошёл момент отпускания второй кнопки (к примеру Z)
                tween.Pause(); //Остановить последовательность автоматического нажатия
        }
        else if (Input.GetKeyUp(firstKey))//Если произошёл момент отпускания первой кнопки (к примеру Ctrl)
            tween.Pause(); //Остановить последовательность автоматического нажатия
    }

    private void InitializeTweens()
    {
        CreateSequence(ref _undoTween, _visualConfig.HistoryData.HoldThreshold, _visualConfig.HistoryData.UndoRedoDelay, _undoButton);
        CreateSequence(ref _redoTween, _visualConfig.HistoryData.HoldThreshold, _visualConfig.HistoryData.UndoRedoDelay, _redoButton);
    }
    private void CreateSequence(ref Sequence sequence, float holdThreshold, float delay, Button button)
    {
        sequence = DOTween.Sequence(); //Инициализация "последовательности"
        sequence.SetDelay(holdThreshold) //Выставление начальной задержки
                .Append(DOVirtual.DelayedCall(delay, () => //Добавление в последовательность отложенного вызова через "delay" промежуток
                {
                    button.onClick?.Invoke(); //Эмуляция клика по кнопке
                })
                .SetAutoKill(false) //Запрет автоудаления последовательности
                .SetLoops(-1) //Включение бесконечного повторения
            )
            .Pause(); //Пауза, т.к. это инциализация последовательности
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
