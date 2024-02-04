using Bell.Data;

namespace Bell.Managers;

internal partial class ScrollManager
{
    internal static void Focus(int line) => TextBox.Ins.ScrollManager.Focus_(line, -1);
    internal static void Focus(int line, int lineSub) => TextBox.Ins.ScrollManager.Focus_(line, lineSub);
    
    internal static void ScrollTo(int line) => TextBox.Ins.ScrollManager.ScrollTo_(line, -1);
    internal static void ScrollTo(int line, int lineSub) => TextBox.Ins.ScrollManager.ScrollTo_(line, lineSub);
}

internal partial class ScrollManager : IManager
{
    private int _line = -1;
    private int _lineSub = -1;
    private bool _isScroll = false;
    private bool _isFocus = false;
    
    private void Focus_(int line, int lineSub)
    {
        if (line < 0)
            line = 0;
        
        if (line >= LineManager.Lines.Count)
            line = LineManager.Lines.Count - 1;
        
        _line = line;
        _lineSub = lineSub;
        _isFocus = true;
    }
    
    private void ScrollTo_(int line, int lineSub)
    {
        if (line < 0)
            line = 0;
        
        if (line >= LineManager.Lines.Count)
            line = LineManager.Lines.Count - 1;
        
        _line = line;
        _lineSub = lineSub;
        _isScroll = true;
    }

    public void Tick()
    {
        if (_isFocus || _isScroll)
        {
            float scrollY = -1;
            
            for (int i = 0; i <= RowManager.Rows.Count; i++)
            {
                if (RowManager.Rows.Count <= i || i < 0)
                    break;

                Row row = RowManager.Rows[i];
                if (null == row.LineSub)
                    continue;

                if (row.LineSub.Coordinates.LineIndex > _line)
                    break;

                if (row.LineSub.Coordinates.LineIndex == _line && _lineSub != -1)
                {
                    if (row.LineSub.Coordinates.LineSubIndex > _lineSub)
                        break;
                }
                
                scrollY = i * FontManager.GetLineHeight();
            }

            if (scrollY < 0)
            {
                _isFocus = false;
                _isScroll = false;
                return;
            }

            if (TextBox.Ins.ViewPos.Y < scrollY && scrollY < TextBox.Ins.ViewPos.Y + TextBox.Ins.ViewSize.Y)
            {
                _isFocus = false;
                _isScroll = false;
                return;
            }

            if (_isFocus)
            {
                if (TextBox.Ins.ViewPos.Y < scrollY)
                    scrollY -= TextBox.Ins.ViewSize.Y - FontManager.GetLineHeight() * 3;

                if (scrollY < TextBox.Ins.ViewPos.Y)
                    scrollY -= FontManager.GetLineHeight() * 3;
            }
                
            TextBox.Ins.Backend.SetScrollY(scrollY);
            _isFocus = false;
            _isScroll = false;
        }
    }
}

