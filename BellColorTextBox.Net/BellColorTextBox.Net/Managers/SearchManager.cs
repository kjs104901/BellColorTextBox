using Bell.Data;

namespace Bell.Managers;

internal partial class SearchManager
{
    internal static int SearchCount => TextBox.Ins.SearchManager._searchResults.Count;
    internal static int SearchIndex => TextBox.Ins.SearchManager._searchIndex;
    internal static SearchResult GetResult(int i) => TextBox.Ins.SearchManager._searchResults[i];
    internal static void Search(string text, StringComparison comparer = StringComparison.Ordinal) => TextBox.Ins.SearchManager.Search_(text, comparer);
    internal static void SearchPrevious() => TextBox.Ins.SearchManager.SearchPrevious_();
    internal static void SearchNext() => TextBox.Ins.SearchManager.SearchNext_();
    internal static void Reset() => TextBox.Ins.SearchManager.Reset_();
}

internal partial class SearchManager : IManager
{
    private string _searchText = string.Empty;
    private readonly List<SearchResult> _searchResults = new List<SearchResult>();
    private int _searchIndex = -1;
    
    private void Search_(string text, StringComparison comparer)
    {
        _searchResults.Clear();
        RowManager.SetRowSearchDirty();
        
        if (string.IsNullOrEmpty(text))
            return;
        
        _searchText = text;

        for (int i = 0; i < LineManager.Lines.Count; i++)
        {
            Line line = LineManager.Lines[i];
            
            var index = line.String.IndexOf(text, comparer);
            if (index < 0)
                continue;
            
            SearchResult result = new SearchResult();
            result.StartPosition = new Coordinates( i, index);
            result.EndPosition = new Coordinates( i, index + text.Length);
            _searchResults.Add(result);
        }
        
        if (_searchResults.Count > 0)
        {
            SearchSelect_(0);
        }
    }

    private void SearchPrevious_()
    {
        _searchIndex--;
        if (_searchIndex < 0)
            _searchIndex = _searchResults.Count - 1;
        SearchSelect_(_searchIndex);
    }
    
    private void SearchNext_()
    {
        _searchIndex++;
        if (_searchIndex > _searchResults.Count - 1)
            _searchIndex = 0;
        SearchSelect_(_searchIndex);
    }

    private void SearchSelect_(int index)
    {
        if (index < 0 || index > _searchResults.Count - 1)
            return;

        _searchIndex = index;
        SearchResult searchResult = _searchResults[_searchIndex];
        
        LineManager.Unfold(searchResult.StartPosition.LineIndex);
        RowManager.SetRowCacheDirty();
        ScrollManager.Focus(searchResult.StartPosition.LineIndex);
    }
    
    private void Reset_()
    {
        _searchText = string.Empty;
        _searchResults.Clear();
        _searchIndex = -1;
    }
    
    public void Tick() { }
}