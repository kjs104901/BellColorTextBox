using Bell.Data;
using Bell.Themes;

namespace Bell.Languages;

public partial class Language
{
    public static Language Proto()
    {
        Language language = new();

        language.AddFolding("{", "}");
        
        language.AddString("\"");
        
        // Message Name
        language.AddPattern(@"\b(message|enum)\s+(?<range>\w+?)\b",
            Theme.Token.Type);
        
        // Keyword
        language.AddPattern(@"\b(syntax|import|package|option|message|enum|service|rpc|returns|stream|map|oneof|reserved|extend|extensions|to|max|public|weak|proto3)\b",
            Theme.Token.Keyword);
        
        // Keyword
        language.AddPattern(@"\b(double|float|int32|int64|uint32|uint64|sint32|sint64|fixed32|fixed64|sfixed32|sfixed64|bool|string|bytes)\b",
            Theme.Token.Keyword);
        
        // Custom Keyword
        language.AddPattern(@"\b(Double|Single|Int32|Int64|UInt32|UInt64|Boolean|String|Byte|Dictionary|List)\b",
            Theme.Token.Keyword);
        
        // Modifiers
        language.AddPattern(@"\b(repeated|optional|required)\b",
            Theme.Token.Attribute);
        
        // Number
        language.AddPattern(@"\b\d+[\.]?\d*\b",
            Theme.Token.Numeric);
        
        return language;
    }
}