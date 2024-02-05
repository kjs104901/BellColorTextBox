using Bell.Data;
using Bell.Managers;

namespace Bell;

public partial class TextBox
{
    public void FoldAll()
    {
        Ins = this;
        foreach (Folding folding in LineManager.FoldingList)
        {
            folding.Folded = true;
        }
        RowManager.SetRowCacheDirty();
    }

    public void UnfoldAll()
    {
        Ins = this;
        foreach (Folding folding in LineManager.FoldingList)
        {
            folding.Folded = false;
        }
        RowManager.SetRowCacheDirty();
    }
    
    public void Focus(int line)
    {
        Ins = this;
        ScrollManager.Focus(line);
    }
    
    public void ScrollTo(int line)
    {
        Ins = this;
        ScrollManager.ScrollTo(line);
    }
    
    public void Search(string text, StringComparison comparer = StringComparison.Ordinal)
    {
        Ins = this;
        SearchManager.Search(text, comparer);
    }
    
    public void SearchPrevious()
    {
        Ins = this;
        SearchManager.SearchPrevious();
    }
    
    public void SearchNext()
    {
        Ins = this;
        SearchManager.SearchNext();
    }
    
    public void ResetSearch()
    {
        Ins = this;
        SearchManager.Reset();
    }
}