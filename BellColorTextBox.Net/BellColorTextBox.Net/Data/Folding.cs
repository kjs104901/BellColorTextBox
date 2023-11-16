namespace Bell.Data;

internal class Folding
{
    internal int Start;
    internal int End;
    
    internal bool Folded;

    internal void Switch()
    {
        Folded = !Folded;
    }
    
    internal static readonly Folding None = new Folding() { Start = -1, End = -1, Folded = false };
}