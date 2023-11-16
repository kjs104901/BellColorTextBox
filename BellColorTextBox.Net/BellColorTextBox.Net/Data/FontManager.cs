using Bell.Utils;

namespace Bell.Data;

// Interface
internal partial class FontManager
{
    internal static void UpdateReferenceSize() => Singleton.TextBox.FontManager.UpdateReferenceSize_();
    internal static float GetFontReferenceWidth() => Singleton.TextBox.FontManager.GetFontReferenceWidth_();
    internal static float GetFontWhiteSpaceWidth() => Singleton.TextBox.FontManager.GetFontWhiteSpaceWidth_();
    internal static float GetFontTabWidth() => Singleton.TextBox.FontManager.GetFontTabWidth_();
    internal static float GetFontNumberWidth() => Singleton.TextBox.FontManager.GetFontNumberWidth_();
    internal static float GetFontWidth(char c) => Singleton.TextBox.FontManager.GetFontWidth_(c);
    internal static float GetFontWidth(string str) => Singleton.TextBox.FontManager.GetFontWidth_(str);
    internal static float GetLineHeight() => Singleton.TextBox.FontManager.GetLineHeight_();
    internal static float GetLineHeightOffset() => Singleton.TextBox.FontManager.GetLineHeightOffset_();
}

// Implementation
internal partial class FontManager
{
    private readonly Dictionary<float, Font> _fontDictionary = new();

    private Font _fontCache = new(10.0f);

    private void UpdateReferenceSize_()
    {
        var fontSize = Singleton.TextBox.Backend.GetFontSize();
        if (false == _fontDictionary.ContainsKey(fontSize))
            _fontDictionary.TryAdd(fontSize, new Font(fontSize));
        _fontCache = _fontDictionary[fontSize];
    }

    private float GetFontReferenceWidth_() => GetFontWidth('#');
    private float GetFontWhiteSpaceWidth_() => GetFontWidth(' ');
    private float GetFontTabWidth_() => GetFontWidth(' ') * Singleton.TextBox.TabSize;
    private float GetFontNumberWidth_() => GetFontWidth('0');

    private float GetFontWidth_(char c)
    {
        if (c == '\t')
            return GetFontWhiteSpaceWidth() * Singleton.TextBox.TabSize;
        
        return _fontCache.GetFontWidth(c);
    }
    
    private float GetFontWidth_(string str)
    {
        return str.Sum(GetFontWidth_);
    }

    private float GetLineHeight_()
    {
        return _fontCache.Size * Singleton.TextBox.LeadingHeight;
    }

    private float GetLineHeightOffset_()
    {
        return ((_fontCache.Size * Singleton.TextBox.LeadingHeight) - _fontCache.Size) / 2.0f;
    }
}