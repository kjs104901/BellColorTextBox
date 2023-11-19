using Bell.Data;
using Bell.Utils;

namespace Bell.Managers;

// Interface
internal partial class RowManager
{
    internal static List<Row> Rows => TextBox.Ins.RowManager._rowsCache.Get();
    internal static void SetRowCacheDirty() => TextBox.Ins.RowManager.SetRowCacheDirty_();
    internal static void SetRowSelectionDirty() => TextBox.Ins.RowManager.SetRowSelectionDirty_();
}

// Implementation
internal partial class RowManager  : IManager
{
    private readonly List<Row> _rows;
    private readonly Cache<List<Row>> _rowsCache;

    private bool _isRowSelectionDirty = true;

    internal RowManager()
    {
        _rows = new();
        _rowsCache = new Cache<List<Row>>("Rows", _rows, UpdateRows);
    }

    private List<Row> UpdateRows(List<Row> rows)
    {
        TextBox.Ins.RowPool.Return(rows);
        rows.Clear();

        int foldingCount = 0;
        foreach (Line line in LineManager.Lines)
        {
            bool visible = true;

            line.Folding = Folding.None;
            foreach (Folding folding in LineManager.FoldingList)
            {
                if (folding.End == line.Index)
                {
                    foldingCount--;
                }

                if (folding.Start < line.Index && line.Index < folding.End)
                {
                    if (folding.Folded)
                    {
                        visible = (0 == foldingCount);
                        break;
                    }
                }

                if (folding.Start == line.Index)
                {
                    line.Folding = folding;
                    foldingCount++;
                }
            }

            if (visible)
            {
                for (int i = 0; i < line.LineSubs.Count; i++)
                {
                    Row row = TextBox.Ins.RowPool.Get();
                    row.LineIndex = line.Index;
                    row.LineSubIndex = i;
                    rows.Add(row);
                }
            }
        }
        return rows;
    }

    private void SetRowCacheDirty_()
    {
        _rowsCache.SetDirty();
    }
    
    private void SetRowSelectionDirty_()
    {
        _isRowSelectionDirty = true;
    }

    public void Tick()
    {
        if (false == _isRowSelectionDirty)
            return;
        
        foreach (Row row in _rows)
        {
            row.RowSelectionCache.SetDirty();
        }
        _isRowSelectionDirty = false;
    }
}