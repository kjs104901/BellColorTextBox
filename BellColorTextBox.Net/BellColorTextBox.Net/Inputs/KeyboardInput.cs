namespace Bell.Inputs;

[Flags]
public enum HotKeys : uint
{
    None = 0,
    
    Shift = 1 << 0,
    Ctrl = 1 << 1,
    Alt = 1 << 2,
    
    A = 1 << 3,
    C = 1 << 4,
    V = 1 << 5,
    X = 1 << 6,
    Y = 1 << 7,
    Z = 1 << 8,
    
    UpArrow = 1 << 9,
    DownArrow = 1 << 10,
    LeftArrow = 1 << 11,
    RightArrow = 1 << 12,
    
    PageUp = 1 << 13,
    PageDown = 1 << 14,
    Home = 1 << 15,
    End = 1 << 16,
    Insert = 1 << 17,
    
    Delete = 1 << 18,
    Backspace = 1 << 19,
    Enter = 1 << 20,
    Tab = 1 << 21,
    
    F3 = 1 << 22,
    Escape = 1 << 23,
}

public struct KeyboardInput
{
    public HotKeys HotKeys;
    public List<char> Chars;
    public string ImeComposition;
}