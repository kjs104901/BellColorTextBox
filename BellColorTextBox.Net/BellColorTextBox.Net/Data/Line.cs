﻿using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Bell.Languages;
using Bell.Managers;
using Bell.Themes;
using Bell.Utils;

namespace Bell.Data;

internal class Line : IReusable
{
    internal int Index;

    internal int CharsCount => _chars.Count;

    private readonly List<char> _chars = new();

    internal Folding Folding = Folding.None;

    internal float Width = 0.0f;

    internal string String => _stringCache.Get();
    private readonly Cache<string> _stringCache;

    private List<ColorStyle> Colors => _colorsCache.Get();
    private readonly Cache<List<ColorStyle>> _colorsCache;
    private List<ColorStyle> _colors = new();

    private HashSet<int> Cutoffs => _cutoffsCache.Get();
    private readonly Cache<HashSet<int>> _cutoffsCache;

    internal List<LineSub> LineSubs => _lineSubsCache.Get();
    private readonly Cache<List<LineSub>> _lineSubsCache;

    private readonly List<Language.Token> _oldTokens = new();
    internal readonly List<Language.Token> Tokens = new();
    private bool _OldEndsInMultilineString = false;
    internal bool EndsInMultilineString = false;

    internal readonly List<ValueTuple<int, int>> CommentRanges = new();
    internal int CommentStart = -1;
    internal readonly List<ValueTuple<int, int>> StringRanges = new();
    internal int StringStart = -1;

    // buffer to avoid GC
    private readonly StringBuilder _sb = new();

    internal static readonly Line None = new() { Index = -1 };

    public Line()
    {
        _colorsCache = new("Colors", _colors, UpdateColors, updateIntervalMs: 300);
        _cutoffsCache = new("Cutoffs", new(), UpdateCutoff);
        _stringCache = new("String", string.Empty, UpdateString);
        _lineSubsCache = new("Line Subs", new List<LineSub>(), UpdateLineSubs);
    }

    internal void ChangeLineIndex(int newIndex)
    {
        if (Index == newIndex)
            return;

        Index = newIndex;
        foreach (LineSub lineSub in LineSubs)
        {
            lineSub.Coordinates.LineIndex = newIndex;
        }
    }

    internal void SetText(string text)
    {
        _chars.Clear();
        _chars.AddRange(text);
        UpdateTokens();
    }

    internal void InsertChars(int charIndex, char[] chars)
    {
        _chars.InsertRange(charIndex, chars);

        ColorStyle prevStyle = ColorStyle.None;
        int prevIndex = charIndex - 1;
        if (prevIndex >= 0 && _colors.Count > prevIndex)
            prevStyle = _colors[prevIndex];

        while (_colors.Count < charIndex)
            _colors.Add(prevStyle);

        for (int i = 0; i < chars.Length; i++)
        {
            _colors.Insert(charIndex, prevStyle);
        }

        SetCharsDirty();
        SetCutoffsDirty();
        UpdateTokens();

        LineManager.Unfold(Index);
        
        if (WrapMode.None != TextBox.Ins.WrapMode)
            RowManager.SetRowCacheDirty();
        else
            RowManager.SetRowSelectionDirty();
    }

    internal char[] RemoveChars(int charIndex, int count)
    {
        var removed = _chars.GetRange(charIndex, count).ToArray();
        _chars.RemoveRange(charIndex, count);

        while (_colors.Count < charIndex + count)
            _colors.Add(ColorStyle.None);

        _colors.RemoveRange(charIndex, count);

        SetCharsDirty();
        SetCutoffsDirty();
        UpdateTokens();

        LineManager.Unfold(Index);

        if (WrapMode.None != TextBox.Ins.WrapMode)
            RowManager.SetRowCacheDirty();
        else
            RowManager.SetRowSelectionDirty();
        
        return removed;
    }

    internal void SetCharsDirty()
    {
        _colorsCache.SetDirty();
        _stringCache.SetDirty();
    }

    internal void SetColorsDirty()
    {
        _colorsCache.SetDirty();
    }

    internal void SetCutoffsDirty()
    {
        _cutoffsCache.SetDirty();
        _lineSubsCache.SetDirty();
    }

