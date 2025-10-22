using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BlueprintHistoryController
{
    BlueprintManager _blueprintManager;

    public abstract class HistoryActionData
    {
        public abstract void Execute(BlueprintManager blueprintManager);
        public abstract void Undo(BlueprintManager blueprintManager);
    }

    private class AddPointActionData : HistoryActionData
    {
        private int _index;
        private Vector2 _position;

        public AddPointActionData(int index, Vector2 position)
        {
            _index = index;
            _position = position;
        }

        public override void Execute(BlueprintManager blueprintManager) => blueprintManager.AddPoint(_index, _position);
        public override void Undo(BlueprintManager blueprintManager) => blueprintManager.RemovePoint(_index);
    }
    private class RemovePointActionData : HistoryActionData
    {
        private int _index;
        private Vector2 _position;

        public RemovePointActionData(int index, Vector2 position)
        {
            _index = index;
            _position = position;
        }

        public override void Execute(BlueprintManager blueprintManager) => blueprintManager.RemovePoint(_index);
        public override void Undo(BlueprintManager blueprintManager) => blueprintManager.AddPoint(_index, _position);
    }

    private class MovePointActionData : HistoryActionData
    {
        private int _index;
        private Vector2 _previousPosition;
        private Vector2 _nextPosition;

        public MovePointActionData(int index, Vector2 previousPosition, Vector2 nextPosition)
        {
            _index = index;
            _previousPosition = previousPosition;
            _nextPosition = nextPosition;
        }

        public override void Execute(BlueprintManager blueprintManager) => blueprintManager.MovePoint(_index, _nextPosition);
        public override void Undo(BlueprintManager blueprintManager) => blueprintManager.MovePoint(_index, _previousPosition);
    }

    private class BlueprintChangeActionData : HistoryActionData
    {
        public Vector2[] _previousPoints;
        public Vector2[]? _nextPoints;

        public BlueprintChangeActionData(Vector2[] points) => _previousPoints = points;
        public void AddNextPoints(Vector2[] nextPoints) => _nextPoints ??= nextPoints;

        public override void Execute(BlueprintManager blueprintManager) => blueprintManager.SetBlueprintData(_nextPoints.ToList());
        public override void Undo(BlueprintManager blueprintManager) => blueprintManager.SetBlueprintData(_previousPoints.ToList());
    }

    public List<HistoryActionData> History { get; private set; } = new();
    public List<HistoryActionData> RedoHistory = new();

    private bool _isPerformingUndoRedo = false;
    private bool _isPerformingDataChanging = false;


    public void OnEnable()
    {
        _blueprintManager.OnPointAdded += AddPointAction;
        _blueprintManager.OnPointRemoved += RemovePointAction;
        _blueprintManager.OnPointMoved += MovePointAction;

        _blueprintManager.OnBlueprintDataChanging += BlueprintDataChangingAction;
        _blueprintManager.OnBlueprintDataChanged += BlueprintDataChangeAction;
    }
    public void OnDisable()
    {
        _blueprintManager.OnPointAdded -= AddPointAction;
        _blueprintManager.OnPointRemoved -= RemovePointAction;
        _blueprintManager.OnPointMoved -= MovePointAction;

        _blueprintManager.OnBlueprintDataChanging -= BlueprintDataChangingAction;
        _blueprintManager.OnBlueprintDataChanged -= BlueprintDataChangeAction;
    }

    public void Awake(BlueprintManager blueprintManager) => _blueprintManager = blueprintManager;

    public void AddPointAction(int index, Vector2 position)
    {
        if (AddActionData(new AddPointActionData(index, position)))
            DebugWrapper.FastLog(this, "Point Added");
    }

    public void RemovePointAction(int index, Vector2 position)
    {
        if (AddActionData(new RemovePointActionData(index, position)))
            DebugWrapper.FastLog(this, "Point Removed");
    }

    public void MovePointAction(int index, Vector2 previousPosition, Vector2 nextPosition)
    {
        if (AddActionData(new MovePointActionData(index, previousPosition, nextPosition)))
            DebugWrapper.FastLog(this, "Point Moved");
    }

    public void BlueprintDataChangingAction(List<Vector2> points)
    {
        if (AddActionData(new BlueprintChangeActionData(points.ToArray())))
            DebugWrapper.FastLog(this, "Blueprint Changing Start");
        
        _isPerformingDataChanging = true;

    }
    public void BlueprintDataChangeAction(List<Vector2> points)
    {
        BlueprintChangeActionData actionData = History[^1] as BlueprintChangeActionData;
        actionData ??= RedoHistory[^1] as BlueprintChangeActionData; //Если в History нет данной информации, значит команда выполняется через Redo

        actionData.AddNextPoints(points.ToArray());

        string pointsStr = "Previous points: " + string.Join(", ", actionData._previousPoints.Select(p => p.ToString()));
        pointsStr += "\nNext points: " + string.Join(", ", actionData._nextPoints.Select(p => p.ToString()));

        DebugWrapper.LogWarning(this, pointsStr);

        DebugWrapper.FastLog(this, "Blueprint Changed");
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

        DebugWrapper.FastLog(this, "<color=green>Undo</color> completed");
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

        DebugWrapper.FastLog(this, "<color=green>Redo</color> completed");
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