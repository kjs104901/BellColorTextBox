using Bell.Data;
using Bell.Themes;

namespace Bell.Languages;

public partial class Language
{
    public static Language Json()
    {
        Language language = new();

        language.AddFolding("{", "}");
        language.AddFolding("[", "]");

        // String
        language.AddPattern(@"""[^""]*""",
            Theme.Token.String);
        
        // Key
        language.AddPattern(@"(?<range>""([^\\""]|\\"")*"")\s*:",
            Theme.Token.Variable);
        
        // Number
        language.AddPattern(@"-?\b\d+(\.\d+)?([eE][+-]?\d+)?\b",
            Theme.Token.Numeric);
        
        // Constant
        language.AddPattern(@"\b(true|false|null)\b",
            Theme.Token.Constant);

        return language;
    }
}