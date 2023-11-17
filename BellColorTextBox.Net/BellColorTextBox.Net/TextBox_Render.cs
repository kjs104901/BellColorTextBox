using System.Diagnostics;
using System.Numerics;
using Bell.Data;
using Bell.Languages;
using Bell.Themes;
using Bell.Utils;

namespace Bell;

public partial class TextBox
{
    internal float LineNumberWidth = 10.0f;
    internal float FoldWidth = 10.0f;

    internal readonly Stopwatch CaretBlinkStopwatch = new();

    internal int LinesPerPage => (int)(_viewSize.Y / FontManager.GetLineHeight());

    public void Render(Vector2 viewPos, Vector2 viewSize)
    {
        Singleton.TextBox = this;
        ProcessInput(viewPos, viewSize);

        FontManager.UpdateReferenceSize();
        FoldWidth = FontManager.GetFontReferenceWidth() * 2;

        Vector2 renderPageSize = PageSize;
        if (renderPageSize.X < _viewSize.X)
            renderPageSize.X = _viewSize.X;
        if (renderPageSize.Y < _viewSize.Y)
            renderPageSize.Y = _viewSize.Y;
        Backend.RenderPage(renderPageSize, ReadOnly ? Theme.BackgroundDimmed.ToVector() : Theme.Background.ToVector());

        LineNumberWidth = (StringPool<int>.Get(LineManager.Lines.Count).Length + 1) * FontManager.GetFontNumberWidth();

        LineManager.UpdateLanguageToken();

        int rowStart = GetRowIndex(_viewPos, -3);
        int rowEnd = GetRowIndex(_viewPos + _viewSize, 3);

        for (int i = rowStart; i <= rowEnd; i++)
        {
            if (RowManager.Rows.Count <= i || i < 0)
                break;

            Row row = RowManager.Rows[i];

            var lineY = i * FontManager.GetLineHeight();
            var lineTextStartY = lineY + (int)FontManager.GetLineHeightOffset();
            var lineEndY = lineY + FontManager.GetLineHeight();
            var lineTextEndY = lineY + FontManager.GetLineHeight() - (int)FontManager.GetLineHeightOffset();

            var lineStartX = LineNumberWidth + FoldWidth + row.LineSub!.IndentWidth;

            if (row.RowSelection.Selected)
            {
                Backend.RenderRectangle(new Vector2(lineStartX + row.RowSelection.SelectionStart, lineTextStartY),
                    new Vector2(lineStartX + row.RowSelection.SelectionEnd, lineTextEndY),
                    Theme.BackgroundSelection.ToVector());
            }

            if (LineManager.GetLine(row.LineSub.Coordinates.LineIndex, out Line line))
            {
                float currPosX = 0.0f;

                for (int j = 0; j < row.LineSub.Chars.Count; j++)
                {
                    char rowChar = row.LineSub.Chars[j];
                    int rowCharIndex = row.LineSub.Coordinates.CharIndex + j;
                    float rowCharWidth = row.LineSub.CharWidths[j];

                    ColorStyle charColor = line.GetColorStyle(rowCharIndex);

                    foreach (Coordinates caretPosition in row.RowSelection.CaretPositions)
                    {
                        if (rowCharIndex == caretPosition.CharIndex)
                            DrawImeComposition(lineStartX, lineTextStartY, lineTextEndY, ref currPosX);
                    }

                    Backend.RenderText(new Vector2(lineStartX + currPosX, lineTextStartY),
                        StringPool<char>.Get(rowChar), charColor.ToVector());

                    if (ShowingWhitespace)
                    {
                        if (rowChar == ' ')
                        {
                            Backend.RenderText(
                                new Vector2(lineStartX + currPosX, lineTextStartY),
                                "·", Theme.ForegroundDimmed.ToVector());
                        }
                        else if (rowChar == '\t')
                        {
                            Backend.RenderLine(
                                new Vector2(
                                    lineStartX + currPosX,
                                    lineY + FontManager.GetLineHeight() / 2.0f),
                                new Vector2(
                                    lineStartX + currPosX + rowCharWidth - FontManager.GetFontWhiteSpaceWidth(),
                                    lineY + FontManager.GetLineHeight() / 2.0f),
                                Theme.ForegroundDimmed.ToVector(), 1.0f);
                        }
                    }

                    currPosX += rowCharWidth;
                }

                // When the Caret is end of chars
                foreach (Coordinates caretPosition in row.RowSelection.CaretPositions)
                {
                    if (row.LineSub.Chars.Count == caretPosition.CharIndex)
                        DrawImeComposition(lineStartX, lineTextStartY, lineTextEndY, ref currPosX);
                }

                if (row.LineSub.Coordinates.LineSubIndex == 0)
                {
                    string lineIndex = StringPool<int>.Get(line.Index + 1);

                    float lineIndexWidth = 0.0f;
                    foreach (char lineChar in lineIndex)
                    {
                        lineIndexWidth += FontManager.GetFontWidth(lineChar);
                    }

                    var lineIndexColor = CaretManager.IsLineHasCaret(line.Index)
                        ? Theme.Foreground
                        : Theme.ForegroundDimmed;

                    Backend.RenderText(new Vector2(LineNumberWidth - lineIndexWidth, lineTextStartY),
                        lineIndex,
                        lineIndexColor.ToVector());
                }

                if (SyntaxFolding && Folding.None != line.Folding && row.LineSub.Coordinates.LineSubIndex == 0)
                {
                    if (line.Folding.Folded)
                    {
                        Backend.RenderIcon(
                            new Vector2(LineNumberWidth + FoldWidth / 2.0f, lineY + (FontManager.GetLineHeight() / 2.0f)),
                            GuiIcon.Fold,
                            Theme.Foreground.ToVector());

                        Backend.RenderIcon(
                            new Vector2(lineStartX + currPosX + FontManager.GetFontWhiteSpaceWidth(), lineY + (FontManager.GetLineHeight() / 2.0f)),
                            GuiIcon.Ellipsis,
                            Theme.Foreground.ToVector());
                    }
                    else
                    {
                        Backend.RenderIcon(
                            new Vector2(LineNumberWidth + FoldWidth / 2.0f, lineY + (FontManager.GetLineHeight() / 2.0f)),
                            GuiIcon.Unfold,
                            Theme.Foreground.ToVector());
                    }
                }
            }

            // TODO: Need auto scroll
            /*
            if (CaretManager.Count == 1 &&
                CaretManager.GetFirstCaret(out var firstCaret) &&
                firstCaret.Position.LineIndex == line.Index &&
                LineManager.GetLineSub(firstCaret.Position, out LineSub scrollLineSub))
            {
                float startPosition = scrollLineSub.GetCharPosition(firstCaret.Position) - _viewPos.X;
                
                if (startPosition < 0.0f)
                {
                    Backend.SetScrollX(startPosition);
                }
                else if (startPosition > _viewSize.X)
                {
                    Backend.SetScrollX(startPosition - _viewSize.X);
                }
            }
            */

            if (CaretBlinkStopwatch.ElapsedMilliseconds < 1000 ||
                CaretBlinkStopwatch.ElapsedMilliseconds % 1000 < 500)
            {
                foreach (Coordinates caretPosition in row.RowSelection.CaretPositions)
                {
                    float caretX = row.LineSub.GetCharPosition(caretPosition);

                    string currString = " ";
                    if (_imeComposition is { Length: > 0 } && false == ReadOnly)
                    {
                        currString = _imeComposition;
                    }
                    else
                    {
                        if (row.LineSub.Chars.Count > caretPosition.CharIndex)
                            currString = StringPool<char>.Get(row.LineSub.Chars[caretPosition.CharIndex]);
                    }

                    if (Overwrite)
                    {
                        Backend.RenderRectangle(
                            new Vector2(lineStartX + caretX - 1.0f, lineTextStartY),
                            new Vector2(lineStartX + caretX - 1.0f + FontManager.GetFontWidth(currString), lineTextEndY),
                            Theme.Foreground.ToVector());
                        Backend.RenderText(new Vector2(lineStartX + caretX - 1.0f, lineTextStartY),
                            currString, Theme.Background.ToVector());
                    }
                    else
                    {
                        Backend.RenderLine(
                            new Vector2(lineStartX + caretX - 1.0f, lineTextStartY),
                            new Vector2(lineStartX + caretX - 1.0f, lineTextEndY),
                            Theme.Foreground.ToVector(),
                            2.0f);
                    }
                }
            }
        }
    }

    private void DrawImeComposition(float lineStartX, float lineTextStartY, float lineTextEndY, ref float currPosX)
    {
        if (_imeComposition is not { Length: > 0 } || ReadOnly)
            return;

        Backend.RenderText(new Vector2(lineStartX + currPosX, lineTextStartY), _imeComposition,
            Theme.Foreground.ToVector());

        float compositionWidth = FontManager.GetFontWidth(_imeComposition);

        Backend.RenderLine(new Vector2(lineStartX + currPosX, lineTextEndY),
            new Vector2(lineStartX + currPosX + compositionWidth, lineTextEndY),
            Theme.Foreground.ToVector(), 1.0f);

        currPosX += compositionWidth;
    }
}