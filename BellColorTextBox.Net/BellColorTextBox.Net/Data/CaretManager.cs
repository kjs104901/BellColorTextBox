using System.Text;
using Bell.Actions;
using Bell.Utils;

namespace Bell.Data;

// Interface
internal partial class CaretManager
{
    internal static void ClearCarets() => Singleton.TextBox.CaretManager.ClearCarets_();
    internal static void AddCaret(Coordinates coordinates) => Singleton.TextBox.CaretManager.AddCaret_(coordinates);
    internal static void AddCaret(Caret caret) => Singleton.TextBox.CaretManager.AddCaret_(caret);
    internal static int Count => Singleton.TextBox.CaretManager._carets.Count;
    internal static Caret GetCaret(int index) => Singleton.TextBox.CaretManager._carets[index];
    internal static bool GetFirstCaret(out Caret caret) => Singleton.TextBox.CaretManager.GetFirstCaret_(out caret);

    internal static void MoveCaretsPosition(CaretMove caretMove) =>
        Singleton.TextBox.CaretManager.MoveCaretsPosition_(caretMove);

    internal static void MoveCaretsAnchor(CaretMove caretMove) =>
        Singleton.TextBox.CaretManager.MoveCaretsAnchor_(caretMove);

    internal static bool HasCaretsSelection() => Singleton.TextBox.CaretManager.HasCaretsSelection_();
    internal static void RemoveCaretsSelection() => Singleton.TextBox.CaretManager.RemoveCaretsSelection_();
    internal static void RemoveCaretsLineSub() => Singleton.TextBox.CaretManager.RemoveCaretsLineSub_();

    internal static void SelectRectangle(List<ValueTuple<Coordinates, Coordinates>> ranges, bool isReversed) =>
        Singleton.TextBox.CaretManager.SelectRectangle_(ranges, isReversed);

    internal static void CopyClipboard() => Singleton.TextBox.CaretManager.CopyClipboard_();
    internal static void PasteClipboard() => Singleton.TextBox.CaretManager.PasteClipboard_();
    internal static bool CheckValid(Caret caret) => Singleton.TextBox.CaretManager.CheckValid_(caret);
    internal static string GetDebugString() => Singleton.TextBox.CaretManager.GetDebugString_();

    internal static void ShiftCaretChar(int lineIndex, int charIndex, EditDirection direction, int count) =>
        Singleton.TextBox.CaretManager.ShiftCaretChar_(lineIndex, charIndex, direction, count);

    internal static void InputCharCaret(Caret caret, int count) =>
        Singleton.TextBox.CaretManager.InputCharCaret_(caret, count);

    internal static void DeleteCharCaret(Caret caret, int count) =>
        Singleton.TextBox.CaretManager.DeleteCharCaret_(caret, count);

    internal static void ShiftCaretLine(int lineIndex, EditDirection direction) =>
        Singleton.TextBox.CaretManager.ShiftCaretLine_(lineIndex, direction);

    internal static void MergeLineCaret(Caret caret, Line line, Line fromLine) =>
        Singleton.TextBox.CaretManager.MergeLineCaret_(caret, line, fromLine);

    internal static void SplitLineCaret(Caret caret, Line line, Line toLine) =>
        Singleton.TextBox.CaretManager.SplitLineCaret_(caret, line, toLine);
    
    internal static bool IsLineHasCaret(int lineIndex) =>
        Singleton.TextBox.CaretManager.IsLineHasCaret_(lineIndex);
}

// Implementation
internal partial class CaretManager
{
    private readonly List<Caret> _carets = new();
    private readonly List<string> _clipboard = new();

    private void ClearCarets_()
    {
        _carets.Clear();
    }

    private void AddCaret_(Coordinates coordinates)
    {
        AddCaret_(new Caret() { Position = coordinates, AnchorPosition = coordinates });
    }

    private void AddCaret_(Caret newCaret)
    {
        if (false == CheckValid_(newCaret))
        {
            Logger.Error(
                $"AddCaret: invalid caret: {newCaret.Position.LineIndex} {newCaret.Position.CharIndex} {newCaret.AnchorPosition.LineIndex} {newCaret.AnchorPosition.CharIndex}");
            return;
        }

        _carets.Add(newCaret);
        RemoveDuplicatedCarets_();
    }

    private bool GetFirstCaret_(out Caret caret)
    {
        if (_carets.Count > 1)
        {
            _carets.RemoveRange(1, _carets.Count - 1);
        }

        if (_carets.Count > 0)
        {
            caret = _carets[0];
            return true;
        }

        caret = Caret.None;
        return false;
    }

