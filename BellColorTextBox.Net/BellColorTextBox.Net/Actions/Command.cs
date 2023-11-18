using Bell.Data;
using Bell.Utils;

namespace Bell.Actions;

internal abstract class Command
{
    internal abstract void Do(Caret caret);
    internal abstract void Undo(Caret caret);
    internal abstract string GetDebugString();
}

internal enum EditDirection
{
    Forward,
    Backward
}

internal class InputCharCommand : Command
{
    private readonly EditDirection _direction;
    private readonly char[] _chars;

    internal InputCharCommand(EditDirection direction, char[] chars)
    {
        _direction = direction;
        _chars = chars;
    }

    internal override void Do(Caret caret)
    {
        if (false == LineManager.GetLine(caret.Position.LineIndex, out Line line))
        {
            Logger.Error($"InputCharCommand: Line not found {caret.Position.LineIndex}");
            return;
        }
        
        line.InsertChars(caret.Position.CharIndex, _chars);
        RowManager.SetRowCacheDirty();

        if (EditDirection.Forward == _direction)
        {
            CaretManager.InputCharCaret(caret, _chars.Length);
        }
        CaretManager.ShiftCaretChar(caret.Position.LineIndex, caret.Position.CharIndex, EditDirection.Forward, _chars.Length);
    }

    internal override void Undo(Caret caret)
    {
        if (EditDirection.Forward == _direction)
            new DeleteCharCommand(EditDirection.Backward, _chars.Length).Do(caret);
        else if (EditDirection.Backward == _direction)
            new DeleteCharCommand(EditDirection.Forward, _chars.Length).Do(caret);
    }

    internal override string GetDebugString()
    {
        return $"Input Char {string.Join(' ', _chars)} {_direction}";
    }
}

internal class DeleteCharCommand : Command
{
    private readonly EditDirection _direction;
    private readonly int _count;
    
    private int _deletedCount;
    private char[] _deletedChars = Array.Empty<char>();

    internal DeleteCharCommand(EditDirection direction, int count)
    {
        _direction = direction;
        _count = count;
    }

    internal override void Do(Caret caret)
    {
        if (false == LineManager.GetLine(caret.Position.LineIndex, out Line line))
        {
            Logger.Error($"DeleteCharCommand: Line not found {caret.Position.LineIndex}");
            return;
        }

        _deletedCount = _count;
            
        int targetIndex = caret.Position.CharIndex;
            
        if (EditDirection.Forward == _direction)
        {
            if (targetIndex + _deletedCount > line.CharsCount)
                _deletedCount = line.CharsCount - targetIndex;
            
            _deletedChars = line.RemoveChars(targetIndex, _deletedCount);
            RowManager.SetRowCacheDirty();
            
            CaretManager.ShiftCaretChar(caret.Position.LineIndex, caret.Position.CharIndex, EditDirection.Backward, _deletedCount);
        }
        else if (EditDirection.Backward == _direction)
        {
            if (targetIndex - _deletedCount < 0)
                _deletedCount = targetIndex;
                
            _deletedChars = line.RemoveChars(targetIndex - _deletedCount, _deletedCount);
            RowManager.SetRowCacheDirty();

            CaretManager.ShiftCaretChar(caret.Position.LineIndex, caret.Position.CharIndex, EditDirection.Backward, _deletedCount);
            CaretManager.DeleteCharCaret(caret, _deletedCount);
        }

        if (_count != _deletedCount)
        {
            Logger.Error($"DeleteCharCommand: _count != _deletedCount {_count} {_deletedCount}");
        }
    }

    internal override void Undo(Caret caret)
    {
        if (EditDirection.Forward == _direction)
            new InputCharCommand(EditDirection.Backward, _deletedChars).Do(caret);
        else if (EditDirection.Backward == _direction)
            new InputCharCommand(EditDirection.Forward, _deletedChars).Do(caret);
    }

    internal override string GetDebugString()
    {
        return $"Delete Char {_count} {_direction}";
    }
}

internal class SplitLineCommand : Command
{
    private EditDirection _direction;

    internal SplitLineCommand(EditDirection direction)
    {
        _direction = direction;
    }

    internal override void Do(Caret caret)
    {
        if (false == LineManager.GetLine(caret.Position.LineIndex, out Line line))
        {
            Logger.Error($"SplitLineCommand: Line not found {caret.Position.LineIndex}");
            return;
        }
        
        char[] restOfLine;
        int insertLineIndex;

        if (EditDirection.Forward == _direction)
        {
            // Get forward rest of line
            restOfLine = line.RemoveChars(caret.Position.CharIndex, line.CharsCount - caret.Position.CharIndex);
            
            insertLineIndex = caret.Position.LineIndex + 1;
            
            Line newLine = LineManager.InsertLine(insertLineIndex);
            CaretManager.ShiftCaretLine(insertLineIndex, EditDirection.Forward);
            CaretManager.SplitLineCaret(caret, line, newLine);

            LineManager.ShiftFoldingLine(insertLineIndex, EditDirection.Forward);
            
            newLine.InsertChars(0, restOfLine);
        }
        else
        {
            // Get backward rest of line
            restOfLine = line.RemoveChars(0, caret.Position.CharIndex);
            
            insertLineIndex = caret.Position.LineIndex;
            
            Line newLine = LineManager.InsertLine(insertLineIndex);
            CaretManager.ShiftCaretLine(insertLineIndex, EditDirection.Forward);
            
            LineManager.ShiftFoldingLine(insertLineIndex, EditDirection.Forward);
            
            newLine.InsertChars(0, restOfLine);
        }
        
        RowManager.SetRowCacheDirty();
    }

