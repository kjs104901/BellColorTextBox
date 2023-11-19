using Bell.Utils;

namespace Bell.Data;

internal class Row : IReusable
{
    internal RowSelection RowSelection => RowSelectionCache.Get();
    internal readonly Cache<RowSelection> RowSelectionCache;
    
    internal int LineIndex = -1;
    internal int LineSubIndex = -1;
    internal LineSub? LineSub
    {
        get
        {
            if (LineIndex < 0 || LineSubIndex < 0)
                return null;

            if (false == LineManager.GetLine(LineIndex, out Line line) || line.LineSubs.Count <= LineSubIndex)
                return null;
            
            return line.LineSubs[LineSubIndex];
        }
    }
    
    public void Reset()
    {
        LineIndex = -1;
        LineSubIndex = -1;
        RowSelectionCache.SetDirty();
    }

    public Row()
    {
        RowSelectionCache = new("Row Selection", new RowSelection()
            {
                CaretPositions = new()
            },
            UpdateRowSelection);
    }

    private RowSelection UpdateRowSelection(RowSelection rowSelection)
    {
        rowSelection.Selected = false;
        
        rowSelection.SelectionStart = 0.0f;
        rowSelection.SelectionEnd = 0.0f;

        rowSelection.SelectionStartChar = 0;
        rowSelection.SelectionEndChar = 0;
        
        rowSelection.CaretPositions.Clear();

        for (int i = 0; i < CaretManager.Count; i++)
        {
            Caret caret = CaretManager.GetCaret(i);

            if (false == LineManager.GetLineSub(caret.AnchorPosition, out LineSub anchorLineSub) ||
                false == LineManager.GetLineSub(caret.Position, out LineSub lineSub))
            {
                Logger.Error("UpdateLineSelection: failed to get line");
                continue;
            }

            caret.GetSorted(out Coordinates start, out Coordinates end);
            if (false == LineManager.GetLineSub(start, out LineSub startLineSub) ||
                false == LineManager.GetLineSub(end, out LineSub endLineSub))
            {
                Logger.Error("UpdateLineSelection: failed to get line");
                continue;
            }

            if (caret.HasSelection)
            {
                if (startLineSub == LineSub)
                {
                    float startPosition = LineSub.GetCharPosition(start);
                    if (endLineSub == LineSub)
                    {
                        float endPosition = LineSub.GetCharPosition(end);
                        rowSelection.SelectionStart = startPosition;
                        rowSelection.SelectionEnd = endPosition;
                        rowSelection.SelectionStartChar = start.CharIndex;
                        rowSelection.SelectionEndChar = end.CharIndex;
                        rowSelection.Selected = true;
                    }
                    else if (endLineSub.IsBiggerThan(LineSub))
                    {
                        rowSelection.SelectionStart = startPosition;
                        rowSelection.SelectionEnd = LineSub.CharWidths.Sum();
                        rowSelection.SelectionStartChar = start.CharIndex;
                        rowSelection.SelectionEndChar = LineSub.CharWidths.Count - 1;
                        if (rowSelection.SelectionEnd < 1.0f)
                        {
                            rowSelection.SelectionEnd = FontManager.GetFontWhiteSpaceWidth();
                        }

                        rowSelection.Selected = true;
                    }
                }
                else if (null != LineSub && LineSub.IsBiggerThan(startLineSub))
                {
                    if (endLineSub == LineSub)
                    {
                        float endPosition = LineSub.GetCharPosition(end);
                        rowSelection.SelectionStart = 0.0f;
                        rowSelection.SelectionEnd = endPosition;
                        rowSelection.SelectionStartChar = 0;
                        rowSelection.SelectionEndChar = end.CharIndex;
                        rowSelection.Selected = true;
                    }
                    else if (endLineSub.IsBiggerThan(LineSub))
                    {
                        rowSelection.SelectionStart = 0.0f;
                        rowSelection.SelectionEnd = LineSub.CharWidths.Sum();
                        rowSelection.SelectionStartChar = 0;
                        rowSelection.SelectionEndChar = LineSub.CharWidths.Count - 1;
                        if (rowSelection.SelectionEnd < 1.0f)
                        {
                            rowSelection.SelectionEnd = FontManager.GetFontWhiteSpaceWidth();
                        }

                        rowSelection.Selected = true;
                    }
                }
            }

            if (lineSub == LineSub)
            {
                rowSelection.CaretPositions.Add(caret.Position);
            }
        }

        return rowSelection;
    }
}

internal struct RowSelection
{
    internal bool Selected;
    
    internal float SelectionStart;
    internal float SelectionEnd;
    
    internal int SelectionStartChar;
    internal int SelectionEndChar;

    internal List<Coordinates> CaretPositions;
}