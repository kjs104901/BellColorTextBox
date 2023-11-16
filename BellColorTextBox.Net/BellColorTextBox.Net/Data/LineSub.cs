using Bell.Utils;

namespace Bell.Data;

internal class LineSub : IReusable
{
    internal Coordinates Coordinates;
    internal float IndentWidth;
    internal float Width;

    internal readonly List<char> Chars = new();
    internal readonly List<float> CharWidths = new();
    
    internal static readonly LineSub None = new()
    {
        Coordinates = new Coordinates(-1, -1, -1),
        IndentWidth = 0.0f,
        Width = 0.0f,
    };
    
    public void Reset()
    {
        Coordinates = new Coordinates(-1, -1, -1);
        IndentWidth = 0.0f;
        Width = 0.0f;
        
        Chars.Clear();
        CharWidths.Clear();
    }
    
    internal int GetCharIndex(float position)
    {
        float current = 0.0f;
        for (var i = 0; i < CharWidths.Count; i++)
        {
            float charWidth = CharWidths[i];
            current += charWidth;
            if (current > position + charWidth * 0.5f)
                return i;
        }
        return CharWidths.Count;
    }

    internal float GetCharPosition(Coordinates coordinates)
    {
        int index = coordinates.CharIndex - Coordinates.CharIndex;
        if (index < 0)
            return 0.0f;
        
        float position = 0.0f;
        for (var i = 0; i < CharWidths.Count; i++)
        {
            if (i == index)
                break;
            position += CharWidths[i];
        }
        return position;
    }

    internal bool IsBiggerThan(LineSub other)
    {
        if (Coordinates.LineIndex != other.Coordinates.LineIndex)
            return Coordinates.LineIndex > other.Coordinates.LineIndex;
        return Coordinates.LineSubIndex > other.Coordinates.LineSubIndex;
    }
}