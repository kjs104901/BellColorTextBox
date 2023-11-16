namespace Bell.Data;

internal enum CaretMove
{
    Up,
    Down,
    Left,
    Right,

    StartOfFile,
    EndOfFile,
    StartOfLine,
    EndOfLine,
    StartOfWord,
    EndOfWord,

    PageUp,
    PageDown
}

internal class Caret
{
    internal Coordinates AnchorPosition;
    internal Coordinates Position;

    internal bool HasSelection => !AnchorPosition.IsSameAs(Position);

    internal void GetSorted(out Coordinates start, out Coordinates end)
    {
        if (Position.IsBiggerThan(AnchorPosition))
        {
            start = AnchorPosition;
            end = Position;
        }
        else
        {
            start = Position;
            end = AnchorPosition;
        }
    }
    
    internal void RemoveSelection()
    {
        AnchorPosition = Position;
    }

    internal Caret Clone() => new() { AnchorPosition = AnchorPosition, Position = Position };

    internal static readonly Caret None = new Caret();
}