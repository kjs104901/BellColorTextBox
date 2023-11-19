using Bell.Utils;

namespace Bell.Data;

// Interface
internal partial class FontManager
{
    internal static void UpdateReferenceSize() => TextBox.Ins.FontManager.UpdateReferenceSize_();
    internal static float GetFontReferenceWidth() => TextBox.Ins.FontManager.GetFontReferenceWidth_();
    internal static float GetFontWhiteSpaceWidth() => TextBox.Ins.FontManager.GetFontWhiteSpaceWidth_();
    internal static float GetFontTabWidth() => TextBox.Ins.FontManager.GetFontTabWidth_();
    internal static float GetFontNumberWidth() => TextBox.Ins.FontManager.GetFontNumberWidth_();
    internal static float GetFontWidth(char c) => TextBox.Ins.FontManager.GetFontWidth_(c);
    internal static float GetFontWidth(string str) => TextBox.Ins.FontManager.GetFontWidth_(str);
    internal static float GetLineHeight() => TextBox.Ins.FontManager.GetLineHeight_();
    internal static float GetLineHeightOffset() => TextBox.Ins.FontManager.GetLineHeightOffset_();
}

// Implementation
internal partial class FontManager
{
    private readonly Dictionary<float, Font> _fontDictionary = new();

    private Font _fontCache = new(10.0f);

    private void UpdateReferenceSize_()
    {
        var fontSize = TextBox.Ins.Backend.GetFontSize();
        if (false == _fontDictionary.ContainsKey(fontSize))
            _fontDictionary.TryAdd(fontSize, new Font(fontSize));
        _fontCache = _fontDictionary[fontSize];
    }

    private float GetFontReferenceWidth_() => GetFontWidth('#');
    private float GetFontWhiteSpaceWidth_() => GetFontWidth(' ');
    private float GetFontTabWidth_() => GetFontWidth(' ') * TextBox.Ins.TabSize;
    private float GetFontNumberWidth_() => GetFontWidth('0');

    private float GetFontWidth_(char c)
    {
        if (c == '\t')
            return GetFontWhiteSpaceWidth() * TextBox.Ins.TabSize;
        
        return _fontCache.GetFontWidth(c);
    }
    
    private float GetFontWidth_(string str)
    {
        return str.Sum(GetFontWidth_);
    }

    private float GetLineHeight_()
    {
        return _fontCache.Size * TextBox.Ins.LeadingHeight;
    }

    private float GetLineHeightOffset_()
    {
        return ((_fontCache.Size * TextBox.Ins.LeadingHeight) - _fontCache.Size) / 2.0f;
    }
}