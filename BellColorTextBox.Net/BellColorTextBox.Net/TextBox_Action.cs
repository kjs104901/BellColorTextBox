using Bell.Data;
using Bell.Managers;

namespace Bell;

public partial class TextBox
{
    public void FoldAll()
    {
        foreach (Folding folding in LineManager.FoldingList)
        {
            folding.Folded = true;
        }
        RowManager.SetRowCacheDirty();
    }

    public void UnfoldAll()
    {
        foreach (Folding folding in LineManager.FoldingList)
        {
            folding.Folded = false;
        }
        RowManager.SetRowCacheDirty();
    }
    
    public void Focus(int line)
    {
        ScrollManager.Focus(line);
    }
    
    public void ScrollTo(int line)
    {
        ScrollManager.ScrollTo(line);
    }
    
    public void Search(string text, StringComparison comparer = StringComparison.Ordinal)
    {
        SearchManager.Search(text, comparer);
    }
    
    public void SearchPrevious()
    {
        SearchManager.SearchPrevious();
    }
    
    public void SearchNext()
    {
        SearchManager.SearchNext();
    }
    
    public void ResetSearch()
    {
        SearchManager.Reset();
    }
}