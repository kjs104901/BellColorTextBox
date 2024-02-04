namespace Bell.Themes;


public class Theme
{
    // Background
    public ColorStyle Background;
    public ColorStyle BackgroundDimmed;
    public ColorStyle BackgroundSelection;
    public ColorStyle BackgroundSearch;
    public ColorStyle BackgroundSearchSelection;

    // Foreground
    public ColorStyle Foreground;
    public ColorStyle ForegroundDimmed;

    // Token colors
    public enum Token
    {
        Comment,
        Constant,
        Attribute,
        Invalid,
        String,
        Keyword,
        KeywordControl,
        Function,
        Namespace,
        Type,
        Numeric,
        Variable
    }
    public Dictionary<Token, ColorStyle> TokenColors = new();

    public static Theme Dark()
    {
        return new Theme()
        {
            Background                      = new ColorStyle("#1E1E1E",       isSrgb: true),
            BackgroundDimmed                = new ColorStyle("#2B2B2B",       isSrgb: true),
            BackgroundSelection             = new ColorStyle("#ADD6FF26",     isSrgb: true),
            BackgroundSearch                = new ColorStyle("#613214",     isSrgb: true),
            BackgroundSearchSelection       = new ColorStyle("#EEEEEE",     isSrgb: true),
            
            Foreground                      = new ColorStyle("#D4D4D4",       isSrgb: true),
            ForegroundDimmed                = new ColorStyle("#767676",       isSrgb: true),
            
            TokenColors = new Dictionary<Token, ColorStyle>()
            {
                { Token.Comment,           new ColorStyle("#6A9955",    isSrgb: true) },
                { Token.Constant,          new ColorStyle("#569CD6",    isSrgb: true) },
                { Token.Attribute,         new ColorStyle("#D7BA7D",    isSrgb: true) },
                { Token.Invalid,           new ColorStyle("#F44747",    isSrgb: true) },
                { Token.String,            new ColorStyle("#CE9178",    isSrgb: true) },
                { Token.Keyword,           new ColorStyle("#569CD6",    isSrgb: true) },
                { Token.KeywordControl,    new ColorStyle("#C586C0",    isSrgb: true) },
                { Token.Function,          new ColorStyle("#DCDCAA",    isSrgb: true) },
                { Token.Namespace,         new ColorStyle("#4EC9B0",    isSrgb: true) },
                { Token.Type,              new ColorStyle("#4EC9B0",    isSrgb: true) },
                { Token.Numeric,           new ColorStyle("#B5CEA8",    isSrgb: true) },
                { Token.Variable,          new ColorStyle("#9CDCFE",    isSrgb: true) }
            }
        };
    }
}