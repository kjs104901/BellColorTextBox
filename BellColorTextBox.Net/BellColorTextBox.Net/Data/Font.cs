using Bell.Utils;

namespace Bell.Data;

public class Font
{
    public float Size { get; }
    private readonly Dictionary<char, float> _sizeWidthCache = new();

    public Font(float size)
    {
        Size = size;
    }

    public float GetFontWidth(char c)
    {
        if (false == _sizeWidthCache.TryGetValue(c, out float fontWidth))
        {
            fontWidth = TextBox.Ins.Backend.GetCharWidth(c);
            _sizeWidthCache[c] = fontWidth;
        }
        return fontWidth;
    }
}