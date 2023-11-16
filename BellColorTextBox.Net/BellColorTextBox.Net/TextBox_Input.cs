using System.Numerics;
using Bell.Actions;
using Bell.Data;
using Bell.Inputs;
using Bell.Utils;

namespace Bell;

public partial class TextBox
{
    private Vector2 _viewPos;
    private Vector2 _viewSize;

    internal Vector2 PageSize;

    private Vector2 _mouseDragStart;

    private bool _shiftPressed;
    private bool _altPressed;

    private string _imeComposition = "";

    private const float ScrollSpeed = 10.0f;
    private const float RenderPadding = 10.0f;

    private void ProcessInput(Vector2 viewPos, Vector2 viewSize)
    {
        ProcessMouseInput();
        ProcessKeyboardInput();
        ProcessViewInput(viewPos, viewSize);
        Backend.OnInputEnd();
    }

    private void ProcessKeyboardInput()
    {
        KeyboardInput keyboardInput = Backend.GetKeyboardInput();

        var hk = keyboardInput.HotKeys;

        _shiftPressed |= EnumFlag.Has(hk, HotKeys.Shift);
        _altPressed |= EnumFlag.Has(hk, HotKeys.Alt);

        // Chars
        foreach (char keyboardInputChar in keyboardInput.Chars)
        {
            if (keyboardInputChar == 0)
                continue;

            if (keyboardInputChar == '\n')
            {
                ActionManager.DoAction(new EnterAction());
                continue;
            }

            if (keyboardInputChar == '\t')
            {
                if (EnumFlag.Has(hk, HotKeys.Shift))
                    ActionManager.DoAction(new UnTabAction());
                else
                    ActionManager.DoAction(new TabAction());
                continue;
            }

            if (keyboardInputChar < 32)
                continue;

            if (CaretManager.HasCaretsSelection())
            {
                ActionManager.DoAction(new DeleteSelectionAction());
                CaretManager.RemoveCaretsSelection();
            }

            ActionManager.DoAction(new InputCharAction(EditDirection.Forward, new[] { keyboardInputChar }));
        }

        // IME
        if (false == string.IsNullOrEmpty(keyboardInput.ImeComposition))
        {
            if (CaretManager.HasCaretsSelection())
            {
                ActionManager.DoAction(new DeleteSelectionAction());
                CaretManager.RemoveCaretsSelection();
            }
        }

        if (_imeComposition != keyboardInput.ImeComposition)
        {
            _imeComposition = keyboardInput.ImeComposition;
            return;
        }

        if (EnumFlag.Has(hk, HotKeys.Ctrl | HotKeys.Shift | HotKeys.Z)) // RedoAction
            ActionManager.RedoAction();
        else if (EnumFlag.Has(hk, HotKeys.Ctrl | HotKeys.Z)) // UndoAction
            ActionManager.UndoAction();
        else if (EnumFlag.Has(hk, HotKeys.Ctrl | HotKeys.Y)) // RedoAction
            ActionManager.RedoAction();
        else if (EnumFlag.Has(hk, HotKeys.Ctrl | HotKeys.C)) // Copy
            CaretManager.CopyClipboard();
        else if (EnumFlag.Has(hk, HotKeys.Ctrl | HotKeys.V)) // Paste
            CaretManager.PasteClipboard();
        else if (EnumFlag.Has(hk, HotKeys.Ctrl | HotKeys.X)) // Cut
        {
            CaretManager.CopyClipboard();
            if (CaretManager.HasCaretsSelection())
            {
                ActionManager.DoAction(new DeleteSelectionAction());
                CaretManager.RemoveCaretsSelection();
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.Ctrl | HotKeys.A)) // Select All
        {
            CaretManager.MoveCaretsAnchor(CaretMove.StartOfFile);
            CaretManager.MoveCaretsPosition(CaretMove.EndOfFile);
        }
        else if (EnumFlag.Has(hk, HotKeys.Delete)) // Delete
        {
            if (EnumFlag.Has(hk, HotKeys.Shift))
            {
                CaretManager.MoveCaretsAnchor(CaretMove.StartOfFile);
                CaretManager.MoveCaretsPosition(CaretMove.EndOfFile);
                if (CaretManager.HasCaretsSelection())
                {
                    ActionManager.DoAction(new DeleteSelectionAction());
                    CaretManager.RemoveCaretsSelection();
                }
            }
            else
            {
                if (CaretManager.HasCaretsSelection())
                {
                    ActionManager.DoAction(new DeleteSelectionAction());
                    CaretManager.RemoveCaretsSelection();
                }
                else
                {
                    ActionManager.DoAction(new DeleteCharAction(EditDirection.Forward));
                }
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.Backspace)) // Backspace
        {
            if (EnumFlag.Has(hk, HotKeys.Alt))
            {
                ActionManager.UndoAction();
            }
            else
            {
                if (CaretManager.HasCaretsSelection())
                {
                    ActionManager.DoAction(new DeleteSelectionAction());
                    CaretManager.RemoveCaretsSelection();
                }
                else
                {
                    ActionManager.DoAction(new DeleteCharAction(EditDirection.Backward));
                }
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.Enter)) // Enter
        {
            if (CaretManager.HasCaretsSelection())
            {
                ActionManager.DoAction(new DeleteSelectionAction());
                CaretManager.RemoveCaretsSelection();
            }

            ActionManager.DoAction(new EnterAction());
        }
        else if (EnumFlag.Has(hk, HotKeys.Tab)) // Tab
        {
            if (EnumFlag.Has(hk, HotKeys.Shift))
                ActionManager.DoAction(new UnTabAction());
            else
                ActionManager.DoAction(new TabAction());
        }
        else if (EnumFlag.Has(hk, HotKeys.UpArrow)) // Move Up
        {
            if (EnumFlag.Has(hk, HotKeys.Shift))
            {
                CaretManager.MoveCaretsPosition(CaretMove.Up);
            }
            else
            {
                CaretManager.MoveCaretsPosition(CaretMove.Up);
                CaretManager.RemoveCaretsSelection();
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.DownArrow)) // Move Down
        {
            if (EnumFlag.Has(hk, HotKeys.Shift))
            {
                CaretManager.MoveCaretsPosition(CaretMove.Down);
            }
            else
            {
                CaretManager.MoveCaretsPosition(CaretMove.Down);
                CaretManager.RemoveCaretsSelection();
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.LeftArrow)) // Move Left
        {
            if (EnumFlag.Has(hk, HotKeys.Shift))
            {
                CaretManager.MoveCaretsPosition(CaretMove.Left);
            }
            else
            {
                CaretManager.MoveCaretsPosition(CaretMove.Left);
                CaretManager.RemoveCaretsSelection();
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.RightArrow)) // Move Right
        {
            if (EnumFlag.Has(hk, HotKeys.Shift))
            {
                CaretManager.MoveCaretsPosition(CaretMove.Right);
            }
            else
            {
                CaretManager.MoveCaretsPosition(CaretMove.Right);
                CaretManager.RemoveCaretsSelection();
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.PageUp)) // Move PageUp
        {
            if (EnumFlag.Has(hk, HotKeys.Shift))
            {
                CaretManager.MoveCaretsPosition(CaretMove.PageUp);
            }
            else
            {
                CaretManager.MoveCaretsPosition(CaretMove.PageUp);
                CaretManager.RemoveCaretsSelection();
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.PageDown)) // Move PageDown
        {
            if (EnumFlag.Has(hk, HotKeys.Shift))
            {
                CaretManager.MoveCaretsPosition(CaretMove.PageDown);
            }
            else
            {
                CaretManager.MoveCaretsPosition(CaretMove.PageDown);
                CaretManager.RemoveCaretsSelection();
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.Home))
        {
            if (EnumFlag.Has(hk, HotKeys.Shift))
            {
                CaretManager.MoveCaretsPosition(
                    EnumFlag.Has(hk, HotKeys.Ctrl) ? CaretMove.StartOfFile : CaretMove.StartOfLine);
            }
            else
            {
                CaretManager.MoveCaretsPosition(
                    EnumFlag.Has(hk, HotKeys.Ctrl) ? CaretMove.StartOfFile : CaretMove.StartOfLine);

                CaretManager.RemoveCaretsSelection();
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.End))
        {
            if (EnumFlag.Has(hk, HotKeys.Shift))
            {
                CaretManager.MoveCaretsPosition(
                    EnumFlag.Has(hk, HotKeys.Ctrl) ? CaretMove.EndOfFile : CaretMove.EndOfLine);
            }
            else
            {
                CaretManager.MoveCaretsPosition(
                    EnumFlag.Has(hk, HotKeys.Ctrl) ? CaretMove.EndOfFile : CaretMove.EndOfLine);

                CaretManager.RemoveCaretsSelection();
            }
        }
        else if (EnumFlag.Has(hk, HotKeys.Insert))
        {
            Overwrite = !Overwrite;
        }
    }