    private void MoveCaretsPosition_(CaretMove caretMove)
    {
        foreach (Caret caret in _carets)
        {
            caret.Position = caret.Position.FindMove(caretMove);
        }

        RemoveDuplicatedCarets_();
        RowManager.SetRowCacheDirty();
    }

    private void MoveCaretsAnchor_(CaretMove caretMove)
    {
        foreach (Caret caret in _carets)
        {
            caret.AnchorPosition = caret.AnchorPosition.FindMove(caretMove);
        }

        RemoveDuplicatedCarets_();
        RowManager.SetRowCacheDirty();
    }

    private bool HasCaretsSelection_()
    {
        foreach (Caret caret in _carets)
        {
            if (caret.HasSelection)
                return true;
        }

        return false;
    }

    private void RemoveCaretsSelection_()
    {
        foreach (Caret caret in _carets)
        {
            caret.AnchorPosition = caret.Position;
        }

        RemoveDuplicatedCarets_();
        RowManager.SetRowCacheDirty();
    }

    private void RemoveCaretsLineSub_()
    {
        foreach (Caret caret in _carets)
        {
            caret.Position.LineSubIndex = -1;
            caret.AnchorPosition.LineSubIndex = -1;
        }
        RowManager.SetRowCacheDirty();
    }

    private void SelectRectangle_(List<ValueTuple<Coordinates, Coordinates>> ranges, bool isReversed)
    {
        _carets.Clear();
        bool hasSelection = ranges.Any(range => range.Item1.CharIndex != range.Item2.CharIndex);

        foreach ((Coordinates, Coordinates) range in ranges)
        {
            if (hasSelection)
            {
                if (range.Item1.CharIndex == range.Item2.CharIndex)
                    continue;
            }

            _carets.Add(new Caret()
            {
                Position = isReversed ? range.Item1 : range.Item2,
                AnchorPosition = isReversed ? range.Item2 : range.Item1
            });
        }

        RemoveDuplicatedCarets_();
        RowManager.SetRowCacheDirty();
    }

    private void CopyClipboard_()
    {
        if (_carets.Count == 0)
            return;

        _clipboard.Clear();
        foreach (Row row in RowManager.Rows)
        {
            if (row.RowSelection.Selected)
            {
                if (null != row.LineSub && LineManager.GetLine(row.LineSub.Coordinates.LineIndex, out Line line))
                {
                    string text = line.GetSubString(row.RowSelection.SelectionStartChar,
                        row.RowSelection.SelectionEndChar);
                    _clipboard.Add(text);
                }
            }
        }

        Singleton.TextBox.Backend.SetClipboard(string.Join(Singleton.TextBox.GetEolString(), _clipboard));
    }

    private void PasteClipboard_()
    {
        if (HasCaretsSelection())
        {
            Singleton.TextBox.ActionManager.DoAction(new DeleteSelectionAction());
            RemoveCaretsSelection();
        }
        
        string clipboardText = Singleton.TextBox.Backend.GetClipboard();
        clipboardText = Singleton.TextBox.ReplaceTab(clipboardText);
        clipboardText = Singleton.TextBox.ReplaceEol(clipboardText);
        
        Singleton.TextBox.ActionManager.DoAction(new PasteAction(clipboardText));
        
        // TODO: If selected row number is same as pasted row number, directly paste
    }

    private bool CheckValid_(Caret caret)
    {
        if (false == caret.Position.IsValid())
            return false;

        if (false == caret.AnchorPosition.IsValid())
            return false;

        return true;
    }

    private string GetDebugString_()
    {
        StringBuilder sb = new();
        foreach (Caret caret in _carets)
        {
            sb.AppendLine("Caret:");
            sb.AppendLine("\tPosition\t" + caret.Position.LineIndex + ":" + caret.Position.CharIndex + ":" +
                          caret.Position.LineSubIndex);
            sb.AppendLine("\tAnchorPosition\t" + caret.AnchorPosition.LineIndex + ":" + caret.AnchorPosition.CharIndex +
                          ":" + caret.AnchorPosition.LineSubIndex);
        }
        return sb.ToString();
    }