    private List<ColorStyle> UpdateColors(List<ColorStyle> colors)
    {
        colors.Clear();
        if (TextBox.Ins.SyntaxHighlight == false ||
            _chars.Count > TextBox.SyntaxGiveUpThreshold)
        {
            return colors;
        }

        for (int i = 0; i < _chars.Count; i++)
        {
            colors.Add(ColorStyle.None);
        }

        string text = String;
        foreach (var kv in TextBox.Ins.Language.PatternsStyle)
        {
            Regex regex = kv.Key;
            ColorStyle colorStyle = TextBox.Ins.Theme.TokenColors[kv.Value];

            foreach (Match match in regex.Matches(text))
            {
                for (int i = match.Index; i < match.Index + match.Length && i < colors.Count; i++)
                {
                    colors[i] = colorStyle;
                }
            }
        }

        foreach (var range in CommentRanges)
        {
            for (int i = range.Item1; i < range.Item2 && i < colors.Count; i++)
            {
                colors[i] = TextBox.Ins.Theme.TokenColors[Theme.Token.Comment];
            }
        }

        if (CommentStart >= 0)
        {
            for (int i = CommentStart; i < colors.Count; i++)
            {
                colors[i] = TextBox.Ins.Theme.TokenColors[Theme.Token.Comment];
            }
        }

        foreach (var range in StringRanges)
        {
            for (int i = range.Item1; i < range.Item2 && i < colors.Count; i++)
            {
                colors[i] = TextBox.Ins.Theme.TokenColors[Theme.Token.String];
            }
        }

        if (StringStart >= 0)
        {
            for (int i = StringStart; i < colors.Count; i++)
            {
                colors[i] = TextBox.Ins.Theme.TokenColors[Theme.Token.String];
            }
        }
        
        return colors;
    }

    private HashSet<int> UpdateCutoff(HashSet<int> cutoffs)
    {
        cutoffs.Clear();
        if (WrapMode.None == TextBox.Ins.WrapMode)
            return cutoffs;

        var lineWidth = TextBox.Ins.PageSize.X - TextBox.Ins.LineNumberWidth - TextBox.Ins.FoldWidth;
        if (lineWidth < 1.0f)
            return cutoffs;

        float widthAccumulated = 0.0f;

        for (int i = 0; i < _chars.Count; i++)
        {
            widthAccumulated += FontManager.GetFontWidth(_chars[i]);
            if (widthAccumulated + FontManager.GetFontReferenceWidth() > lineWidth)
            {
                if (TextBox.Ins.WrapMode == WrapMode.BreakWord)
                {
                    cutoffs.Add(i);
                    widthAccumulated = GetIndentWidth();
                }
                else if (TextBox.Ins.WrapMode == WrapMode.Word)
                {
                    // go back to the start of word
                    float backWidth = 0.0f;
                    while (i > 0)
                    {
                        if (char.IsWhiteSpace(_chars[i]))
                            break;

                        backWidth += FontManager.GetFontWidth(_chars[i]);
                        if (backWidth + FontManager.GetFontReferenceWidth() * 10 > lineWidth)
                            break; // Give up on word wrap. break word.

                        i--;
                    }

                    cutoffs.Add(i);
                    widthAccumulated = GetIndentWidth();
                }
            }
        }

        return cutoffs;
    }

    private string UpdateString(string _)
    {
        _sb.Clear();
        _sb.Append(CollectionsMarshal.AsSpan(_chars));
        return _sb.ToString();
    }

    private List<LineSub> UpdateLineSubs(List<LineSub> lineSubs)
    {
        TextBox.Ins.LineSubPool.Return(lineSubs);
        lineSubs.Clear();
        Width = 0.0f;

        int lineSubIndex = 0;
        LineSub lineSub = TextBox.Ins.LineSubPool.Get();
        lineSub.Coordinates = new Coordinates(Index, 0, lineSubIndex);
        lineSub.IndentWidth = 0.0f;
        lineSub.Width = 0.0f;

        float currentX = 0.0f;
        for (int i = 0; i < _chars.Count; i++)
        {
            char c = _chars[i];
            float cWidth = FontManager.GetFontWidth(c);
            if (c == '\t' && TextBox.Ins.TabMode == TabMode.Tab)
            {
                float posX = ((int)((lineSub.IndentWidth + currentX + cWidth) / FontManager.GetFontTabWidth())) *
                             FontManager.GetFontTabWidth();
                cWidth = posX - lineSub.IndentWidth - currentX;
                if (cWidth < FontManager.GetFontWhiteSpaceWidth())
                    cWidth += FontManager.GetFontTabWidth();
            }

            lineSub.Chars.Add(c);
            lineSub.CharWidths.Add(cWidth);
            lineSub.Width += cWidth;
            currentX += cWidth;

            if (Cutoffs.Contains(i)) // need new line
            {
                lineSubs.Add(lineSub);
                Width = Math.Max(Width, lineSub.Width);

                lineSubIndex++;
                lineSub = TextBox.Ins.LineSubPool.Get();
                lineSub.Coordinates = new Coordinates(Index, i + 1, lineSubIndex);
                lineSub.IndentWidth = GetIndentWidth();
                lineSub.Width = GetIndentWidth();
            }
        }

        lineSubs.Add(lineSub);
        Width = Math.Max(Width, lineSub.Width);
        return lineSubs;
    }

