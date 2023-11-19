using System.Text;
using Bell.Actions;
using Action = Bell.Actions.Action;

namespace Bell.Managers;

internal class ActionManager : IManager
{
    private const int HistoryCapacity = 1000;
    private readonly LinkedList<Action> _actionHistory = new();
    private readonly LinkedList<Action> _actionRedoHistory = new();
    
    internal void DoAction(Action action)
    {
        if (TextBox.Ins.ReadOnly)
            return;
        
        action.DoCommands();
        
        _actionHistory.AddLast(action);
        if (_actionHistory.Count > HistoryCapacity)
        {
            _actionHistory.RemoveFirst();
        }
        _actionRedoHistory.Clear();
    }

    internal void UndoAction()
    {
        if (_actionHistory.Last == null)
            return;

        var lastAction = _actionHistory.Last.Value;
        lastAction.UndoCommands();
        _actionHistory.RemoveLast();
        _actionRedoHistory.AddFirst(lastAction);

        if (lastAction.IsAllSame<InputCharCommand>())
        {
            UndoActionSequence<InputCharCommand>(lastAction);
        }
        else if (lastAction.IsAllSame<DeleteCharCommand>())
        {
            UndoActionSequence<DeleteCharCommand>(lastAction);
        }
    }

    private void UndoActionSequence<T>(Action fromAction)
    {
        while (_actionHistory.Last != null &&
               _actionHistory.Last.Value.IsAllSame<T>())
        {
            var lastAction = _actionHistory.Last.Value;
            if (false == fromAction.IsChained(lastAction))
                break;
            
            lastAction.UndoCommands();
            _actionHistory.RemoveLast();
            _actionRedoHistory.AddFirst(lastAction);

            fromAction = lastAction;
        }
    }

    internal void RedoAction()
    {
        if (_actionRedoHistory.First == null)
            return;

        var firstAction = _actionRedoHistory.First.Value;
        firstAction.RedoCommands();
        _actionRedoHistory.RemoveFirst();
        _actionHistory.AddLast(firstAction);

        if (firstAction.IsAllSame<InputCharCommand>())
        {
            RedoActionSequence<InputCharCommand>(firstAction);
        }
        else if (firstAction.IsAllSame<DeleteCharCommand>())
        {
            RedoActionSequence<DeleteCharCommand>(firstAction);
        }
    }

    private void RedoActionSequence<T>(Action toAction)
    {
        while (_actionRedoHistory.First != null &&
               _actionRedoHistory.First.Value.IsAllSame<T>())
        {
            var firstAction = _actionRedoHistory.First.Value;
            if (false == firstAction.IsChained(toAction))
                break;
            
            firstAction.RedoCommands();
            _actionRedoHistory.RemoveFirst();
            _actionHistory.AddLast(firstAction);

            toAction = firstAction;
        }
    }

    internal string GetDebugString()
    {
        StringBuilder sb = new ();
        sb.AppendLine("History:");
        foreach (var action in _actionHistory)
        {
            sb.AppendLine(action.GetDebugString());
        }

        sb.AppendLine("Redo History:");
        foreach (var action in _actionRedoHistory)
        {
            sb.AppendLine(action.GetDebugString());
        }
        return sb.ToString();
    }

    public void Tick()
    {
    }
}