    private void ProcessMouseInput()
    {
        MouseInput mouseInput = Backend.GetMouseInput();
        if (false == string.IsNullOrEmpty(_imeComposition))
            return;

        ConvertCoordinates(mouseInput.Position,
            out Coordinates coordinates,
            out bool isLineNumber,
            out bool isFold);

        if (MouseAction.Click == mouseInput.LeftAction ||
            MouseAction.DoubleClick == mouseInput.LeftAction)
        {
            if (isFold)
            {
                if (LineManager.GetLine(coordinates.LineIndex, out Line line))
                {
                    if (SyntaxFolding && Folding.None != line.Folding)
                    {
                        line.Folding.Switch();
                        RowManager.SetRowCacheDirty();
                        return;
                    }
                }
            }

            if (isLineNumber)
                return;
        }

        if (mouseInput.Position.X > _viewPos.X && mouseInput.Position.X < _viewPos.X + _viewSize.X &&
            mouseInput.Position.Y > _viewPos.Y && mouseInput.Position.Y < _viewPos.Y + _viewSize.Y)
        {
            if (isFold)
            {
                Backend.SetMouseCursor(MouseCursor.Hand);
            }
            else if (isLineNumber)
            {
            }
            else
            {
                Backend.SetMouseCursor(MouseCursor.Beam);
            }
        }

        if (MouseAction.Click == mouseInput.LeftAction ||
            MouseAction.Click == mouseInput.MiddleAction)
        {
            _mouseDragStart = mouseInput.Position;
        }

        if (MouseAction.Click == mouseInput.LeftAction)
        {
            if (_shiftPressed)
            {
                if (CaretManager.GetFirstCaret(out Caret caret))
                {
                    caret.AnchorPosition = coordinates;
                    RowManager.SetRowCacheDirty();
                }
            }
            else if (_altPressed)
            {
                CaretManager.AddCaret(coordinates);
                RowManager.SetRowCacheDirty();
            }
            else
            {
                CaretManager.ClearCarets();
                CaretManager.AddCaret(coordinates);
                RowManager.SetRowCacheDirty();
            }
        }
        else if (MouseAction.DoubleClick == mouseInput.LeftAction)
        {
            CaretManager.ClearCarets();
            CaretManager.AddCaret(coordinates);
            RowManager.SetRowCacheDirty();

            if (_shiftPressed)
            {
                // Select Line
                CaretManager.MoveCaretsPosition(CaretMove.EndOfLine);
                CaretManager.MoveCaretsAnchor(CaretMove.StartOfLine);
            }
            else
            {
                // Select word
                CaretManager.MoveCaretsPosition(CaretMove.EndOfWord);
                CaretManager.MoveCaretsAnchor(CaretMove.StartOfWord);
            }
        }
        else if (MouseAction.Dragging == mouseInput.LeftAction)
        {
            if (_altPressed)
            {
                ConvertCoordinatesRange(_mouseDragStart, mouseInput.Position,
                    out List<(Coordinates, Coordinates)> ranges,
                    out bool isReversed);

                CaretManager.SelectRectangle(ranges, isReversed);
            }
            else
            {
                if (CaretManager.GetFirstCaret(out Caret caret))
                {
                    caret.Position = coordinates;
                    RowManager.SetRowCacheDirty();
                }
            }
        }
        else if (MouseAction.Dragging == mouseInput.MiddleAction)
        {
            ConvertCoordinatesRange(_mouseDragStart, mouseInput.Position,
                out List<(Coordinates, Coordinates)> ranges,
                out bool isReversed);

            CaretManager.SelectRectangle(ranges, isReversed);
        }

        if (MouseAction.Dragging == mouseInput.LeftAction ||
            MouseAction.Dragging == mouseInput.MiddleAction)
        {
            if (mouseInput.Position.Y - _viewPos.Y < 0)
                Backend.SetScrollY(Backend.GetScrollY() - ScrollSpeed);

            else if (mouseInput.Position.Y - _viewPos.Y > _viewSize.Y)
                Backend.SetScrollY(Backend.GetScrollY() + ScrollSpeed);

            if (mouseInput.Position.X - _viewPos.X < 0)
                Backend.SetScrollX(Backend.GetScrollX() - ScrollSpeed);

            else if (mouseInput.Position.X - _viewPos.X > _viewSize.X)
                Backend.SetScrollX(Backend.GetScrollX() + ScrollSpeed);
        }

        _shiftPressed = false;
        _altPressed = false;
    }

