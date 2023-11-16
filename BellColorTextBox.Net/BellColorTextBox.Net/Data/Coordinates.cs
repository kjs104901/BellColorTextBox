using Bell.Utils;

namespace Bell.Data;

internal struct Coordinates
{
    internal int LineIndex;
    internal int CharIndex;
    internal int LineSubIndex;

    internal Coordinates(int lineIndex, int charIndex, int lineSubIndex = -1)
    {
        LineIndex = lineIndex;
        CharIndex = charIndex;
        LineSubIndex = lineSubIndex;
    }

    internal bool IsSameAs(Coordinates other)
    {
        if (false == LineManager.GetLineSub(this, out LineSub lineSub) ||
            false == LineManager.GetLineSub(other, out LineSub otherLineSub))
        {
            Logger.Error($"IsSameAs: failed to get line sub: {LineIndex},{CharIndex} {other.LineIndex},{other.CharIndex}");
            return false;
        }

        if (LineIndex != other.LineIndex)
            return false;

        if (lineSub.Coordinates.LineSubIndex != otherLineSub.Coordinates.LineSubIndex)
            return false;

        return CharIndex == other.CharIndex;
    }

    internal bool IsBiggerThan(Coordinates other)
    {
        if (false == LineManager.GetLineSub(this, out LineSub lineSub))
        {
            Logger.Error($"IsBiggerThan: failed to get line sub: {LineIndex}, {CharIndex}");
            return false;
        }
        
        if (false == LineManager.GetLineSub(other, out LineSub otherLineSub))
        {
            Logger.Error($"IsBiggerThan: failed to get line sub: {other.LineIndex}, {other.CharIndex}");
            return false;
        }

        if (LineIndex != other.LineIndex)
            return LineIndex > other.LineIndex;

        if (lineSub.Coordinates.LineSubIndex != otherLineSub.Coordinates.LineSubIndex)
            return lineSub.Coordinates.LineSubIndex > otherLineSub.Coordinates.LineSubIndex;

        return CharIndex > other.CharIndex;
    }
    
    internal bool IsBiggerThanWithoutLineSub(Coordinates other)
    {
        if (LineIndex != other.LineIndex)
            return LineIndex > other.LineIndex;

        return CharIndex > other.CharIndex;
    }

    internal bool IsValid()
    {
        if (false == LineManager.GetLine(LineIndex, out Line line))
            return false;

        if (CharIndex < 0 || CharIndex > line.CharsCount + 1)
            return false;

        return true;
    }

    internal void Validate()
    {
        if (LineIndex < 0)
        {
            Logger.Error($"Validate failed. LineIndex < 0. {LineIndex}:{CharIndex}");
            LineIndex = 0;
            return;
        }
        
        if (LineIndex >= LineManager.Lines.Count)
        {
            Logger.Error($"Validate failed. LineIndex > Lines.Count. {LineIndex}:{CharIndex}");
            LineIndex = LineManager.Lines.Count - 1;
            return;
        }

        if (false == LineManager.GetLine(LineIndex, out Line line))
        {
            Logger.Error($"Validate failed. GetLine. {LineIndex}:{CharIndex}");
            return;
        }
        
        if (CharIndex < 0)
        {
            Logger.Error($"Validate failed. CharIndex < 0. {LineIndex}:{CharIndex}");
            CharIndex = 0;
            return;
        }
        
        if (CharIndex > line.CharsCount + 1)
        {
            Logger.Error($"Validate failed. CharIndex > CharsCount. {LineIndex}:{CharIndex}");
            CharIndex = line.CharsCount;
            return;
        }
    }

