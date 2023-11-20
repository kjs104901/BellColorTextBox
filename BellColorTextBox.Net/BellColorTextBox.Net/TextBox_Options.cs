using Bell.Data;
using Bell.Languages;
using Bell.Managers;
using Bell.Themes;
using Bell.Utils;

namespace Bell;

public enum WrapMode
{
    None,
    Word,
    BreakWord
}

public enum EolMode
{
    CRLF,
    LF,
    CR
}

public enum TabMode
{
    Space,
    Tab
}

public partial class TextBox
{
    public bool AutoIndent { get; set; } = true;
    public bool AutoComplete { get; set; } = true; // TODO
    public readonly List<string> AutoCompleteList = new();
    
    public bool Overwrite { get; set; } = false; // TODO
    public bool ReadOnly { get; set; } = false;
    
    private WrapMode _wrapMode = WrapMode.None;
    public WrapMode WrapMode
    {
        get => _wrapMode;
        set
        {
            if (_wrapMode == value)
                return;

            _wrapMode = value;
            foreach (Line line in LineManager.Lines)
            {
                line.SetCutoffsDirty();
            }
            RowManager.SetRowCacheDirty();
        }
    }

    private bool _wordWrapIndent = true;

    public bool WordWrapIndent
    {
        get => _wordWrapIndent;
        set
        {
            if (_wordWrapIndent == value)
                return;

            _wordWrapIndent = value;
            foreach (Line line in LineManager.Lines)
            {
                line.SetCutoffsDirty();
            }
            RowManager.SetRowCacheDirty();
        }
    }

    public EolMode EolMode = EolMode.LF;
    
    public bool SyntaxHighlight { get; set; } = true;
    public bool SyntaxFolding { get; set; } = true;
    
    public bool ShowingSpace { get; set; } = true;
    public bool ShowingTab { get; set; } = true;
    
    public float LeadingHeight { get; set; } = 1.2f;
    
    private TabMode _tabMode = TabMode.Tab;
    public TabMode TabMode
    {
        get => _tabMode;
        set
        {
            if (_tabMode == value)
                return;

            _tabMode = value;
            _tabStringCache.SetDirty();
        }
    }
    
    private int _tabSize = 4;
    public int TabSize
    {
        get => _tabSize;
        set
        {
            if (_tabSize == value)
                return;

            _tabSize = value;
            _tabStringCache.SetDirty();
        }
    }
    
    private Language _language = Language.CSharp();
    public Language Language
    {
        get => _language;
        set
        {
            if (_language == value)
                return;

            _language = value;
            LineManager.SetLanguageTokenDirty();
        }
    }

    public Theme Theme = Theme.Dark();

    public bool IsDebugMode { get; set; } = false;
}

public partial class TextBox
{
    internal string TabString => _tabStringCache.Get();
    private readonly Cache<string> _tabStringCache;
    
    internal const int SyntaxGiveUpThreshold = 3000;

    internal int CountTabStart(string line)
    {
        int count = 0;
        for (int i = 0; i < line.Length; i++)
        {
            for (int j = 0; j < TabString.Length; j++)
            {
                if (i + j >= line.Length)
                    return count;
                
                char lineChar = line[i + j];
                char tabChar = TabString[j];
                
                if (lineChar != tabChar)
                    return count;
            }
            count++;
            i += TabString.Length;
            i -= 1; // To ignore the i++ in the for loop
        }
        return count;
    }

    private string UpdateTabString(string _)
    {
        if (TabMode.Space == TabMode)
            return new string(' ', TabSize);
        return "\t";
    }
    
    internal string ReplaceTab(string text)
    {
        if (TabMode == TabMode.Space && text.Contains('\t'))
        {
            return text.Replace("\t", new string(' ', TabSize));
        }
        else if (TabMode == TabMode.Tab && text.Contains(new string(' ', TabSize)))
        {
            return text.Replace(new string(' ', TabSize), "\t");
        }

        return text;
    }

    internal string ReplaceEol(string text)
    {
        if (text.Contains('\r'))
        {
            return text.Replace("\r\n", "\n")
                .Replace("\r", "\n");
        }
        return text;
    }

    internal string GetEolString()
    {
        switch (EolMode)
        {
            case EolMode.CRLF:
                return "\r\n";
            case EolMode.LF:
                return "\n";
            case EolMode.CR:
                return "\r";
        }
        return "\n";
    }
}