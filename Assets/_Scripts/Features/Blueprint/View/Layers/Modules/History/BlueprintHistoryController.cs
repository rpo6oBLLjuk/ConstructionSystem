using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

[Serializable]
public class BlueprintHistoryController : MonoBehaviour
{
    [Inject] BlueprintManager _blueprintManager;

    public abstract class HistoryActionData
    {
        public abstract void Execute(BlueprintManager blueprintManager);
        public abstract void Undo(BlueprintManager blueprintManager);
    }

    private class AddPointActionData : HistoryActionData
    {
        private readonly int _index;
        private Vector2 _position;

        public AddPointActionData(int index, Vector2 position)
        {
            _index = index;
            _position = position;
        }

        public override void Execute(BlueprintManager blueprintManager)
        {
            blueprintManager.AddPoint(_index, _position);
            this.FastLog($"Redo: Point <b>{_index}</b> added", sender: nameof(BlueprintHistoryController));
        }
        public override void Undo(BlueprintManager blueprintManager)
        {
            blueprintManager.RemovePoint(_index);
            this.SuccessLog($"Undo: Point <b>{_index}</b> removed", sender: nameof(BlueprintHistoryController));
        }
    }
    private class RemovePointActionData : HistoryActionData
    {
        private readonly int _index;
        private Vector2 _position;

        public RemovePointActionData(int index, Vector2 position)
        {
            _index = index;
            _position = position;
        }

        public override void Execute(BlueprintManager blueprintManager)
        {
            blueprintManager.RemovePoint(_index);
            this.FastLog($"Redo: Point <b>{_index}</b> removed", sender: nameof(BlueprintHistoryController));
        }
        public override void Undo(BlueprintManager blueprintManager)
        {
            blueprintManager.AddPoint(_index, _position);
            this.SuccessLog($"Undo: Point <b>{_index}</b> added", sender: nameof(BlueprintHistoryController));
        }
    }
    private class MovePointActionData : HistoryActionData
    {
        private readonly int _index;
        private Vector2 _previousPosition;
        private Vector2 _nextPosition;

        public MovePointActionData(int index, Vector2 previousPosition, Vector2 nextPosition)
        {
            _index = index;
            _previousPosition = previousPosition;
            _nextPosition = nextPosition;
        }

        public override void Execute(BlueprintManager blueprintManager)
        {
            blueprintManager.MovePoint(_index, _nextPosition);
            this.FastLog($"Redo: MoveToPos {_nextPosition}", sender: nameof(BlueprintHistoryController));
        }
        public override void Undo(BlueprintManager blueprintManager)
        {
            blueprintManager.MovePoint(_index, _previousPosition);
            this.SuccessLog($"Undo: MoveToPos {_nextPosition}", sender: nameof(BlueprintHistoryController));
        }
    }
    private class BlueprintChangeActionData : HistoryActionData
    {
        public Vector2[] _previousPoints;
        public Vector2[] _nextPoints;

        public BlueprintChangeActionData(Vector2[] points) => _previousPoints = points;
        public void AddNextPoints(Vector2[] nextPoints) => _nextPoints ??= nextPoints;

        public override void Execute(BlueprintManager blueprintManager)
        {
            blueprintManager.SetBlueprintData(_nextPoints.ToList());
            this.FastLog("Redo: blueprint data changed", sender: nameof(BlueprintHistoryController));
        }
        public override void Undo(BlueprintManager blueprintManager)
        {
            blueprintManager.SetBlueprintData(_previousPoints.ToList());
            this.SuccessLog("Redo: blueprint data changed", sender: nameof(BlueprintHistoryController));
        }
    }

    public List<HistoryActionData> History { get; private set; } = new();
    public List<HistoryActionData> RedoHistory = new();

    private bool _isPerformingUndoRedo = false;
    private bool _isPerformingDataChanging = false;


    public void Start()
    {
        _blueprintManager.OnPointAdded += AddPointAction;
        _blueprintManager.OnPointRemoved += RemovePointAction;
        _blueprintManager.OnPointMoved += MovePointAction;

        _blueprintManager.OnBlueprintDataChanging += BlueprintDataChangingAction;
        _blueprintManager.OnBlueprintDataChanged += BlueprintDataChangeAction;

        this.InactiveLog("=== Start recording ===");
    }
    public void OnDisable()
    {
        _blueprintManager.OnPointAdded -= AddPointAction;
        _blueprintManager.OnPointRemoved -= RemovePointAction;
        _blueprintManager.OnPointMoved -= MovePointAction;

        _blueprintManager.OnBlueprintDataChanging -= BlueprintDataChangingAction;
        _blueprintManager.OnBlueprintDataChanged -= BlueprintDataChangeAction;
    }

    public void AddPointAction(int index, Vector2 position)
    {
        if (AddActionData(new AddPointActionData(index, position)))
            DebugWrapper.InactiveLog(this, $"Point <b>{index}</b> Added");
    }
    public void RemovePointAction(int index, Vector2 position)
    {
        if (AddActionData(new RemovePointActionData(index, position)))
            DebugWrapper.InactiveLog(this, $"Point <b>{index}</b> Removed");
    }
    public void MovePointAction(int index, Vector2 previousPosition, Vector2 nextPosition)
    {
        if (AddActionData(new MovePointActionData(index, previousPosition, nextPosition)))
            DebugWrapper.InactiveLog(this, $"Point <b>{index}</b> Moved from <u>{previousPosition}</u> to <u>{nextPosition}</u>");
    }

    public void BlueprintDataChangingAction(List<Vector2> points)
    {
        if (AddActionData(new BlueprintChangeActionData(points.ToArray())))
            DebugWrapper.InactiveLog(this, "Blueprint Changing Start");

        _isPerformingDataChanging = true;

    }
    public void BlueprintDataChangeAction(List<Vector2> points)
    {
        BlueprintChangeActionData actionData = (History.Count > 0) ? History[^1] as BlueprintChangeActionData : null;
        actionData ??= RedoHistory[^1] as BlueprintChangeActionData; //Если в History нет данной информации, значит команда выполняется через Redo

        actionData.AddNextPoints(points.ToArray());

        string pointsStr = "Previous points: " + string.Join(", ", actionData._previousPoints.Select(p => p.ToString()));
        pointsStr += "\nNext points: " + string.Join(", ", actionData._nextPoints.Select(p => p.ToString()));

        DebugWrapper.InactiveLog(this, "Blueprint Changed");
        _isPerformingDataChanging = false;
    }

    public void Undo()
    {
        if (History.Count == 0)
            return;

        _isPerformingUndoRedo = true;

        HistoryActionData had = History.Last();
        had.Undo(_blueprintManager);

        History.RemoveAt(History.Count - 1);
        RedoHistory.Add(had);

        _isPerformingUndoRedo = false;
    }
    public void Redo()
    {
        if (RedoHistory.Count == 0)
            return;

        _isPerformingUndoRedo = true;

        HistoryActionData had = RedoHistory.Last();
        had.Execute(_blueprintManager);

        RedoHistory.RemoveAt(RedoHistory.Count - 1);
        History.Add(had);

        _isPerformingUndoRedo = false;
    }

    private bool AddActionData(HistoryActionData actionData)
    {
        if (_isPerformingUndoRedo || _isPerformingDataChanging)
            return false;

        RedoHistory.Clear();
        History.Add(actionData);

        return true;
    }
}