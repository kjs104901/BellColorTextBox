using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Bell.Inputs;
using ImGuiNET;

namespace Bell;

public class ImGuiBackend : IBackend
{
    private Vector2 _drawPosOnPage;

    private KeyboardInput _keyboardInput = new() { Chars = new List<char>() };
    private MouseInput _mouseInput;

    private readonly List<ValueTuple<ImGuiKey, HotKeys>> _keyboardMapping = new()
    {
        (ImGuiKey.A, HotKeys.A),
        (ImGuiKey.C, HotKeys.C),
        (ImGuiKey.V, HotKeys.V),
        (ImGuiKey.X, HotKeys.X),
        (ImGuiKey.Y, HotKeys.Y),
        (ImGuiKey.Z, HotKeys.Z),

        (ImGuiKey.UpArrow, HotKeys.UpArrow),
        (ImGuiKey.DownArrow, HotKeys.DownArrow),
        (ImGuiKey.LeftArrow, HotKeys.LeftArrow),
        (ImGuiKey.RightArrow, HotKeys.RightArrow),

        (ImGuiKey.PageUp, HotKeys.PageUp),
        (ImGuiKey.PageDown, HotKeys.PageDown),
        (ImGuiKey.Home, HotKeys.Home),
        (ImGuiKey.End, HotKeys.End),
        (ImGuiKey.Insert, HotKeys.Insert),

        (ImGuiKey.Delete, HotKeys.Delete),
        (ImGuiKey.Backspace, HotKeys.Backspace),
        (ImGuiKey.Enter, HotKeys.Enter),
        (ImGuiKey.Tab, HotKeys.Tab),

        (ImGuiKey.F3, HotKeys.F3),
        (ImGuiKey.Escape, HotKeys.Escape),
    };

    public KeyboardInput GetKeyboardInput() => _keyboardInput;
    public MouseInput GetMouseInput() => _mouseInput;

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

