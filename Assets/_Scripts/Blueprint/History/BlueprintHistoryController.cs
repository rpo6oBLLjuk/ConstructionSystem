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

    private class ResetBlueprintActionData : HistoryActionData
    {
        private List<Vector2> _points;

        public ResetBlueprintActionData(List<Vector2> points)
        {
            _points = points;
        }

        public override void Execute(BlueprintManager blueprintManager) => blueprintManager.SetBlueprintData(_points);
        public override void Undo(BlueprintManager blueprintManager) => blueprintManager.ResetBlueprint();
    }

    public List<HistoryActionData> History { get; private set; } = new();
    public List<HistoryActionData> RedoHistory = new();


    public void Awake(BlueprintManager blueprintManager) => _blueprintManager = blueprintManager;

    public void AddPointAction(int index, Vector2 position)
    {
        AddActionData(new AddPointActionData(index, position));
        Debug.Log("Point Added");
    }

    public void RemovePointAction(int index, Vector2 position)
    {
        AddActionData(new RemovePointActionData(index, position));
        Debug.Log("Point Removed");
    }

    public void ResetBlueprintAction(List<Vector2> points)
    {
        AddActionData(new ResetBlueprintActionData(points));
        Debug.Log("Blueprint Reseted");
    }

    public void MovePointAction(int index, Vector2 previousPosition, Vector2 nextPosition)
    {
        AddActionData(new MovePointActionData(index, previousPosition, nextPosition));
        Debug.Log("Point Moved");
    }

    public void Undo()
    {
        if(History.Count == 0)
            return;

        Debug.Log("<color=green>Undo</color> completed");

        HistoryActionData had = History.Last();
        had.Undo(_blueprintManager);
        
        History.RemoveAt(History.Count - 1);
        RedoHistory.Add(had);
    }

    public void Redo()
    {
        if (RedoHistory.Count == 0)
            return;

        Debug.Log("<color=red>Redo</color> completed");

        HistoryActionData had = RedoHistory.Last();
        had.Execute(_blueprintManager);

        RedoHistory.RemoveAt(History.Count - 1);
        History.Add(had);
    }

    private void AddActionData(HistoryActionData actionData)
    {
        RedoHistory.Clear();
        History.Add(actionData);
    }
}