    private void ProcessViewInput(Vector2 viewPos, Vector2 viewSize)
    {
        if (MathHelper.IsNotSame(viewPos.X, _viewPos.X) ||
            MathHelper.IsNotSame(viewPos.Y, _viewPos.Y) ||
            MathHelper.IsNotSame(viewSize.X, _viewSize.X) ||
            MathHelper.IsNotSame(viewSize.Y, _viewSize.Y))
        {
            _viewPos = viewPos;
            _viewSize = viewSize;

            foreach (Line line in LineManager.Lines)
            {
                line.SetCutoffsDirty();
            }

            CaretManager.RemoveCaretsLineSub();
            RowManager.SetRowCacheDirty();
        }

        if (WrapMode.Word == WrapMode || WrapMode.BreakWord == WrapMode)
        {
            PageSize.X = _viewSize.X;
        }
        else
        {
            PageSize.X = LineManager.GetMaxLineWidth() + LineNumberWidth + FoldWidth + RenderPadding;
        }
        PageSize.Y = RowManager.Rows.Count * FontManager.GetLineHeight() + RenderPadding;
    }

    private int GetRowIndex(Vector2 viewCoordinates, int yOffset = 0)
    {
        float y = viewCoordinates.Y;

        int rowIndex = (int)(y / FontManager.GetLineHeight()) + yOffset;
        if (rowIndex < 0)
            rowIndex = 0;
        if (rowIndex >= RowManager.Rows.Count)
            rowIndex = RowManager.Rows.Count - 1;

        return rowIndex;
    }

