using System.Numerics;
using Bell.Inputs;

namespace Bell;

public enum GuiIcon
{
    Fold,
    Unfold,
    Ellipsis
}

public interface IBackend
{
    public KeyboardInput GetKeyboardInput();
    public MouseInput GetMouseInput();
    public void OnInputEnd();
    
    public void RenderPage(Vector2 size, Vector4 color);
    public void RenderText(Vector2 pos, string text, Vector4 color);
    public void RenderIcon(Vector2 pos, GuiIcon icon, Vector4 color);
    public void RenderLine(Vector2 start, Vector2 end, Vector4 color, float thickness);
    public void RenderRectangle(Vector2 start, Vector2 end, Vector4 color);

    public void SetClipboard(string text);
    public string GetClipboard();
    
    public void SetScrollX(float scrollX);
    public float GetScrollX();
    public void SetScrollY(float scrollY);
    public float GetScrollY();

    public float GetCharWidth(char c);
    public float GetFontSize();
    
    public void SetMouseCursor(MouseCursor mouseCursor);
}