    internal Coordinates FindMove(CaretMove caretMove, int count = 1)
    {
        Coordinates newCoordinates = this;
        for (int i = 0; i < count; i++)
        {
            newCoordinates = newCoordinates.FindMoveSingle(caretMove);
        }

        Logger.Info("FindMove: " + caretMove + " " + count + " " + newCoordinates.LineIndex + " " +
                    newCoordinates.CharIndex);

        if (IsValid())
        {
            if (IsSameAs(newCoordinates))
            {
                Logger.Warning($"FindMove: IsSameAs {caretMove} {count}");
            }
        }

        return newCoordinates;
    }

    internal Coordinates FindMoveSingle(CaretMove caretMove)
    {
        if (CaretMove.Right == caretMove)
        {
            if (false == LineManager.GetLine(LineIndex, out Line line))
                return this;

            // End of line
            if (line.CharsCount <= CharIndex)
            {
                // End of file
                if (false == LineManager.GetLine(LineIndex + 1, out Line nextLine))
                    return this;

                LineIndex = nextLine.Index;
                CharIndex = 0;
                LineSubIndex = -1;
                return this;
            }

            // Has next line sub
            if (LineManager.GetLineSub(this, out LineSub lineSub) &&
                CharIndex == lineSub.Coordinates.CharIndex + lineSub.Chars.Count &&
                LineManager.GetLineSub(LineIndex, lineSub.Coordinates.LineSubIndex + 1,
                    out LineSub nextLineSub))
            {
                LineSubIndex = nextLineSub.Coordinates.LineSubIndex;
                return this;
            }

            CharIndex++;
            return this;
        }

        if (CaretMove.Left == caretMove)
        {
            // Start of line
            if (CharIndex <= 0)
            {
                // Start of file
                if (false == LineManager.GetLine(LineIndex - 1, out Line prevLine))
                    return this;

                LineIndex = prevLine.Index;
                CharIndex = prevLine.CharsCount;
                LineSubIndex = -1;
                return this;
            }

            // Has prev line sub
            if (LineManager.GetLineSub(this, out LineSub lineSub) &&
                CharIndex == lineSub.Coordinates.CharIndex &&
                LineManager.GetLineSub(LineIndex, lineSub.Coordinates.LineSubIndex - 1,
                    out LineSub prevLineSub))
            {
                LineSubIndex = prevLineSub.Coordinates.LineSubIndex;
                return this;
            }

            CharIndex--;
            return this;
        }

        if (CaretMove.Up == caretMove)
        {
            // Has prev line sub
            if (LineManager.GetLineSub(this, out LineSub lineSub) &&
                lineSub.Coordinates.LineSubIndex > 0 &&
                LineManager.GetLineSub(LineIndex, lineSub.Coordinates.LineSubIndex - 1,
                    out LineSub prevLineSub))
            {
                float indentDiff = lineSub.IndentWidth - prevLineSub.IndentWidth;
                int subCharIndex = prevLineSub.GetCharIndex(lineSub.GetCharPosition(this) + indentDiff);
                CharIndex = prevLineSub.Coordinates.CharIndex + subCharIndex;
                LineSubIndex = lineSub.Coordinates.LineSubIndex - 1;
                return this;
            }

            // Start of file
            if (false == LineManager.GetLine(LineIndex - 1, out Line prevLine))
                return this;

            // Last line sub of prev line
            if (LineManager.GetLineSub(prevLine.Index, prevLine.LineSubs.Count - 1,
                    out LineSub prevLineLastLineSub))
            {
                float indentDiff = lineSub.IndentWidth - prevLineLastLineSub.IndentWidth;
                int subCharIndex = prevLineLastLineSub.GetCharIndex(lineSub.GetCharPosition(this) + indentDiff);
                LineIndex = prevLine.Index;
                CharIndex = prevLineLastLineSub.Coordinates.CharIndex + subCharIndex;
                LineSubIndex = prevLineLastLineSub.Coordinates.LineSubIndex;
                return this;
            }
            Logger.Error("Failed to find prev line sub");
            return this;
        }

        if (CaretMove.Down == caretMove)
        {
            // Has next line sub
            if (LineManager.GetLineSub(this, out LineSub lineSub) &&
                LineManager.GetLine(LineIndex, out Line line) &&
                lineSub.Coordinates.LineSubIndex + 1 < line.LineSubs.Count &&
                LineManager.GetLineSub(LineIndex, lineSub.Coordinates.LineSubIndex + 1,
                    out LineSub nextLineSub))
            {
                float indentDiff = lineSub.IndentWidth - nextLineSub.IndentWidth;
                int subCharIndex = nextLineSub.GetCharIndex(lineSub.GetCharPosition(this) + indentDiff);
                CharIndex = nextLineSub.Coordinates.CharIndex + subCharIndex;
                LineSubIndex = lineSub.Coordinates.LineSubIndex + 1;
                return this;
            }

            // End of file
            if (false == LineManager.GetLine(LineIndex + 1, out Line nextLine))
                return this;

            // First line sub of next line
            if (LineManager.GetLineSub(nextLine.Index, 0, out LineSub nextLineFirstLineSub))
            {
                float indentDiff = lineSub.IndentWidth - nextLineFirstLineSub.IndentWidth;
                int subCharIndex = nextLineFirstLineSub.GetCharIndex(lineSub.GetCharPosition(this) + indentDiff);
                LineIndex = nextLine.Index;
                CharIndex = nextLineFirstLineSub.Coordinates.CharIndex + subCharIndex;
                LineSubIndex = nextLineFirstLineSub.Coordinates.LineSubIndex;
                return this;
            }

            Logger.Error("Failed to find next line sub");
            return this;
        }

        if (CaretMove.StartOfLine == caretMove)
        {
            CharIndex = 0;
            LineSubIndex = -1;
            return this;
        }

        if (CaretMove.EndOfLine == caretMove)
        {
            if (false == LineManager.GetLine(LineIndex, out Line line))
                return this;

            CharIndex = line.CharsCount;
            LineSubIndex = -1;
            return this;
        }

        if (CaretMove.StartOfWord == caretMove)
        {
            if (false == LineManager.GetLine(LineIndex, out Line line))
                return this;

            if (false == line.GetWordStart(CharIndex, out int wordStart))
                return this;
            
            CharIndex = wordStart;
            return this;
        }

        if (CaretMove.EndOfWord == caretMove)
        {
            if (false == LineManager.GetLine(LineIndex, out Line line))
                return this;

            if (false == line.GetWordEnd(CharIndex, out int wordEnd))
                return this;
            
            CharIndex = wordEnd;
            return this;
        }

        if (CaretMove.StartOfFile == caretMove)
        {
            if (false == LineManager.GetLine(0, out Line startLine))
                return this;

            LineIndex = startLine.Index;
            CharIndex = 0;
            return this;
        }

        if (CaretMove.EndOfFile == caretMove)
        {
            if (false == LineManager.GetLine(LineManager.Lines.Count - 1, out Line lastLine))
                return this;

            LineIndex = lastLine.Index;
            CharIndex = lastLine.CharsCount;
            return this;
        }

        if (CaretMove.PageUp == caretMove)
        {
            int newLineIndex = LineIndex - Singleton.TextBox.LinesPerPage / 2;
            if (newLineIndex < 0)
                newLineIndex = 0;
            
            if (false == LineManager.GetLine(newLineIndex, out Line line))
                return this;

            LineIndex = newLineIndex;
            CharIndex = 0;
            return this;
        }

        if (CaretMove.PageDown == caretMove)
        {
            int newLineIndex = LineIndex + Singleton.TextBox.LinesPerPage / 2;
            if (newLineIndex >= LineManager.Lines.Count)
                newLineIndex = LineManager.Lines.Count - 1;
            
            if (false == LineManager.GetLine(newLineIndex, out Line line))
                return this;
            
            LineIndex = newLineIndex;
            CharIndex = 0;
            return this;
        }

        return this;
    }
}