using Bell.Inputs;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace Bell;

internal class WinFormBackend : IBackend
{
    private TextBoxControl _control;
    internal Graphics? Graphics;

    internal Vector2 PageSize;

    internal KeyboardInput KeyboardInput = new() { Chars = new List<char>() };
    internal MouseInput MouseInput;

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
        if (null == Graphics || null == _control.Font)
            return 0.0f;

        var text = c.ToString();

        RectangleF rect = new RectangleF(0, 0, 0, 0);
        CharacterRange[] ranges = { new CharacterRange(0, 1) };

        StringFormat format = new StringFormat(StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap |
                                            StringFormatFlags.NoClip | StringFormatFlags.LineLimit);
        format.SetMeasurableCharacterRanges(ranges);

        Region[] regions = Graphics.MeasureCharacterRanges(text, _control.Font, new(), format);
        rect = regions[0].GetBounds(Graphics);

        return rect.Width;
    }

    /*
    public float GetCharWidth(char c)
    {
        Size sz2 = TextRenderer.MeasureText("<" + c.ToString() + ">", _control.Font);
        Size sz3 = TextRenderer.MeasureText("<>", _control.Font);
        return sz2.Width - sz3.Width + 1;
    }
    */

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
        return KeyboardInput;
    }

    public MouseInput GetMouseInput()
    {
        return MouseInput;
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
        KeyboardInput.HotKeys = HotKeys.None;
        KeyboardInput.Chars.Clear();

        MouseInput.LeftAction = MouseAction.None;
        MouseInput.MiddleAction = MouseAction.None;
    }

    public void RenderIcon(Vector2 pos, GuiIcon icon, Vector4 color, float ratio = 1)
    {
        //throw new NotImplementedException();
    }

    public void RenderLine(Vector2 start, Vector2 end, Vector4 color, float thickness)
    {
        if (null == Graphics)
            return;

        using SolidBrush brush = new SolidBrush(VectorToColor(color));
        
        PointF startPoint = GetDrawPoint(start);
        PointF endPoint = GetDrawPoint(end);

        Graphics.DrawLine(new Pen(brush, thickness), startPoint, endPoint);
    }

    public void RenderPage(Vector2 size, Vector4 color)
    {
        if (null == Graphics)
            return;

        PageSize = size;

        using SolidBrush brush = new SolidBrush(VectorToColor(color));

        Graphics.FillRectangle(brush, new RectangleF(0, 0, size.X, size.Y));
    }

    public void RenderRectangle(Vector2 start, Vector2 end, Vector4 color)
    {
        if (null == Graphics)
            return;

        using SolidBrush brush = new SolidBrush(VectorToColor(color));

        PointF startPoint = GetDrawPoint(start);
        PointF endPoint = GetDrawPoint(end);
        
        Graphics.FillRectangle(brush, new RectangleF(startPoint.X, startPoint.Y, endPoint.X - startPoint.X, endPoint.Y - startPoint.Y));
        
        Graphics.FillRectangle(new SolidBrush(Color.Brown), (int)startPoint.X, (int)startPoint.Y, (int)endPoint.X - (int)startPoint.X, (int)endPoint.Y - (int)startPoint.Y);
    }

    public void RenderText(Vector2 pos, string text, Vector4 color)
    {
        if (null == Graphics)
            return;

        using SolidBrush brush = new SolidBrush(VectorToColor(color));
        Graphics.DrawString(text, _control.Font, brush, GetDrawPoint(pos));
    }

    public void SetMouseCursor(MouseCursor mouseCursor)
    {
    }

    internal void OnKeyPress(KeyPressEventArgs e)
    {
        KeyboardInput.Chars.Add(e.KeyChar);
    }

    internal void ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_keyboardMapping.TryGetValue(keyData, out HotKeys hotKeys))
        {
            KeyboardInput.HotKeys |= hotKeys;
        }
    }
    
    internal void OnImeComposition(string text)
    {
        Console.WriteLine(text);
        KeyboardInput.ImeComposition = text;
    }
    
    private static Color VectorToColor(Vector4 color)
    {
        return Color.FromArgb((int)(color.W * 255), (int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }
    
    private PointF GetDrawPoint(Vector2 pos)
    {
        return new PointF(pos.X - _control.HorizontalScroll.Value, pos.Y - _control.VerticalScroll.Value);
    }
}