    internal override void Undo(Caret caret)
    {
        if (EditDirection.Forward == _direction)
            new MergeLineCommand(EditDirection.Backward).Do(caret);
        else if (EditDirection.Backward == _direction)
            new MergeLineCommand(EditDirection.Forward).Do(caret);
    }

    internal override string GetDebugString()
    {
        return $"Split Line {_direction}";
    }
}

internal class MergeLineCommand : Command
{
    private EditDirection _direction;

    internal MergeLineCommand(EditDirection direction)
    {
        _direction = direction;
    }

    internal override void Do(Caret caret)
    {
        if (false == LineManager.GetLine(caret.Position.LineIndex, out Line line))
        {
            Logger.Error($"MergeLineCommand: Line not found {caret.Position.LineIndex}");
            return;
        }
        
        if (EditDirection.Forward == _direction)
        {
            int nextLineIndex = line.Index + 1;
            
            if (false == LineManager.GetLine(nextLineIndex, out Line nextLine))
                return;
            
            CaretManager.MergeLineCaret(caret, line, nextLine);
            
            line.InsertChars(line.CharsCount,
                nextLine.RemoveChars(0, nextLine.CharsCount));
            
            LineManager.RemoveLine(nextLineIndex);
            CaretManager.ShiftCaretLine(nextLineIndex, EditDirection.Backward);
            
            LineManager.ShiftFoldingLine(nextLineIndex, EditDirection.Backward);
        }
        else if (EditDirection.Backward == _direction)
        {
            int currentLineIndex = line.Index;
            int prevLineIndex = line.Index - 1;
            
            if (false == LineManager.GetLine(prevLineIndex, out Line prevLine))
                return;
            
            CaretManager.MergeLineCaret(caret, prevLine, line);
                
            prevLine.InsertChars(prevLine.CharsCount,
                line.RemoveChars(0, line.CharsCount));
            
            LineManager.RemoveLine(currentLineIndex);
            CaretManager.ShiftCaretLine(currentLineIndex, EditDirection.Backward);
            
            LineManager.ShiftFoldingLine(currentLineIndex, EditDirection.Backward);
        }
        
        RowManager.SetRowCacheDirty();
    }

    internal override void Undo(Caret caret)
    {
        if (EditDirection.Forward == _direction)
            new SplitLineCommand(EditDirection.Backward).Do(caret);
        else if (EditDirection.Backward == _direction)
            new SplitLineCommand(EditDirection.Forward).Do(caret);
    }

    internal override string GetDebugString()
    {
        return $"Merge Line {_direction}";
    }
}

internal class IndentSelectionCommand : Command
{
    private readonly HashSet<int> _indentLineIndexList = new();
    
    internal override void Do(Caret caret)
    {
        foreach (Line line in LineManager.Lines)
        {
            caret.GetSorted(out Coordinates start, out Coordinates end);
            if (line.Index < start.LineIndex || line.Index > end.LineIndex)
                continue;

            var tabChars = Singleton.TextBox.TabString;
            line.InsertChars(0, tabChars.ToCharArray());
            RowManager.SetRowCacheDirty();
            CaretManager.ShiftCaretChar(line.Index, 0, EditDirection.Forward, tabChars.Length);
            
            _indentLineIndexList.Add(line.Index);
        }
    }

    internal override void Undo(Caret caret)
    {
        foreach (int lineIndex in _indentLineIndexList)
        {
            if (LineManager.GetLine(lineIndex, out Line line))
            {
                var tabChars = Singleton.TextBox.TabString;
                if (line.GetSubString(0, tabChars.Length - 1) == tabChars)
                {
                    line.RemoveChars(0, tabChars.Length);
                    RowManager.SetRowCacheDirty();
                    CaretManager.ShiftCaretChar(line.Index, 0, EditDirection.Backward, tabChars.Length);
                }
                else
                {
                    Logger.Error($"IndentSelectionCommand: line.GetSubString != tabChars: {line.GetSubString(0, tabChars.Length - 1)} != {tabChars}");
                }
            }
        }
    }

    internal override string GetDebugString()
    {
        return "Indent Selection";
    }
}

internal class UnindentSelectionCommand : Command
{
    private readonly HashSet<int> _unindentLineIndexList = new();
    
    internal override void Do(Caret caret)
    {
        foreach (Line line in LineManager.Lines)
        {
            caret.GetSorted(out Coordinates start, out Coordinates end);
            if (line.Index < start.LineIndex || line.Index > end.LineIndex)
                continue;

            var tabChars = Singleton.TextBox.TabString;
            if (line.GetSubString(0, tabChars.Length - 1) == tabChars)
            {
                line.RemoveChars(0, tabChars.Length);
                RowManager.SetRowCacheDirty();
                CaretManager.ShiftCaretChar(line.Index, 0, EditDirection.Backward, tabChars.Length);
                
                _unindentLineIndexList.Add(line.Index);
            }
        }
    }

    internal override void Undo(Caret caret)
    {
        foreach (int lineIndex in _unindentLineIndexList)
        {
            if (LineManager.GetLine(lineIndex, out Line line))
            {
                var tabChars = Singleton.TextBox.TabString;
                line.InsertChars(0, tabChars.ToCharArray());
                RowManager.SetRowCacheDirty();
                CaretManager.ShiftCaretChar(line.Index, 0, EditDirection.Forward, tabChars.Length);
            }
        }
    }

    internal override string GetDebugString()
    {
        return "Unindent Selection";
    }
}