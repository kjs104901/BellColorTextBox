using Bell.Inputs;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace Bell;

internal class WinFormBackend : IBackend
{
    private TextBoxControl _control;

    internal Vector2 PageSize;

    private KeyboardInput _keyboardInput = new() { Chars = new List<char>() };
    private MouseInput _mouseInput;

    private readonly Dictionary<Keys, HotKeys> _keyboardMapping = new()
    {
        { Keys.A, HotKeys.A },
        { Keys.C, HotKeys.C },
        { Keys.V, HotKeys.V },
        { Keys.X, HotKeys.X },
        { Keys.Y, HotKeys.Y },
        { Keys.Z, HotKeys.Z },

        { Keys.Up, HotKeys.UpArrow },
        { Keys.Down, HotKeys.DownArrow },
        { Keys.Left, HotKeys.LeftArrow },
        { Keys.Right, HotKeys.RightArrow },
        
        { Keys.PageUp, HotKeys.PageUp },
        { Keys.PageDown, HotKeys.PageDown },
        { Keys.Home, HotKeys.Home },
        { Keys.End, HotKeys.End },
        { Keys.Insert, HotKeys.Insert },
        
        { Keys.Delete, HotKeys.Delete },
        { Keys.Back, HotKeys.Backspace },
        { Keys.Enter, HotKeys.Enter },
        { Keys.Tab, HotKeys.Tab },
        
        { Keys.F3, HotKeys.F3 },
        { Keys.Escape, HotKeys.Escape },
    };


    public WinFormBackend(TextBoxControl control)
    {
        _control = control;
    }

    public float GetCharWidth(char c)
    {
        if (null == _control.Graphics || null == _control.Font)
            return 0.0f;

        var text = c.ToString();

        RectangleF rect = new RectangleF(0, 0, 0, 0);
        CharacterRange[] ranges = { new CharacterRange(0, 1) };

        StringFormat format = new StringFormat(StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap |
                                            StringFormatFlags.NoClip | StringFormatFlags.LineLimit);
        format.SetMeasurableCharacterRanges(ranges);

        Region[] regions = _control.Graphics.MeasureCharacterRanges(text, _control.Font, new(), format);
        rect = regions[0].GetBounds(_control.Graphics);

        return rect.Width;
    }

    public string GetClipboard()
    {
        return Clipboard.GetText(TextDataFormat.Text);
    }

    public void SetClipboard(string text)
    {
        Clipboard.SetText(text);
    }

    public float GetFontSize()
    {
        return _control.Font.Height;
    }

    public KeyboardInput GetKeyboardInput()
    {
        return _keyboardInput;
    }

    public MouseInput GetMouseInput()
    {
        return _mouseInput;
    }

    public float GetScrollX()
    {
        return 0.0f;
        //throw new NotImplementedException();
    }

    public void SetScrollX(float scrollX)
    {
        //throw new NotImplementedException();
    }

    public float GetScrollY()
    {
        return 0.0f;
        //throw new NotImplementedException();
    }

    public void SetScrollY(float scrollY)
    {
        //throw new NotImplementedException();
    }

    public void OnInputEnd()
    {
        _keyboardInput.HotKeys = HotKeys.None;
        _keyboardInput.Chars.Clear();
        _keyboardInput.ImeComposition = string.Empty;

        _mouseInput.Position.X = 0.0f;
        _mouseInput.Position.Y = 0.0f;
        _mouseInput.LeftAction = MouseAction.None;
        _mouseInput.MiddleAction = MouseAction.None;
    }

    public void RenderIcon(Vector2 pos, GuiIcon icon, Vector4 color, float ratio = 1)
    {
        //throw new NotImplementedException();
    }

    public void RenderLine(Vector2 start, Vector2 end, Vector4 color, float thickness)
    {
        //throw new NotImplementedException();
    }

    public void RenderPage(Vector2 size, Vector4 color)
    {
        if (null == _control.Graphics)
            return;

        PageSize = size;

        using SolidBrush brush = new SolidBrush(VectorToColor(color));

        _control.Graphics.FillRectangle(brush, new Rectangle(0, 0, (int)size.X, (int)size.Y));
    }

    public void RenderRectangle(Vector2 start, Vector2 end, Vector4 color)
    {
        if (null == _control.Graphics)
            return;

        using SolidBrush brush = new SolidBrush(VectorToColor(color));

        _control.Graphics.FillRectangle(brush, new Rectangle((int)start.X, (int)start.Y, (int)end.X - (int)start.X, (int)end.Y - (int)start.Y));
    }

    public void RenderText(Vector2 pos, string text, Vector4 color)
    {
        if (null == _control.Graphics)
            return;

        using SolidBrush brush = new SolidBrush(VectorToColor(color));
        _control.Graphics.DrawString(text, _control.Font, brush, pos.X, pos.Y);
    }

    public void SetMouseCursor(MouseCursor mouseCursor)
    {
        throw new NotImplementedException();
    }

    internal void OnKeyPress(KeyPressEventArgs e)
    {
        _keyboardInput.Chars.Add(e.KeyChar);
    }

    internal void ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_keyboardMapping.TryGetValue(keyData, out HotKeys hotKeys))
        {
            _keyboardInput.HotKeys |= hotKeys;
        }
    }


    private static Color VectorToColor(Vector4 color)
    {
        return Color.FromArgb((int)(color.W * 255), (int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }
}
