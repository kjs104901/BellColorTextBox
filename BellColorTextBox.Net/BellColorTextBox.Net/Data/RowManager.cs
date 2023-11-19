using Bell.Utils;

namespace Bell.Data;

// Interface
internal partial class RowManager
{
    internal static List<Row> Rows => Singleton.TextBox.RowManager._rowsCache.Get();
    internal static void SetRowCacheDirty() => Singleton.TextBox.RowManager.SetRowCacheDirty_();
    internal static void SetRowSelectionDirty() => Singleton.TextBox.RowManager.SetRowSelectionDirty_();
}

// Implementation
internal partial class RowManager
{
    private readonly List<Row> _rows;
    private readonly Cache<List<Row>> _rowsCache;

    internal RowManager()
    {
        _rows = new();
        _rowsCache = new Cache<List<Row>>("Rows", _rows, UpdateRows);
    }

    private List<Row> UpdateRows(List<Row> rows)
    {
        Singleton.TextBox.RowPool.Return(rows);
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
                foreach (var lineSub in line.LineSubs)
                {
                    Row row = Singleton.TextBox.RowPool.Get();
                    row.LineSub = lineSub;
                    
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
        foreach (Row row in _rows)
        {
            row.RowSelectionCache.SetDirty();
        }
    }
}