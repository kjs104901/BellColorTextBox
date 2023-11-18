using Bell.Data;
using Bell.Themes;

namespace Bell.Languages;

public partial class Language
{
    public static Language CSharp()
    {
        Language language = new();
        
        language.AddFolding("{", "}");
        language.AddFolding("#region", "#endregion");
        language.AddFolding("#if", "#endif");

        language.AddLineComment("//");

        language.AddBlockComment("/*", "*/");

        language.AddString("'");
        language.AddString("\"");
        language.AddString("\"\"\"\"");

        // TODO fix this
        //language.AddMultilineString("@\"", "\"");
        //language.AddMultilineString("$@\"", "\"");
        //language.AddMultilineString("@$\"", "\"");

        // Number
        language.AddPattern(@"(\b0x[0-9a-fA-F]+\b|\b0b[01]+\b|\b0[0-7]+\b|\b\d+(\.\d+)?([eE][-+]?\d+)?[lLdDfF]?\b)",
            Theme.Token.Numeric);
        
        // Attribute
        language.AddPattern(@"\[\s*[a-zA-Z0-9_\.]+\s*(\([^\)]*\))?\s*\]",
            Theme.Token.Attribute);
        
        // Class
        language.AddPattern(@"\b(class|struct|enum|interface)\s+(?<range>\w+?)\b",
            Theme.Token.Type);
        
        // Function
        language.AddPattern(@"(?<=\b(public|private|protected|internal|static|virtual|override|abstract)\s+[\w<>\[\]]+\s+)\w+(?=\s*\()",
            Theme.Token.Function);
        
        // Function Call
        language.AddPattern(@"\b\w+(?=\s*\()",
            Theme.Token.Function);

        // Keyword
        language.AddPattern(@"\b(public|private|protected|internal|class|struct|interface|enum|delegate|int|long|float|double|bool|char|string|object|byte|sbyte|short|ushort|uint|ulong|decimal|lock|new|override|virtual|abstract|sealed|static|readonly|extern|ref|out|in|params|using|namespace|true|false|null|this|base|operator|sizeof|typeof|stackalloc|nameof|async|await|volatile|unchecked|checked|unsafe|fixed|void|get|set|Byte|SByte|Short|UShort|Int|UInt|Long|ULong|Float|Double|Decimal)\b",
            Theme.Token.Keyword);
        
        // Keyword control
        language.AddPattern(@"\b(if|else|switch|case|do|for|foreach|while|break|continue|return|goto|throw|try|catch|finally)\b",
            Theme.Token.KeywordControl);
        
        return language;
    }
}