    private void UpdateTokens()
    {
        Language language = TextBox.Ins.Language;
        
        Tokens.Clear();
        EndsInMultilineString = false;
        
        string text = String;
        for (int i = 0; i < text.Length; i++)
        {
            if (language.FindMatching(text, i, out Language.Token matchedToken))
            {
                Tokens.Add(matchedToken);
                i += matchedToken.TokenString.Length;
                i--; // Ignore i++ in for loop
            }
        }

        if (false == _oldTokens.SequenceEqual(Tokens))
        {
            _oldTokens.Clear();
            _oldTokens.AddRange(Tokens);
            LineManager.SetLanguageTokenDirty();
        }
        
        foreach (string postfix in language.MultilinePostfixes)
        {
            if (!text.EndsWith(postfix, StringComparison.Ordinal))
                continue;
            
            EndsInMultilineString = true;
            break;
        }

        if (_OldEndsInMultilineString != EndsInMultilineString)
        {
            _OldEndsInMultilineString = EndsInMultilineString;
            LineManager.SetLanguageTokenDirty();
        }
    }

    internal bool GetLineSub(int charIndex, out LineSub foundLineSub)
    {
        foundLineSub = LineSub.None;

        foreach (LineSub lineSub in LineSubs)
        {
            if (lineSub.Coordinates.CharIndex <= charIndex &&
                charIndex <= lineSub.Coordinates.CharIndex + lineSub.Chars.Count + 1)
            {
                foundLineSub = lineSub;
                return true;
            }
        }

        Logger.Error($"GetLineSub failed to find. LineIndex: {Index}, charIndex: {charIndex}");
        foundLineSub = LineSubs[0];
        return false;
    }

    private float GetIndentWidth()
    {
        if (TextBox.Ins.WordWrapIndent)
            return TextBox.Ins.CountTabStart(String) * FontManager.GetFontTabWidth();
        return 0.0f;
    }

    internal ColorStyle GetColorStyle(int charIndex)
    {
        ColorStyle charColor = TextBox.Ins.Theme.Foreground;
        if (false == TextBox.Ins.SyntaxHighlight)
            return charColor;
        
        if (Colors.Count > charIndex)
            charColor = Colors[charIndex];
        if (charColor == ColorStyle.None)
            charColor = TextBox.Ins.Theme.Foreground;
        return charColor;
    }

    public bool GetWordStart(int charIndex, out int search)
    {
        search = charIndex;
        if (charIndex < 0 || charIndex >= _chars.Count)
            return false;

        while (search >= 0)
        {
            if (false == char.IsLetter(_chars[search]) && false == char.IsNumber(_chars[search]) && '_' != _chars[search])
                break;
            search--;
        }

        search++;
        return true;
    }

    public bool GetWordEnd(int charIndex, out int search)
    {
        search = charIndex;
        if (charIndex < 0 || charIndex >= _chars.Count)
            return false;

        while (search < _chars.Count)
        {
            if (false == char.IsLetter(_chars[search]) && false == char.IsNumber(_chars[search]) && '_' != _chars[search])
                break;
            search++;
        }

        return true;
    }

    public string GetSubString(int startCharIndex, int endCharIndex)
    {
        if (startCharIndex > endCharIndex)
            return string.Empty;

        if (startCharIndex < 0)
            startCharIndex = 0;

        if (endCharIndex >= _chars.Count)
            endCharIndex = _chars.Count - 1;

        return String.Substring(startCharIndex, endCharIndex - startCharIndex + 1);
    }

    public void Reset()
    {
        Index = 0;
        
        _chars.Clear();
        Folding = Folding.None;
        Width = 0.0f;
        
        _stringCache.SetDirty();
        _colorsCache.SetDirty();
        _cutoffsCache.SetDirty();
        _lineSubsCache.SetDirty();
    
        _oldTokens.Clear();
        Tokens.Clear();
        
        CommentRanges.Clear();
        CommentStart = -1;
        StringRanges.Clear();
        StringStart = -1;
    
        _sb.Clear();
    }
}