    public void RenderPage(Vector2 size, Vector4 color)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, color);
        ImGui.BeginChild("##Page", size, false, ImGuiWindowFlags.NoScrollbar);

        _drawPosOnPage = ImGui.GetCursorScreenPos();

        if (ImGui.IsWindowFocused() ||
            (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(0)))
        {
            var io = ImGui.GetIO();
            if (io.KeyShift)
                _keyboardInput.HotKeys |= HotKeys.Shift;
            if (io.KeyCtrl)
                _keyboardInput.HotKeys |= HotKeys.Ctrl;
            if (io.KeyAlt)
                _keyboardInput.HotKeys |= HotKeys.Alt;
            foreach (var keyMap in _keyboardMapping)
            {
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(keyMap.Item1)))
                    _keyboardInput.HotKeys |= keyMap.Item2;
            }

            _keyboardInput.Chars.Clear();
            if (io.InputQueueCharacters.Size > 0)
            {
                _keyboardInput.Chars = new List<char>();
                for (int i = 0; i < io.InputQueueCharacters.Size; i++)
                {
                    _keyboardInput.Chars.Add((char)io.InputQueueCharacters[i]);
                }
            }

            if (OperatingSystem.IsWindows())
            {
                _keyboardInput.ImeComposition = NativeMethods.GetCompositionString();
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                _mouseInput.LeftAction = MouseAction.Click;
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                _mouseInput.LeftAction = MouseAction.DoubleClick;
            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                _mouseInput.LeftAction = MouseAction.Dragging;

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Middle))
                _mouseInput.MiddleAction = MouseAction.Click;
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Middle))
                _mouseInput.MiddleAction = MouseAction.DoubleClick;
            if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle) && ImGui.IsMouseDown(ImGuiMouseButton.Middle))
                _mouseInput.MiddleAction = MouseAction.Dragging;
        }

        var mouse = ImGui.GetMousePos();
        _mouseInput.Position.X = mouse.X - _drawPosOnPage.X;
        _mouseInput.Position.Y = mouse.Y - _drawPosOnPage.Y;
        
        ImGui.EndChild();
        ImGui.PopStyleColor();
    }

    public void RenderText(Vector2 pos, string text, Vector4 color)
    {
        ImGui.GetWindowDrawList().AddText(new Vector2(_drawPosOnPage.X + pos.X, _drawPosOnPage.Y + pos.Y),
            ImGui.ColorConvertFloat4ToU32(color), text);
    }

    public void RenderIcon(Vector2 pos, GuiIcon icon, Vector4 color, float ratio = 1.0f)
    {
        char iconChar = ' ';
        string iconString = string.Empty; // To avoid garbage collection
        switch (icon)
        {
            case GuiIcon.Fold:
                iconChar = '\uf105'; // angle-right
                iconString = "\uf105";
                break;
            case GuiIcon.Unfold:
                iconChar = '\uf107'; //angle-down
                iconString = "\uf107";
                break;
            case GuiIcon.Ellipsis:
                iconChar = '\uf141'; //ellipsis
                iconString = "\uf141";
                break;
            case GuiIcon.Tab:
                iconChar = '\uf178'; //arrow-right-long
                iconString = "\uf178";
                break;
        }
        
        float fontSize = ImGui.GetFont().FontSize;

        float iconSize = fontSize * 3.0f / 4.0f * ratio;
        float iconWidth = ImGuiTextBox.FontAwesome.GetCharAdvance(iconChar) * ratio;

        float posX = _drawPosOnPage.X + pos.X - (iconWidth / 2.0f);
        float posY = _drawPosOnPage.Y + pos.Y - (iconSize / 2.0f);

        ImGui.GetWindowDrawList().AddText(ImGuiTextBox.FontAwesome, iconSize, new Vector2(posX, posY),
            ImGui.ColorConvertFloat4ToU32(color), iconString);
    }

    public void RenderLine(Vector2 start, Vector2 end, Vector4 color, float thickness)
    {
        var startPos = new Vector2(_drawPosOnPage.X + start.X, _drawPosOnPage.Y + start.Y);
        var endPos = new Vector2(_drawPosOnPage.X + end.X, _drawPosOnPage.Y + end.Y);

        ImGui.GetWindowDrawList().AddLine(startPos, endPos, ImGui.ColorConvertFloat4ToU32(color), thickness);
    }

    public void RenderRectangle(Vector2 start, Vector2 end, Vector4 color)
    {
        var startPos = new Vector2(_drawPosOnPage.X + start.X, _drawPosOnPage.Y + start.Y);
        var endPos = new Vector2(_drawPosOnPage.X + end.X, _drawPosOnPage.Y + end.Y);

        ImGui.GetWindowDrawList().AddRectFilled(startPos, endPos, ImGui.ColorConvertFloat4ToU32(color));
    }

    public void SetClipboard(string text)
    {
        ImGui.SetClipboardText(text);
    }

    public string GetClipboard()
    {
        return ImGui.GetClipboardText();
    }

    public void SetScrollX(float scrollX)
    {
        ImGui.SetScrollX(scrollX);
    }

    public float GetScrollX()
    {
        return ImGui.GetScrollX();
    }

    public void SetScrollY(float scrollY)
    {
        ImGui.SetScrollY(scrollY);
    }

    public float GetScrollY()
    {
        return ImGui.GetScrollY();
    }

    public float GetCharWidth(char c)
    {
        return ImGui.GetFont().GetCharAdvance(c);
    }

    public float GetFontSize()
    {
        return ImGui.GetFont().FontSize;
    }

    public void SetMouseCursor(MouseCursor mouseCursor)
    {
        if (OperatingSystem.IsWindows())
        {
            switch (mouseCursor)
            {
                case MouseCursor.Arrow:
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Arrow);
                    NativeMethods.SetCursor(NativeMethods.ArrowCursor);
                    break;
                case MouseCursor.Beam:
                    ImGui.SetMouseCursor(ImGuiMouseCursor.TextInput);
                    NativeMethods.SetCursor(NativeMethods.BeamCursor);
                    break;
                case MouseCursor.Hand:
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    NativeMethods.SetCursor(NativeMethods.HandCursor);
                    break;
            }
        }
    }
}