    private void ConvertCoordinates(Vector2 viewCoordinates,
        out Coordinates coordinates,
        out bool isLineNumber,
        out bool isFold)
    {
        int rowIndex = GetRowIndex(viewCoordinates);
        coordinates = new Coordinates(0, 0);
        isLineNumber = false;
        isFold = false;

        float x = viewCoordinates.X - LineNumberWidth - FoldWidth;
        if (x < -FoldWidth)
        {
            isLineNumber = true;
            return;
        }
        
        if (rowIndex < 0)
            return;

        if (RowManager.Rows.Count > rowIndex)
        {
            Row row = RowManager.Rows[rowIndex];
            if (null != row.LineSub && LineManager.GetLine(row.LineSub.Coordinates.LineIndex, out Line line))
            {
                coordinates.LineIndex = line.Index;

                if (x < -FontManager.GetFontWhiteSpaceWidth())
                {
                    if (SyntaxFolding && Folding.None != line.Folding && row.LineSub.Coordinates.LineSubIndex == 0)
                    {
                        isFold = true;
                        return;
                    }
                }

                coordinates.CharIndex =
                    row.LineSub.Coordinates.CharIndex +
                    row.LineSub.GetCharIndex(x - row.LineSub.IndentWidth);

                coordinates.LineSubIndex = row.LineSub.Coordinates.LineSubIndex;
            }
        }
    }

    private void ConvertCoordinatesRange(Vector2 a, Vector2 b,
        out List<ValueTuple<Coordinates, Coordinates>> ranges,
        out bool isReversed)
    {
        ranges = new List<(Coordinates, Coordinates)>();

        isReversed = b.X < a.X;
        Vector2 from = new Vector2() { X = Math.Min(a.X, b.X), Y = Math.Min(a.Y, b.Y) };
        Vector2 to = new Vector2() { X = Math.Max(a.X, b.X), Y = Math.Max(a.Y, b.Y) };

        int fromRow = GetRowIndex(from);
        int toRow = GetRowIndex(to);

        float fromX = from.X - LineNumberWidth - FoldWidth;
        float toX = to.X - LineNumberWidth - FoldWidth;

        for (int i = fromRow; i <= toRow; i++)
        {
            if (RowManager.Rows.Count <= i || i < 0)
                break;

            Row row = RowManager.Rows[i];
            if (null != row.LineSub && LineManager.GetLine(row.LineSub.Coordinates.LineIndex, out Line line))
            {
                int lineIndex = line.Index;
                int lineSubIndex = row.LineSub.Coordinates.LineSubIndex;

                int fromCharIndex =
                    row.LineSub.Coordinates.CharIndex +
                    row.LineSub.GetCharIndex(fromX - row.LineSub.IndentWidth);
                int toCharIndex =
                    row.LineSub.Coordinates.CharIndex +
                    row.LineSub.GetCharIndex(toX - row.LineSub.IndentWidth);

                ranges.Add(new(
                    new Coordinates() { LineIndex = lineIndex, LineSubIndex = lineSubIndex, CharIndex = fromCharIndex },
                    new Coordinates() { LineIndex = lineIndex, LineSubIndex = lineSubIndex, CharIndex = toCharIndex }
                ));
            }
        }
    }
}