using Bell.Actions;
using Bell.Data;
using Bell.Languages;
using Bell.Utils;

namespace Bell.Managers;

// Interface
internal partial class LineManager
{
    internal static List<Line> Lines => TextBox.Ins.LineManager._lines;
    internal static List<Folding> FoldingList => TextBox.Ins.LineManager._foldingList;

    internal static bool GetLine(int lineIndex, out Line line) =>
        TextBox.Ins.LineManager.GetLine_(lineIndex, out line);

    internal static bool GetLineSub(int lineIndex, int lineSubIndex, out LineSub lineSub) =>
        TextBox.Ins.LineManager.GetLineSub_(lineIndex, lineSubIndex, out lineSub);

    internal static bool GetLineSub(Coordinates coordinates, out LineSub lineSub) =>
        TextBox.Ins.LineManager.GetLineSub_(coordinates, out lineSub);

    internal static Line InsertLine(int lineIndex) =>
        TextBox.Ins.LineManager.InsertLine_(lineIndex);

    internal static void RemoveLine(int removeLineIndex) =>
        TextBox.Ins.LineManager.RemoveLine_(removeLineIndex);

    internal static void SetLanguageTokenDirty() => TextBox.Ins.LineManager._languageTokenCache.SetDirty();

    internal static void ShiftFoldingLine(int lineIndex, EditDirection direction) =>
        TextBox.Ins.LineManager.ShiftFoldingLine_(lineIndex, direction);

    internal static void Unfold(int lineIndex) =>
        TextBox.Ins.LineManager.Unfold_(lineIndex);

    internal static float GetMaxLineWidth() =>
        TextBox.Ins.LineManager.GetMaxLineWidth_();
}

// Implementation
internal partial class LineManager : IManager
{
    private readonly List<Line> _lines = new();

    private readonly List<Folding> _foldingList = new();
    private readonly List<Folding> _foldedList = new();
    private readonly Dictionary<int, Stack<int>> _foldingStacks = new();

    private readonly Cache<Stack<Language.Token>> _languageTokenCache;

    internal LineManager()
    {
        _languageTokenCache =
            new Cache<Stack<Language.Token>>("Language Token", new Stack<Language.Token>(),
                UpdateLanguageToken_, 100);
    }

    private bool GetLine_(int lineIndex, out Line line)
    {
        if (0 <= lineIndex && lineIndex < _lines.Count)
        {
            line = _lines[lineIndex];
            if (line.Index != lineIndex)
            {
                Logger.Error($"LineManager Line.Index != lineIndex: {line.Index} != {lineIndex}");
            }

            return true;
        }

        line = Line.None;
        return false;
    }

    private bool GetLineSub_(int lineIndex, int lineSubIndex, out LineSub lineSub)
    {
        lineSub = LineSub.None;
        if (false == GetLine(lineIndex, out Line line))
            return false;

        if (line.LineSubs.Count <= lineSubIndex)
            return false;

        lineSub = line.LineSubs[lineSubIndex];
        return true;
    }

    private bool GetLineSub_(Coordinates coordinates, out LineSub lineSub)
    {
        lineSub = LineSub.None;
        if (coordinates.LineSubIndex >= 0 &&
            GetLineSub(coordinates.LineIndex, coordinates.LineSubIndex, out lineSub))
        {
            return true;
        }

        return GetLine(coordinates.LineIndex, out Line line) && line.GetLineSub(coordinates.CharIndex, out lineSub);
    }

    private Line InsertLine_(int lineIndex)
    {
        Line newLine = TextBox.Ins.LinePool.Get();
        newLine.Index = lineIndex;

        _lines.Insert(lineIndex, newLine);

        // Update line index
        for (int i = lineIndex; i < _lines.Count; i++)
        {
            _lines[i].ChangeLineIndex(i);
        }

        SetLanguageTokenDirty();
        return newLine;
    }

    private void RemoveLine_(int removeLineIndex)
    {
        var deletedLine = _lines[removeLineIndex];
        _lines.RemoveAt(removeLineIndex);
        TextBox.Ins.LinePool.Return(deletedLine);

        // Update line index
        for (int i = removeLineIndex; i < _lines.Count; i++)
        {
            _lines[i].ChangeLineIndex(i);
        }
        SetLanguageTokenDirty();
    }

