using Bell.Data;
using Bell.Themes;

namespace Bell.Languages;

public partial class Language
{
    public static Language CSharpStack()
    {
        Language language = new();

        // File name
        language.AddPattern(@"(?<=in )(?<file>.*?)(?=:line)",
            Theme.Token.Comment);
        
        // Line info
        language.AddPattern(@":line\s+\d+",
            Theme.Token.Constant);
        
        // Namespace
        language.AddPattern(@"(?<=at\s+)([\w.]+)(?=\.[^.(]+)",
            Theme.Token.Namespace);
        
        // Function
        language.AddPattern(@"(?:\.|>)([^>.]+)(?=\()",
            Theme.Token.Function);
        
        // Param
        language.AddPattern( @"\(([^)]+)\)",
            Theme.Token.Variable);
        
        // Class
        language.AddPattern(@"\b(class|struct|enum|interface)\s+(?<range>\w+?)\b",
            Theme.Token.Type);
        
        // Function Call
        language.AddPattern(@"\b\w+(?=\s*\()",
            Theme.Token.Function);
        
        // Keyword
        language.AddPattern(@"\b(public|private|protected|internal|class|struct|interface|enum|delegate|int|long|float|double|bool|char|string|object|byte|sbyte|short|ushort|uint|ulong|decimal|lock|new|override|virtual|abstract|sealed|static|readonly|extern|ref|out|in|params|using|namespace|true|false|null|this|base|operator|sizeof|typeof|stackalloc|nameof|async|await|volatile|unchecked|checked|unsafe|fixed|void|get|set)\b",
            Theme.Token.Keyword);
        
        return language;
    }
}