    private void ShiftCaretChar_(int lineIndex, int charIndex, EditDirection direction, int count)
    {
        int moveCount = count * (EditDirection.Forward == direction ? 1 : -1);

        foreach (Caret caret in _carets)
        {
            if (caret.Position.LineIndex == lineIndex)
            {
                if (charIndex < caret.Position.CharIndex)
                {
                    caret.Position.CharIndex += moveCount;
                    caret.Position.Validate();
                }
            }

            if (caret.AnchorPosition.LineIndex == lineIndex)
            {
                if (charIndex < caret.AnchorPosition.CharIndex)
                {
                    caret.AnchorPosition.CharIndex += moveCount;
                    caret.AnchorPosition.Validate();
                }
            }

            Logger.Info(
                "ShiftCaretChar: " + caret.Position.LineIndex + " " + caret.Position.CharIndex + " " + moveCount);
        }
    }

    private void InputCharCaret_(Caret caret, int count)
    {
        caret.Position.CharIndex += count;
        caret.Position.Validate();
        caret.RemoveSelection();
        RemoveDuplicatedCarets_();
    }

    private void DeleteCharCaret_(Caret caret, int count)
    {
        caret.Position.CharIndex -= count;
        caret.Position.Validate();
        caret.RemoveSelection();
        RemoveDuplicatedCarets_();
    }

    private void ShiftCaretLine_(int lineIndex, EditDirection direction)
    {
        int moveCount = EditDirection.Forward == direction ? 1 : -1;
        foreach (Caret c in _carets)
        {
            if (lineIndex < c.Position.LineIndex)
            {
                c.Position.LineIndex += moveCount;
                c.Position.Validate();
            }

            if (lineIndex < c.AnchorPosition.LineIndex)
            {
                c.AnchorPosition.LineIndex += moveCount;
                c.AnchorPosition.Validate();
            }

            Logger.Info("ShiftCaretLine: " + c.Position.LineIndex + " " + c.Position.CharIndex + " " + moveCount);
        }
    }

    private void MergeLineCaret_(Caret caret, Line line, Line fromLine)
    {
        foreach (Caret c in _carets)
        {
            if (c.Position.LineIndex == fromLine.Index)
            {
                c.Position.LineIndex = line.Index;
                c.Position.CharIndex += line.CharsCount;
                //c.Position.Validate(); Skip this validation because of command order. TODO: Fix this
            }

            if (c.AnchorPosition.LineIndex == fromLine.Index)
            {
                c.AnchorPosition.LineIndex = line.Index;
                c.AnchorPosition.CharIndex += line.CharsCount;
                //c.AnchorPosition.Validate(); Skip this validation because of command order. TODO: Fix this
            }

            Logger.Info("MergeLineCaret: " + c.Position.LineIndex + " " + c.Position.CharIndex);
        }

        caret.RemoveSelection();
        RemoveDuplicatedCarets_();
    }

    private void SplitLineCaret_(Caret caret, Line line, Line toLine)
    {
        foreach (Caret c in _carets)
        {
            if (c.Position.LineIndex == caret.Position.LineIndex)
            {
                if (caret.Position.CharIndex <= c.Position.CharIndex)
                {
                    c.Position.LineIndex = toLine.Index;
                    c.Position.CharIndex -= line.CharsCount;
                    c.Position.Validate();
                }
            }

            if (c.AnchorPosition.LineIndex == caret.Position.LineIndex)
            {
                if (caret.Position.CharIndex <= c.AnchorPosition.CharIndex)
                {
                    c.AnchorPosition.LineIndex = toLine.Index;
                    c.AnchorPosition.CharIndex -= line.CharsCount;
                    c.AnchorPosition.Validate();
                }
            }

            Logger.Info("SplitLineCaret: " + c.Position.LineIndex + " " + c.Position.CharIndex);
        }

        caret.RemoveSelection();
        RemoveDuplicatedCarets_();
    }

    private bool IsLineHasCaret_(int lineIndex)
    {
        foreach (Caret caret in _carets)
        {
            if (caret.Position.LineIndex == lineIndex)
                return true;
        }
        return false;
    }

    private void RemoveDuplicatedCarets_()
    {
        for (int i = _carets.Count - 1; i >= 0; i--)
        {
            Caret caret = _carets[i];
            caret.GetSorted(out Coordinates start, out Coordinates end);

            for (int j = i - 1; j >= 0; j--)
            {
                Caret priorityCaret = _carets[j];
                priorityCaret.GetSorted(out Coordinates priorityStart, out Coordinates priorityEnd);

                // not overlapped
                if (priorityStart.IsBiggerThanWithoutLineSub(end) || start.IsBiggerThanWithoutLineSub(priorityEnd))
                    continue;

                // overlapped
                _carets.RemoveAt(i);
                Logger.Info("Overlapped caret has been removed");
            }
        }
        Singleton.TextBox.CaretBlinkStopwatch.Restart();
    }
}