    private Stack<Language.Token> UpdateLanguageToken_(Stack<Language.Token> tokens)
    {
        SaveFolded_();
        _foldingList.Clear();

        int foldingTypeCount = TextBox.Ins.Language.FoldingStarts.Count;
        for (int i = 0; i < foldingTypeCount; i++)
        {
            if (_foldingStacks.ContainsKey(i) == false)
                _foldingStacks.TryAdd(i, new Stack<int>());
            _foldingStacks[i].Clear();
        }

        tokens.Clear();

        bool blockCommentOn = false;
        bool multilineStringOn = false;
        bool multilineStringNextLine = false;

        foreach (Line line in _lines)
        {
            line.CommentRanges.Clear();
            line.CommentStart = -1;
            line.StringRanges.Clear();
            line.StringStart = -1;

            int commentStart = blockCommentOn ? 0 : -1;
            int stringStart = (multilineStringOn || multilineStringNextLine) ? 0 : -1;
            multilineStringNextLine = false;

            foreach (Language.Token token in line.Tokens)
            {
                if (tokens.TryPeek(out Language.Token top))
                {
                    // If the previous token is not finished, ignore the token with continue
                    if (top.Type == Language.TokenType.BlockCommentStart)
                    {
                        if (token.Type != Language.TokenType.BlockCommentEnd ||
                            token.TokenIndex != top.TokenIndex)
                            continue;

                        tokens.Pop();
                        line.CommentRanges.Add(new ValueTuple<int, int>(commentStart,
                            token.CharIndex + token.TokenString.Length));
                        commentStart = -1;
                        blockCommentOn = false;
                        continue;
                    }

                    if (top.Type == Language.TokenType.MultilineString)
                    {
                        if (token.Type != Language.TokenType.MultilineString ||
                            token.TokenIndex != top.TokenIndex)
                            continue;

                        tokens.Pop();
                        line.StringRanges.Add(new ValueTuple<int, int>(stringStart,
                            token.CharIndex + token.TokenString.Length));
                        stringStart = -1;
                        multilineStringOn = false;
                        continue;
                    }

                    if (top.Type == Language.TokenType.String)
                    {
                        if (token.Type == Language.TokenType.String &&
                            token.TokenIndex == top.TokenIndex)
                        {
                            tokens.Pop();
                            line.StringRanges.Add(new ValueTuple<int, int>(stringStart,
                                token.CharIndex + token.TokenString.Length));
                            stringStart = -1;
                            multilineStringOn = false;
                        }
                        continue;
                    }

                    if (top.Type == Language.TokenType.LineComment)
                    {
                        continue;
                    }
                }

                tokens.Push(token);

                if (token.Type == Language.TokenType.LineComment)
                {
                    commentStart = token.CharIndex;
                }
                else if (token.Type == Language.TokenType.BlockCommentStart)
                {
                    commentStart = token.CharIndex;
                    blockCommentOn = true;
                }
                else if (token.Type == Language.TokenType.String)
                {
                    stringStart = token.CharIndex;
                    multilineStringOn = token.IsMultiline;
                }
                else if (token.Type == Language.TokenType.MultilineString)
                {
                    stringStart = token.CharIndex;
                    multilineStringOn = true;
                }
                else if (token.Type == Language.TokenType.FoldingStart)
                {
                    _foldingStacks[token.TokenIndex].Push(line.Index);
                }
                else if (token.Type == Language.TokenType.FoldingEnd)
                {
                    if (_foldingStacks[token.TokenIndex].TryPop(out int start))
                    {
                        int end = line.Index;
                        if (start < end)
                        {
                            _foldingList.Add(new Folding() { Start = start, End = end, Folded = false });
                        }
                    }
                }
            }

            // If there are tokens left until the end of the line, process them as comment or string until the end of the line
            if (commentStart >= 0)
                line.CommentStart = commentStart;

            if (stringStart >= 0)
                line.StringStart = stringStart;

            // Remove tokens that cannot cross lines
            while (tokens.TryPeek(out Language.Token lineTop))
            {
                if (lineTop.Type == Language.TokenType.LineComment)
                {
                    tokens.Pop();
                }
                else if (lineTop.Type == Language.TokenType.String)
                {
                    if (multilineStringOn)
                        break;
                    
                    if (line.EndsInMultilineString)
                    {
                        multilineStringNextLine = true;
                        break;
                    }
                    tokens.Pop();
                }
                else
                {
                    break;
                }
            }
            line.SetColorsDirty();
        }

        // Adding remaining foldings
        foreach (Stack<int> foldingStack in _foldingStacks.Values)
        {
            while (foldingStack.TryPop(out int start))
            {
                int end = Lines.Count - 1;
                if (start < end)
                {
                    _foldingList.Add(new Folding() { Start = start, End = end, Folded = false });
                }
            }
        }

        RestoreFolded_();
        RowManager.SetRowCacheDirty();
        return tokens;
    }

    private void ShiftFoldingLine_(int lineIndex, EditDirection direction)
    {
        int moveCount = EditDirection.Forward == direction ? 1 : -1;
        foreach (Folding folding in _foldingList)
        {
            if (lineIndex <= folding.Start)
            {
                folding.Start += moveCount;
            }

            if (lineIndex <= folding.End)
            {
                folding.End += moveCount;
            }
            
            //Logger.Debug("ShiftFoldingLine: " + folding.Start + " " + folding.End + " " + moveCount);
        }
    }

    private void Unfold_(int lineIndex)
    {
        foreach (Folding folding in _foldingList)
        {
            if (folding.Start <= lineIndex && lineIndex <= folding.End)
            {
                folding.Folded = false;
            }
        }
    }

    private void SaveFolded_()
    {
        _foldedList.Clear();
        foreach (Folding folding in _foldingList)
        {
            if (folding.Folded)
            {
                _foldedList.Add(folding);
            }
        }
    }

    private void RestoreFolded_()
    {
        foreach (Folding folding in _foldingList)
        {
            foreach (Folding folded in _foldedList)
            {
                if (folding.Start == folded.Start && folding.End == folded.End)
                {
                    folding.Folded = true;
                }
            }
        }
    }

    private float GetMaxLineWidth_()
    {
        float maxLineWidth = 0.0f;
        foreach (Line line in _lines)
        {
            maxLineWidth = Math.Max(maxLineWidth, line.Width);
        }

        return maxLineWidth;
    }

    public void Tick()
    {
        TextBox.Ins.LineManager._languageTokenCache.Get();
    }
}